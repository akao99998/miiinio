using System.Collections.Generic;
using Kampai.Common;
using Kampai.Game.Transaction;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game
{
	public class TransactionEngine
	{
		private ExchangeRate exchangeRate;

		private IKampaiLogger logger { get; set; }

		private IDefinitionService defService { get; set; }

		private IRandomService randomService { get; set; }

		private IPlayerService playerService { get; set; }

		public TransactionEngine(IKampaiLogger myLogger, IDefinitionService myDefs, IRandomService myRand, IPlayerService myPlayer)
		{
			logger = myLogger;
			defService = myDefs;
			randomService = myRand;
			playerService = myPlayer;
			exchangeRate = new ExchangeRate(myDefs);
		}

		public bool Perform(Player player, TransactionDefinition transaction, TransactionArg arg = null)
		{
			IList<Instance> newItems;
			return Perform(player, transaction, out newItems, arg);
		}

		public bool Perform(Player player, TransactionDefinition transaction, out IList<Instance> newItems, TransactionArg arg = null)
		{
			newItems = null;
			if (player != null && transaction != null)
			{
				if (ValidateInputs(player, transaction) && ValidateOutputs(transaction, arg))
				{
					if (transaction.Inputs != null)
					{
						foreach (QuantityItem input in transaction.Inputs)
						{
							player.AlterQuantityByDefId(input.ID, (int)(0L - (long)input.Quantity));
						}
					}
					if (transaction.Outputs != null)
					{
						newItems = new List<Instance>();
						foreach (QuantityItem output in transaction.Outputs)
						{
							int iD = output.ID;
							Definition definition = defService.Get(iD);
							Instance instance = null;
							if (definition is ItemDefinition)
							{
								instance = ProcessItemOutput(player, output, definition);
							}
							else if (definition is BuildingDefinition)
							{
								instance = AddBuildingItem(player, definition, arg, output.Quantity);
							}
							else if (definition is MinionDefinition)
							{
								instance = new Minion(definition as MinionDefinition);
							}
							else if (definition is WeightedDefinition)
							{
								QuantityItem quantityItem = player.GetWeightedInstance(iD).NextPick(randomService);
								instance = player.AlterQuantityByDefId(quantityItem.ID, (int)quantityItem.Quantity);
							}
							else if (definition is LandExpansionConfig)
							{
								LandExpansionConfig expansionConfig = definition as LandExpansionConfig;
								player.AddLandExpansion(expansionConfig);
							}
							else if (definition is CompositeBuildingPieceDefinition)
							{
								AddCompositePieceAndPossiblyBuilding(player, newItems, (CompositeBuildingPieceDefinition)definition);
							}
							else if (definition is StickerDefinition)
							{
								Sticker sticker = new Sticker(definition as StickerDefinition);
								sticker.UTCTimeEarned = arg.TransactionUTCTime;
								instance = sticker;
							}
							else if (definition is AchievementDefinition)
							{
								Achievement firstInstanceByDefinitionId = player.GetFirstInstanceByDefinitionId<Achievement>(iD);
								int quantity = (int)output.Quantity;
								if (firstInstanceByDefinitionId == null)
								{
									firstInstanceByDefinitionId = new Achievement(definition as AchievementDefinition);
									firstInstanceByDefinitionId.Progress += quantity;
									instance = firstInstanceByDefinitionId;
								}
								else
								{
									firstInstanceByDefinitionId.Progress += quantity;
								}
							}
							else
							{
								LogError(FatalCode.TE_UNKNOWN_OUTPUT, transaction, "Unknown output type");
							}
							if (arg != null)
							{
								foreach (ItemAccumulator accumulator in arg.GetAccumulators())
								{
									accumulator.AwardOutput(output);
								}
							}
							if (instance != null)
							{
								AssignIDAndAddToReturnList(player, newItems, instance);
							}
						}
					}
					return true;
				}
				return false;
			}
			logger.FatalNullArgument(FatalCode.TE_NULL_ARG);
			return true;
		}

		private Instance ProcessItemOutput(Player player, QuantityItem qi, Definition d)
		{
			int iD = d.ID;
			if (d is UnlockDefinition)
			{
				player.AddUnlockedItems(qi);
			}
			else
			{
				switch (iD)
				{
				case 5:
				{
					QuantityInstance quantityInstance = new QuantityInstance();
					quantityInstance.ID = qi.ID;
					quantityInstance.Quantity = qi.Quantity;
					return quantityInstance;
				}
				case 2:
					playerService.AddXP((int)qi.Quantity);
					break;
				case 21:
				case 700:
				case 701:
				case 702:
					return null;
				case 6:
				case 9:
				{
					uint quantityByDefinitionId = player.GetQuantityByDefinitionId(iD);
					if (qi.Quantity > quantityByDefinitionId)
					{
						return player.AlterQuantityByDefId(iD, (int)(qi.Quantity - quantityByDefinitionId));
					}
					break;
				}
				default:
					return player.AlterQuantityByDefId(iD, (int)qi.Quantity);
				}
			}
			return null;
		}

		private void AddCompositePieceAndPossiblyBuilding(Player player, IList<Instance> newItems, CompositeBuildingPieceDefinition pieceDefinition)
		{
			CompositeBuildingPiece compositeBuildingPiece = new CompositeBuildingPiece(pieceDefinition);
			AssignIDAndAddToReturnList(player, newItems, compositeBuildingPiece);
			CompositeBuilding compositeBuilding = player.GetFirstInstanceByDefinitionId<CompositeBuilding>(pieceDefinition.BuildingDefinitionID);
			if (compositeBuilding == null)
			{
				CompositeBuildingDefinition definition = defService.Get<CompositeBuildingDefinition>(pieceDefinition.BuildingDefinitionID);
				Instance instance = AddBuildingItem(player, definition, null, 1u);
				AssignIDAndAddToReturnList(player, newItems, instance);
				compositeBuilding = (CompositeBuilding)instance;
			}
			compositeBuilding.AttachedCompositePieceIDs.Add(compositeBuildingPiece.ID);
		}

		private void AssignIDAndAddToReturnList(Player player, IList<Instance> newItems, Instance newItem)
		{
			if (!(newItem is Item) && !(newItem is QuantityInstance))
			{
				player.AssignNextInstanceId(newItem);
				player.Add(newItem);
			}
			newItems.Add(newItem);
		}

		private Instance AddBuildingItem(Player player, Definition definition, TransactionArg arg, uint count = 1)
		{
			BuildingDefinition buildingDefinition = definition as BuildingDefinition;
			Building building = buildingDefinition.BuildBuilding();
			if (arg != null)
			{
				Location location = arg.Get<Location>();
				if (location != null)
				{
					building.Location = location;
				}
				else
				{
					building.SetState(BuildingState.Inventory);
				}
			}
			else
			{
				building.SetState(BuildingState.Inventory);
			}
			for (int i = 1; i < count; i++)
			{
				Building building2 = buildingDefinition.BuildBuilding();
				building2.SetState(BuildingState.Inventory);
				player.AssignNextInstanceId(building2);
				player.Add(building2);
			}
			return building;
		}

		public bool SubtractInputs(Player player, TransactionDefinition transaction)
		{
			if (player != null && transaction != null)
			{
				if (ValidateInputs(player, transaction))
				{
					if (transaction.Inputs != null)
					{
						foreach (QuantityItem input in transaction.Inputs)
						{
							player.AlterQuantityByDefId(input.ID, (int)(0L - (long)input.Quantity));
						}
					}
					return true;
				}
				return false;
			}
			logger.FatalNullArgument(FatalCode.TE_NULL_ARG);
			return false;
		}

		public IList<QuantityItem> GetRequiredItems(Player player, TransactionDefinition transaction)
		{
			if (player != null && transaction != null && transaction.Inputs != null)
			{
				IList<QuantityItem> inputs = transaction.Inputs;
				if (inputs != null)
				{
					IList<QuantityItem> list = new List<QuantityItem>();
					{
						foreach (QuantityItem item2 in inputs)
						{
							uint quantityByDefinitionId = player.GetQuantityByDefinitionId(item2.ID);
							uint quantity = item2.Quantity;
							if (quantityByDefinitionId < quantity)
							{
								QuantityItem item = new QuantityItem(item2.ID, quantity - quantityByDefinitionId);
								list.Add(item);
							}
						}
						return list;
					}
				}
			}
			logger.FatalNullArgument(FatalCode.TE_NULL_ARG);
			return null;
		}

		public bool AddOutputs(Player player, IList<QuantityItem> items)
		{
			if (player != null && items != null)
			{
				foreach (QuantityItem item in items)
				{
					int iD = item.ID;
					Definition definition = defService.Get(iD);
					Instance instance = null;
					if (definition is ItemDefinition)
					{
						player.AlterQuantityByDefId(iD, (int)item.Quantity);
					}
					else if (definition is WeightedDefinition)
					{
						QuantityItem quantityItem = player.GetWeightedInstance(iD).NextPick(randomService);
						player.AlterQuantityByDefId(quantityItem.ID, (int)quantityItem.Quantity);
					}
					else
					{
						if (!(definition is MinionDefinition))
						{
							LogError(FatalCode.TE_UNKNOWN_OUTPUT, definition, "Unknown output type");
							return false;
						}
						instance = new Minion(definition as MinionDefinition);
					}
					if (instance != null)
					{
						player.AssignNextInstanceId(instance);
						player.Add(instance);
					}
				}
				return true;
			}
			logger.FatalNullArgument(FatalCode.TE_NULL_ARG);
			return false;
		}

		public bool AddOutputs(Player player, TransactionDefinition transaction, TransactionArg arg = null)
		{
			IList<Instance> newItems;
			return AddOutputs(player, transaction, out newItems, arg);
		}

		public bool AddOutputs(Player player, TransactionDefinition transaction, out IList<Instance> newItems, TransactionArg arg = null)
		{
			newItems = null;
			if (player != null && transaction != null)
			{
				if (ValidateOutputs(transaction, arg))
				{
					if (transaction.Outputs != null)
					{
						newItems = new List<Instance>();
						foreach (QuantityItem output in transaction.Outputs)
						{
							int iD = output.ID;
							Definition definition = defService.Get(iD);
							Instance instance = null;
							if (definition is ItemDefinition)
							{
								if (definition.ID == 2)
								{
									playerService.AddXP((int)output.Quantity);
								}
								else
								{
									player.AlterQuantityByDefId(iD, (int)output.Quantity);
								}
							}
							else if (definition is BuildingDefinition)
							{
								instance = AddBuildingItem(player, definition, arg, output.Quantity);
							}
							else if (definition is WeightedDefinition)
							{
								QuantityItem quantityItem = player.GetWeightedInstance(iD).NextPick(randomService);
								player.AlterQuantityByDefId(quantityItem.ID, (int)quantityItem.Quantity);
							}
							else if (definition is MinionDefinition)
							{
								instance = new Minion(definition as MinionDefinition);
							}
							else
							{
								LogError(FatalCode.TE_UNKNOWN_OUTPUT, transaction, "Unknown output type");
							}
							if (arg != null)
							{
								foreach (ItemAccumulator accumulator in arg.GetAccumulators())
								{
									accumulator.AwardOutput(output);
								}
							}
							if (instance != null)
							{
								AssignIDAndAddToReturnList(player, newItems, instance);
							}
						}
					}
					return true;
				}
				return false;
			}
			logger.FatalNullArgument(FatalCode.TE_NULL_ARG);
			return false;
		}

		public bool ValidateOutputs(TransactionDefinition transaction, TransactionArg arg)
		{
			if (transaction != null)
			{
				if (transaction.Outputs != null)
				{
					foreach (QuantityItem output in transaction.Outputs)
					{
						if (!ValidateOutput(transaction, output, arg))
						{
							return false;
						}
					}
				}
				return true;
			}
			logger.FatalNullArgument(FatalCode.TE_NULL_ARG);
			return false;
		}

		private bool ValidateOutput(TransactionDefinition transaction, Identifiable qi, TransactionArg arg = null)
		{
			int iD = qi.ID;
			Definition definition = defService.Get(iD);
			if (!(definition is ItemDefinition) && !(definition is MinionDefinition) && !(definition is LandExpansionConfig) && !(definition is CompositeBuildingPieceDefinition) && !(definition is StickerDefinition) && !(definition is BuildingDefinition) && !(definition is AchievementDefinition))
			{
				if (!(definition is WeightedDefinition))
				{
					LogError(FatalCode.TE_UNKNOWN_OUTPUT, transaction, "Unknown output type");
					return false;
				}
				foreach (WeightedQuantityItem entity in ((WeightedDefinition)definition).Entities)
				{
					if (!ValidateOutput(transaction, entity, arg))
					{
						return false;
					}
				}
			}
			return true;
		}

		public bool ValidateInputs(Player player, TransactionDefinition transaction)
		{
			if (player != null && transaction != null)
			{
				if (transaction.Inputs != null)
				{
					IList<QuantityItem> inputs = transaction.Inputs;
					if (inputs != null)
					{
						List<int> list = new List<int>(inputs.Count);
						foreach (QuantityItem item in inputs)
						{
							uint quantityByDefinitionId = player.GetQuantityByDefinitionId(item.ID);
							uint quantity = item.Quantity;
							if (quantityByDefinitionId < quantity)
							{
								return false;
							}
							if (list.Contains(item.ID))
							{
								LogError(FatalCode.TE_INCONSISTENT_DEF_MODEL, transaction, "Transaction has duplicate inputs ID={0} T={1}", item.ID, transaction.ID);
								return false;
							}
							list.Add(item.ID);
						}
					}
				}
				return true;
			}
			logger.FatalNullArgument(FatalCode.TE_NULL_ARG);
			return false;
		}

		public int CalculateRushCost(Player player, IList<QuantityItem> items)
		{
			int num = 0;
			if (player != null && items != null)
			{
				foreach (QuantityItem item in items)
				{
					ItemDefinition itemDefinition = defService.Get<ItemDefinition>(item.ID);
					int num2 = Mathf.FloorToInt(itemDefinition.BasePremiumCost * (float)item.Quantity);
					num2 = ((num2 == 0 && item.Quantity != 0) ? 1 : num2);
					num += num2;
				}
				return num;
			}
			logger.FatalNullArgument(FatalCode.TE_NULL_ARG);
			return num;
		}

		private void LogError(FatalCode code, object t, string message, params object[] args)
		{
			logger.Fatal(code, "{0} : {1}", t.ToString(), string.Format(message, args));
		}

		public int RequiredPremiumForGrind(int desiredGrind)
		{
			return exchangeRate.GrindToPremium(desiredGrind);
		}

		public int PremiumToGrind(int premium)
		{
			return exchangeRate.PremiumToGrind(premium);
		}
	}
}
