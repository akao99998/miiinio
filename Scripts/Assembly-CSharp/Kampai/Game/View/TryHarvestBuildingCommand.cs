using System;
using System.Collections.Generic;
using Kampai.Game.Transaction;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game.View
{
	public class TryHarvestBuildingCommand : Command
	{
		[Inject]
		public BuildingObject buildingObj { get; set; }

		[Inject]
		public Action callback { get; set; }

		[Inject]
		public bool sentFromUI { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public SpawnDooberSignal spawnDooberSignal { get; set; }

		[Inject]
		public BuildingHarvestSignal buildingHarvestSignal { get; set; }

		[Inject]
		public UpdateResourceIconCountSignal updateResourceIconCountSignal { get; set; }

		[Inject]
		public BuildingChangeStateSignal buildingChangeStateSignal { get; set; }

		[Inject]
		public MIBBuildingResourceIconSelectedSignal mibBuildingResourceIconSelectedSignal { get; set; }

		[Inject]
		public OpenStorageBuildingSignal openStorageBuildingSignal { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject]
		public VillainLairModel lairModel { get; set; }

		public override void Execute()
		{
			if (playerService.HasStorageBuilding())
			{
				Building byInstanceId = playerService.GetByInstanceId<Building>(buildingObj.ID);
				if (!TryHarvestMIB(byInstanceId) && !TryHarvestTaskable(byInstanceId) && !TryHarvestVillainLairPortal(byInstanceId) && !TryHarvestVillainLairResource(byInstanceId))
				{
					TryHarvestCrafting(byInstanceId);
				}
			}
		}

		private bool TryHarvestMIB(Building building)
		{
			MIBBuilding mIBBuilding = building as MIBBuilding;
			if (mIBBuilding == null)
			{
				return false;
			}
			mibBuildingResourceIconSelectedSignal.Dispatch();
			return true;
		}

		private bool TryHarvestTaskable(Building building)
		{
			TaskableBuilding taskableBuilding = building as TaskableBuilding;
			if (taskableBuilding == null)
			{
				return false;
			}
			ResourceBuilding resourceBuilding = taskableBuilding as ResourceBuilding;
			if (resourceBuilding != null && resourceBuilding.BonusMinionItems.Count > 0 && !sentFromUI)
			{
				return RewardBonusItems(resourceBuilding);
			}
			if (taskableBuilding.GetNumCompleteMinions() > 0 || (resourceBuilding != null && resourceBuilding.AvailableHarvest > 0))
			{
				int iD = taskableBuilding.ID;
				Vector3 type = new Vector3(taskableBuilding.Location.x, 0f, taskableBuilding.Location.y);
				bool flag = false;
				int transactionID = taskableBuilding.GetTransactionID(definitionService);
				if (playerService.FinishTransaction(transactionID, TransactionTarget.HARVEST, new TransactionArg(iD)))
				{
					int harvestItemDefinitionIdFromTransactionId = definitionService.GetHarvestItemDefinitionIdFromTransactionId(transactionID);
					spawnDooberSignal.Dispatch(type, DestinationType.STORAGE, harvestItemDefinitionIdFromTransactionId, true);
					buildingManager.GetComponent<BuildingManagerMediator>().LastHarvestedBuildingID = iD;
					buildingManager.GetComponent<BuildingManagerMediator>().HarvestTimer = 1f;
					if (resourceBuilding == null)
					{
						if (taskableBuilding.GetMinionsInBuilding() <= 0)
						{
							return false;
						}
						int minionByIndex = taskableBuilding.GetMinionByIndex(0);
						playerService.GetByInstanceId<Minion>(minionByIndex).AlreadyRushed = false;
					}
					buildingHarvestSignal.Dispatch(iD);
					callback();
					int newHarvestAvailable = GetNewHarvestAvailable(resourceBuilding, taskableBuilding);
					updateResourceIconCountSignal.Dispatch(new Tuple<int, int>(iD, harvestItemDefinitionIdFromTransactionId), newHarvestAvailable);
					buildingObj.Bounce();
					IQuestService obj = questService;
					int item = harvestItemDefinitionIdFromTransactionId;
					obj.UpdateAllQuestsWithQuestStepType(QuestStepType.Harvest, QuestTaskTransition.Complete, null, 0, item);
				}
				return true;
			}
			return false;
		}

		private void OpenStorage()
		{
			StorageBuilding firstInstanceByDefintion = playerService.GetFirstInstanceByDefintion<StorageBuilding, StorageBuildingDefinition>();
			openStorageBuildingSignal.Dispatch(firstInstanceByDefintion, true);
		}

		private bool TryHarvestVillainLairResource(Building building)
		{
			VillainLairResourcePlot villainLairResourcePlot = building as VillainLairResourcePlot;
			if (villainLairResourcePlot == null)
			{
				return false;
			}
			if (villainLairResourcePlot.BonusMinionItems.Count > 0 && !sentFromUI)
			{
				return RewardBonusItems(villainLairResourcePlot);
			}
			if (villainLairResourcePlot.MinionIsTaskedToBuilding() || villainLairResourcePlot.UTCLastTaskingTimeStarted == 0)
			{
				return true;
			}
			bool flag = false;
			int resourceItemID = villainLairResourcePlot.parentLair.Definition.ResourceItemID;
			IngredientsItemDefinition ingredientsItemDefinition = definitionService.Get<IngredientsItemDefinition>(resourceItemID);
			int transactionId = ingredientsItemDefinition.TransactionId;
			if (playerService.FinishTransaction(transactionId, TransactionTarget.HARVEST, new TransactionArg(villainLairResourcePlot.ID)))
			{
				VillainLairEntranceBuilding byInstanceId = playerService.GetByInstanceId<VillainLairEntranceBuilding>(villainLairResourcePlot.parentLair.portalInstanceID);
				Vector3 type = ((!lairModel.isPortalResourceModalOpen) ? new Vector3(building.Location.x, 0f, building.Location.y) : new Vector3(byInstanceId.Location.x, 0f, byInstanceId.Location.y));
				spawnDooberSignal.Dispatch(type, DestinationType.STORAGE, ingredientsItemDefinition.ID, true);
				SetPlotAsHarvested(villainLairResourcePlot, resourceItemID);
				int item = byInstanceId.GetNewHarvestAvailableForPortal(playerService).Item1;
				updateResourceIconCountSignal.Dispatch(new Tuple<int, int>(byInstanceId.ID, resourceItemID), item);
				buildingObj.Bounce();
				callback();
			}
			return true;
		}

		private void SetPlotAsHarvested(VillainLairResourcePlot plot, int itemID)
		{
			plot.UTCLastTaskingTimeStarted = 0;
			buildingChangeStateSignal.Dispatch(plot.ID, BuildingState.Idle);
			plot.harvestCount--;
			updateResourceIconCountSignal.Dispatch(new Tuple<int, int>(plot.ID, itemID), 0);
		}

		private bool TryHarvestVillainLairPortal(Building building)
		{
			VillainLairEntranceBuilding villainLairEntranceBuilding = building as VillainLairEntranceBuilding;
			if (villainLairEntranceBuilding == null)
			{
				return false;
			}
			if (villainLairEntranceBuilding.State == BuildingState.Inaccessible)
			{
				return true;
			}
			Tuple<int, int> newHarvestAvailableForPortal = villainLairEntranceBuilding.GetNewHarvestAvailableForPortal(playerService);
			int item = newHarvestAvailableForPortal.Item1;
			int item2 = newHarvestAvailableForPortal.Item2;
			if (item2 > 0 && !sentFromUI)
			{
				return RewardBonusItems(villainLairEntranceBuilding);
			}
			if (item < 1)
			{
				return true;
			}
			VillainLair byInstanceId = playerService.GetByInstanceId<VillainLair>(villainLairEntranceBuilding.VillainLairInstanceID);
			int firstBuildingNumberOfHarvestableResourcePlot = byInstanceId.GetFirstBuildingNumberOfHarvestableResourcePlot(playerService, false);
			if (firstBuildingNumberOfHarvestableResourcePlot != 0)
			{
				VillainLairResourcePlot byInstanceId2 = playerService.GetByInstanceId<VillainLairResourcePlot>(firstBuildingNumberOfHarvestableResourcePlot);
				if (byInstanceId2 != null)
				{
					bool flag = false;
					int resourceItemID = byInstanceId2.parentLair.Definition.ResourceItemID;
					IngredientsItemDefinition ingredientsItemDefinition = definitionService.Get<IngredientsItemDefinition>(resourceItemID);
					int transactionId = ingredientsItemDefinition.TransactionId;
					if (playerService.FinishTransaction(transactionId, TransactionTarget.HARVEST, new TransactionArg(byInstanceId2.ID)))
					{
						Vector3 type = new Vector3(villainLairEntranceBuilding.Location.x, 0f, villainLairEntranceBuilding.Location.y);
						spawnDooberSignal.Dispatch(type, DestinationType.STORAGE, ingredientsItemDefinition.ID, true);
						SetPlotAsHarvested(byInstanceId2, resourceItemID);
						int item3 = villainLairEntranceBuilding.GetNewHarvestAvailableForPortal(playerService).Item1;
						updateResourceIconCountSignal.Dispatch(new Tuple<int, int>(villainLairEntranceBuilding.ID, resourceItemID), item3);
						callback();
					}
				}
			}
			return true;
		}

		private bool RewardBonusItems(Building building)
		{
			Vector3 buildingPosition = new Vector3(building.Location.x, 0f, building.Location.y);
			int iD = building.ID;
			ResourceBuilding resourceBuilding = building as ResourceBuilding;
			VillainLairResourcePlot villainLairResourcePlot = building as VillainLairResourcePlot;
			VillainLairEntranceBuilding villainLairEntranceBuilding = building as VillainLairEntranceBuilding;
			int num = 0;
			int num2 = 0;
			VillainLairResourcePlot villainLairResourcePlot2 = null;
			VillainLairEntranceBuilding villainLairEntranceBuilding2 = null;
			if (resourceBuilding != null)
			{
				SortBonusItemList(resourceBuilding.BonusMinionItems);
				num = resourceBuilding.BonusMinionItems.Count - 1;
				num2 = resourceBuilding.BonusMinionItems[num];
			}
			else if (villainLairResourcePlot != null)
			{
				SortBonusItemList(villainLairResourcePlot.BonusMinionItems);
				num = villainLairResourcePlot.BonusMinionItems.Count - 1;
				num2 = villainLairResourcePlot.BonusMinionItems[num];
				villainLairEntranceBuilding2 = playerService.GetByInstanceId<VillainLairEntranceBuilding>(villainLairResourcePlot.parentLair.portalInstanceID);
				if (lairModel.isPortalResourceModalOpen)
				{
					buildingPosition = new Vector3(villainLairEntranceBuilding2.Location.x, 0f, villainLairEntranceBuilding2.Location.y);
				}
			}
			else if (villainLairEntranceBuilding != null)
			{
				VillainLair byInstanceId = playerService.GetByInstanceId<VillainLair>(villainLairEntranceBuilding.VillainLairInstanceID);
				List<int> allPlotBonusItems = byInstanceId.GetAllPlotBonusItems(playerService);
				SortBonusItemList(allPlotBonusItems);
				int index = allPlotBonusItems.Count - 1;
				int num3 = allPlotBonusItems[index];
				for (int i = 0; i < byInstanceId.resourcePlotInstanceIDs.Count; i++)
				{
					villainLairResourcePlot2 = playerService.GetByInstanceId<VillainLairResourcePlot>(byInstanceId.resourcePlotInstanceIDs[i]);
					if (villainLairResourcePlot2 != null && villainLairResourcePlot2.BonusMinionItems.Count > 0)
					{
						SortBonusItemList(villainLairResourcePlot2.BonusMinionItems);
						num = villainLairResourcePlot2.BonusMinionItems.Count - 1;
						num2 = villainLairResourcePlot2.BonusMinionItems[num];
						if (num2 == num3)
						{
							break;
						}
					}
				}
			}
			if (LimitStorage(num2))
			{
				return false;
			}
			RunBonusTransactionAndSpawnDoobers(num2, iD, buildingPosition);
			if (resourceBuilding != null)
			{
				buildingManager.GetComponent<BuildingManagerMediator>().LastHarvestedBuildingID = iD;
				buildingManager.GetComponent<BuildingManagerMediator>().HarvestTimer = 1f;
				resourceBuilding.BonusMinionItems.RemoveAt(num);
				updateResourceIconCountSignal.Dispatch(new Tuple<int, int>(iD, 196), resourceBuilding.BonusMinionItems.Count);
				buildingObj.Bounce();
				questService.UpdateAllQuestsWithQuestStepType(QuestStepType.MysteryBoxOnboarding, QuestTaskTransition.Complete, resourceBuilding, resourceBuilding.Definition.ID, num2);
				if (resourceBuilding.GetTotalHarvests() == 0)
				{
					buildingChangeStateSignal.Dispatch(resourceBuilding.ID, BuildingState.Idle);
				}
			}
			else if (villainLairResourcePlot != null)
			{
				if (villainLairEntranceBuilding2 != null)
				{
					RemoveAndUpdateBonusIconsFromPlotAndPortal(villainLairResourcePlot, num, villainLairEntranceBuilding2);
				}
				buildingObj.Bounce();
			}
			else if (villainLairEntranceBuilding != null && villainLairResourcePlot2 != null)
			{
				RemoveAndUpdateBonusIconsFromPlotAndPortal(villainLairResourcePlot2, num, villainLairEntranceBuilding);
			}
			return true;
		}

		private void SortBonusItemList(List<int> bonusItems)
		{
			bonusItems.Sort(CompareBonusIds);
		}

		private int CompareBonusIds(int item1, int item2)
		{
			int result = item2.CompareTo(item1);
			if (item1 != 1 && item2 != 1)
			{
				DropItemDefinition definition;
				if (definitionService.TryGet<DropItemDefinition>(item1, out definition))
				{
					return 1;
				}
				if (definitionService.TryGet<DropItemDefinition>(item2, out definition))
				{
					return -1;
				}
			}
			return result;
		}

		private bool LimitStorage(int itemID)
		{
			if (itemID == 1)
			{
				return false;
			}
			if (playerService.isStorageFull())
			{
				OpenStorage();
				return true;
			}
			return false;
		}

		private void RunBonusTransactionAndSpawnDoobers(int itemDefID, int buildingId, Vector3 buildingPosition)
		{
			int quantity = 1;
			DestinationType type = DestinationType.MYSTERY_BOX;
			if (itemDefID == 1)
			{
				quantity = definitionService.Get<MinionBenefitLevelBandDefintion>(89898).BonusPremiumRewardValue;
			}
			playerService.CreateAndRunCustomTransaction(itemDefID, quantity, TransactionTarget.NO_VISUAL, new TransactionArg(buildingId));
			spawnDooberSignal.Dispatch(buildingPosition, type, itemDefID, true);
		}

		private void RemoveAndUpdateBonusIconsFromPlotAndPortal(VillainLairResourcePlot plot, int endIndex, VillainLairEntranceBuilding portal)
		{
			plot.BonusMinionItems.RemoveAt(endIndex);
			updateResourceIconCountSignal.Dispatch(new Tuple<int, int>(plot.ID, 196), plot.BonusMinionItems.Count);
			int item = portal.GetNewHarvestAvailableForPortal(playerService).Item2;
			updateResourceIconCountSignal.Dispatch(new Tuple<int, int>(plot.parentLair.portalInstanceID, 196), item);
		}

		private int GetNewHarvestAvailable(ResourceBuilding resourceBuilding, TaskableBuilding taskBuilding)
		{
			int num = 0;
			if (resourceBuilding != null)
			{
				num = resourceBuilding.AvailableHarvest;
				if (taskBuilding.GetMinionsInBuilding() == 0)
				{
					if (resourceBuilding.GetTotalHarvests() == 0)
					{
						buildingChangeStateSignal.Dispatch(taskBuilding.ID, BuildingState.Idle);
					}
				}
				else
				{
					buildingChangeStateSignal.Dispatch(taskBuilding.ID, BuildingState.Working);
				}
			}
			else
			{
				num = taskBuilding.GetNumCompleteMinions();
			}
			return num;
		}

		private void TryHarvestCrafting(Building building)
		{
			CraftingBuilding craftingBuilding = building as CraftingBuilding;
			if (craftingBuilding == null)
			{
				return;
			}
			int count = craftingBuilding.CompletedCrafts.Count;
			if (count <= 0)
			{
				return;
			}
			Vector3 type = new Vector3(craftingBuilding.Location.x, 0f, craftingBuilding.Location.y);
			int num = craftingBuilding.CompletedCrafts[0];
			IngredientsItemDefinition ingredientsItemDefinition = definitionService.Get<IngredientsItemDefinition>(num);
			TransactionDefinition transaction = definitionService.Get<TransactionDefinition>(ingredientsItemDefinition.TransactionId);
			int xPOutputForTransaction = TransactionUtil.GetXPOutputForTransaction(transaction);
			TransactionArg transactionArg = new TransactionArg(craftingBuilding.ID);
			transactionArg.CraftableXPEarned = xPOutputForTransaction;
			if (!playerService.FinishTransaction(ingredientsItemDefinition.TransactionId, TransactionTarget.HARVEST, transactionArg))
			{
				return;
			}
			spawnDooberSignal.Dispatch(type, DestinationType.STORAGE, ingredientsItemDefinition.ID, true);
			IQuestService obj = questService;
			int item = num;
			obj.UpdateAllQuestsWithQuestStepType(QuestStepType.Harvest, QuestTaskTransition.Complete, null, 0, item);
			int num2 = 0;
			for (int i = 1; i < count; i++)
			{
				if (craftingBuilding.CompletedCrafts[i] == num)
				{
					num2++;
				}
			}
			craftingBuilding.CompletedCrafts.RemoveAt(GetLastIndexOfItem(craftingBuilding));
			count = craftingBuilding.CompletedCrafts.Count;
			int count2 = craftingBuilding.RecipeInQueue.Count;
			BuildingState param = BuildingState.Idle;
			if (count > 0 && count2 > 0)
			{
				param = BuildingState.HarvestableAndWorking;
			}
			else if (count > 0)
			{
				param = BuildingState.Harvestable;
			}
			else if (count2 > 0)
			{
				param = BuildingState.Working;
			}
			buildingChangeStateSignal.Dispatch(craftingBuilding.ID, param);
			TryHarvestCraftingDone(craftingBuilding.ID, num2, num);
		}

		private int GetLastIndexOfItem(CraftingBuilding craftBuilding)
		{
			for (int num = craftBuilding.CompletedCrafts.Count - 1; num >= 0; num--)
			{
				if (craftBuilding.CompletedCrafts[num] == craftBuilding.CompletedCrafts[0])
				{
					return num;
				}
			}
			return 0;
		}

		private void TryHarvestCraftingDone(int buildingId, int newCount, int itemDefId)
		{
			buildingObj.Bounce();
			buildingHarvestSignal.Dispatch(buildingId);
			callback();
			updateResourceIconCountSignal.Dispatch(new Tuple<int, int>(buildingId, itemDefId), newCount);
		}
	}
}
