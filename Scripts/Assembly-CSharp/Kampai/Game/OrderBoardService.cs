using System;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game.Transaction;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class OrderBoardService : IOrderBoardService
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("OrderBoardService") as IKampaiLogger;

		private OrderBoard board;

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IRandomService randomService { get; set; }

		[Inject]
		public IPrestigeService characterService { get; set; }

		[Inject]
		public IPlayerDurationService playerDurationService { get; set; }

		[Inject]
		public ShowDialogSignal showDialogSignal { get; set; }

		[Inject]
		public OrderBoardUpdateTicketOnBoardSignal updateTicketOnBoardSignal { get; set; }

		[Inject]
		public DisplayPlayerTrainingSignal displaySignal { get; set; }

		[Inject]
		public UnlockCharacterModel characterModel { get; set; }

		[PostConstruct]
		public void Initialize()
		{
			board = playerService.GetByInstanceId<OrderBoard>(309);
		}

		public OrderBoard GetBoard()
		{
			return board;
		}

		public void ReplaceCharacterTickets(int characterDefinitionID)
		{
			foreach (OrderBoardTicket ticket in board.tickets)
			{
				if (ticket.CharacterDefinitionId == characterDefinitionID)
				{
					GetNewTicket(ticket.BoardIndex);
				}
			}
			updateTicketOnBoardSignal.Dispatch();
		}

		public int GetLongestIdleOrderDuration()
		{
			int longestIdleTime = 0;
			GetLongestIdleOrder(out longestIdleTime);
			return longestIdleTime;
		}

		public TransactionDefinition GetLongestIdleOrderTransaction()
		{
			int longestIdleTime = 0;
			OrderBoardTicket longestIdleOrder = GetLongestIdleOrder(out longestIdleTime);
			if (longestIdleOrder != null)
			{
				return longestIdleOrder.TransactionInst.ToDefinition();
			}
			return null;
		}

		private OrderBoardTicket GetLongestIdleOrder(out int longestIdleTime)
		{
			if (board == null)
			{
				logger.Warning("OrderBoardService is not instantiated yet. Calling Initialize.");
				Initialize();
			}
			longestIdleTime = 0;
			OrderBoardTicket result = null;
			foreach (OrderBoardTicket ticket in board.tickets)
			{
				if (ticket.StartTime <= 0)
				{
					int gameTimeDuration = playerDurationService.GetGameTimeDuration(ticket);
					if (gameTimeDuration > longestIdleTime)
					{
						longestIdleTime = gameTimeDuration;
						result = ticket;
					}
				}
			}
			return result;
		}

		public void GetNewTicket(int orderBoardIndex)
		{
			if (board == null)
			{
				return;
			}
			bool flag = false;
			IList<string> list = new List<string>(board.Definition.OrderNames);
			IList<string> list2 = new List<string>();
			OrderBoardTicket orderBoardTicket = null;
			foreach (OrderBoardTicket ticket in board.tickets)
			{
				if (ticket.BoardIndex != orderBoardIndex)
				{
					list2.Add(list[ticket.OrderNameTableIndex]);
					continue;
				}
				orderBoardTicket = ticket;
				flag = true;
			}
			foreach (string item2 in list2)
			{
				list.Remove(item2);
			}
			if (!flag)
			{
				orderBoardTicket = new OrderBoardTicket();
			}
			orderBoardTicket.StartTime = -1;
			orderBoardTicket.BoardIndex = orderBoardIndex;
			orderBoardTicket.TransactionInst = CreateNewOrder(orderBoardIndex);
			orderBoardTicket.StartGameTime = playerDurationService.TotalGamePlaySeconds;
			CheckIfTransactionContainsPartyFavor(orderBoardTicket.TransactionInst);
			if (orderBoardTicket.TransactionInst.Inputs.Count != 0)
			{
				if (!flag)
				{
					board.tickets.Add(orderBoardTicket);
				}
				if (!GetIsCharacterOrder(orderBoardTicket))
				{
					int index = RollDice(0, list.Count);
					string item = list[index];
					orderBoardTicket.OrderNameTableIndex = board.Definition.OrderNames.IndexOf(item);
					orderBoardTicket.CharacterDefinitionId = 0;
				}
			}
		}

		private bool GetIsCharacterOrder(OrderBoardTicket ticket)
		{
			bool result = false;
			int currentTicketXP = GetCurrentTicketXP(ticket);
			Prestige prestige = characterService.GetPrestige(ticket.CharacterDefinitionId);
			if (prestige != null && prestige.state != PrestigeState.InQueue && !characterService.IsPrestigeFulfilled(prestige))
			{
				for (int num = board.PriorityPrestigeDefinitionIDs.Count - 1; num >= 0; num--)
				{
					if (board.PriorityPrestigeDefinitionIDs[num] == ticket.CharacterDefinitionId)
					{
						board.PriorityPrestigeDefinitionIDs.RemoveAt(num);
					}
				}
				result = true;
			}
			else if (board.PriorityPrestigeDefinitionIDs.Count > 0)
			{
				ticket.CharacterDefinitionId = board.PriorityPrestigeDefinitionIDs[0];
				board.PriorityPrestigeDefinitionIDs.RemoveAt(0);
				CheckAndUpdatePriorityPrestigeCharacterXP(ticket, currentTicketXP);
				result = true;
			}
			else if (ShouldBeCharacterOrder())
			{
				int num2 = PickCharacterDefinitionId(currentTicketXP);
				if (num2 != 0)
				{
					ticket.CharacterDefinitionId = num2;
					result = true;
				}
			}
			return result;
		}

		private void CheckIfTransactionContainsPartyFavor(TransactionInstance trasaction)
		{
			if (trasaction == null)
			{
				return;
			}
			List<PartyFavorAnimationDefinition> all = definitionService.GetAll<PartyFavorAnimationDefinition>();
			if (all == null || all.Count == 0)
			{
				return;
			}
			foreach (QuantityItem input in trasaction.Inputs)
			{
				foreach (PartyFavorAnimationDefinition item in all)
				{
					if (input.ID == item.ItemID)
					{
						trasaction.Outputs.Add(new QuantityItem(item.UnlockId, 1u));
					}
				}
			}
		}

		public void AddPriorityPrestigeCharacter(int prestigeDefinitionID)
		{
			if (board.PriorityPrestigeDefinitionIDs.Contains(prestigeDefinitionID))
			{
				return;
			}
			Prestige prestige = characterService.GetPrestige(prestigeDefinitionID);
			if (prestigeDefinitionID != 40003 || prestige.CurrentPrestigeLevel >= 1)
			{
				board.PriorityPrestigeDefinitionIDs.Add(prestigeDefinitionID);
				PrestigeDefinition definition = prestige.Definition;
				QuestDialogSetting questDialogSetting = new QuestDialogSetting();
				questDialogSetting.type = QuestDialogType.NEWPRESTIGE;
				questDialogSetting.additionalStringParameter = definition.LocalizedKey;
				if (prestige.CurrentPrestigeLevel >= 1)
				{
					displaySignal.Dispatch(19000022, false, new Signal<bool>());
				}
				MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
				if (minionPartyInstance.IsPartyReady || minionPartyInstance.IsPartyHappening)
				{
					characterModel.dialogQueue.Add(questDialogSetting);
				}
				else
				{
					showDialogSignal.Dispatch("AlertPrePrestige", questDialogSetting, new Tuple<int, int>(0, 0));
				}
			}
		}

		public void SetEnabled(bool b)
		{
			GetBoard().menuEnabled = b;
		}

		private void CheckAndUpdatePriorityPrestigeCharacterXP(OrderBoardTicket ticket, int currentTicketXP)
		{
			Prestige prestige = characterService.GetPrestige(ticket.CharacterDefinitionId);
			if (prestige == null)
			{
				return;
			}
			int neededPrestigePoints = prestige.NeededPrestigePoints;
			if (currentTicketXP < neededPrestigePoints || prestige.state != PrestigeState.PreUnlocked)
			{
				return;
			}
			int num = RollDice(1, neededPrestigePoints);
			float num2 = (float)num / (float)currentTicketXP;
			TransactionInstance transactionInst = ticket.TransactionInst;
			uint num3 = 0u;
			foreach (QuantityItem input in transactionInst.Inputs)
			{
				num3 = (uint)((float)input.Quantity * num2);
				num3 = ((num3 == 0) ? 1u : num3);
				input.Quantity = num3;
			}
			foreach (QuantityItem output in transactionInst.Outputs)
			{
				num3 = (uint)((float)output.Quantity * num2);
				num3 = ((num3 == 0) ? 1u : num3);
				output.Quantity = num3;
			}
		}

		private bool ShouldBeCharacterOrder()
		{
			int num = RollDice(0, 100);
			if (num < board.Definition.CharacterOrderChance)
			{
				return true;
			}
			return false;
		}

		private int PickCharacterDefinitionId(int currentTicketXP)
		{
			Dictionary<int, CharacterTicketData> dictionary = UpdateCharacterTicketDataOnOrderBoard();
			WeightedDefinition weightedDefinition = new WeightedDefinition();
			weightedDefinition.Entities = new List<WeightedQuantityItem>();
			Dictionary<int, Prestige> allUnlockedPrestiges = characterService.GetAllUnlockedPrestiges();
			bool flag = characterService.IsTikiBarFull();
			bool flag2 = characterService.GetEmptyCabana() == null;
			if (flag && flag2)
			{
				return 0;
			}
			foreach (KeyValuePair<int, Prestige> item in allUnlockedPrestiges)
			{
				Prestige value = item.Value;
				PrestigeDefinition definition = value.Definition;
				if ((!flag || definition.Type != 0) && (!flag2 || definition.Type != PrestigeType.Villain))
				{
					int num = 0;
					int num2 = 0;
					int iD = definition.ID;
					if (dictionary.ContainsKey(iD))
					{
						num = dictionary[iD].OnBoardCount;
						num2 = dictionary[iD].XPAmount;
					}
					if (num < definition.MaxedBadgedOrder && (value.state != PrestigeState.PreUnlocked || num < 1) && (value.state == PrestigeState.PreUnlocked || value.state == PrestigeState.Prestige) && num2 + value.CurrentPrestigePoints <= value.NeededPrestigePoints && (value.state != PrestigeState.PreUnlocked || value.CurrentPrestigePoints + currentTicketXP + num2 <= value.NeededPrestigePoints))
					{
						weightedDefinition.Entities.Add(new WeightedQuantityItem(iD, 0u, definition.OrderBoardWeight));
					}
				}
			}
			if (weightedDefinition.Entities.Count == 0)
			{
				return 0;
			}
			WeightedInstance weightedInstance = new WeightedInstance(weightedDefinition);
			return weightedInstance.NextPick(randomService).ID;
		}

		private Dictionary<int, CharacterTicketData> UpdateCharacterTicketDataOnOrderBoard()
		{
			Dictionary<int, CharacterTicketData> dictionary = new Dictionary<int, CharacterTicketData>();
			foreach (OrderBoardTicket ticket in board.tickets)
			{
				int characterDefinitionId = ticket.CharacterDefinitionId;
				if (characterDefinitionId != 0)
				{
					if (!dictionary.ContainsKey(characterDefinitionId))
					{
						CharacterTicketData characterTicketData = new CharacterTicketData();
						characterTicketData.OnBoardCount = 1;
						characterTicketData.XPAmount = GetCurrentTicketXP(ticket);
						dictionary.Add(characterDefinitionId, characterTicketData);
					}
					else
					{
						dictionary[characterDefinitionId].OnBoardCount++;
						dictionary[characterDefinitionId].XPAmount += TransactionUtil.ExtractQuantityFromTransaction(ticket.TransactionInst, 2);
					}
				}
			}
			return dictionary;
		}

		public void UpdateOrderNumber()
		{
			if (playerService.GetUnlockedDefsByType<IngredientsItemDefinition>().Count == 0)
			{
				return;
			}
			int num = 0;
			int quantity = (int)playerService.GetQuantity(StaticItem.LEVEL_ID);
			foreach (BlackMarketBoardUnlockedOrderSlotDefinition unlockTicketSlot in board.Definition.UnlockTicketSlots)
			{
				if (playerService.GetHighestFtueCompleted() < 999999)
				{
					num = 1;
				}
				else if (quantity >= unlockTicketSlot.UnlockLevel)
				{
					num = unlockTicketSlot.OrderSlots;
				}
			}
			StuartCharacter firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<StuartCharacter>(70001);
			if (firstInstanceByDefinitionId == null)
			{
				num = 1;
			}
			int count = board.tickets.Count;
			num = ((count <= num) ? num : count);
			for (int i = count; i < num; i++)
			{
				GetNewTicket(i);
			}
		}

		private TransactionInstance CreateNewOrder(int orderBoardIndex)
		{
			List<QuantityItem> uniqueIngredients = GetUniqueIngredients(orderBoardIndex);
			BlackMarketBoardSlotDefinition currentSlotValues = new BlackMarketBoardSlotDefinition();
			foreach (BlackMarketBoardSlotDefinition minMaxIngredient in board.Definition.MinMaxIngredients)
			{
				if (minMaxIngredient.SlotIndex == orderBoardIndex + 1)
				{
					currentSlotValues = minMaxIngredient;
				}
			}
			SetIngredientsQty(uniqueIngredients, currentSlotValues);
			return CreateOrderBoardTransactionBasedOnQuantityList(uniqueIngredients);
		}

		private int GetCurrentTicketXP(OrderBoardTicket targetTicket)
		{
			return TransactionUtil.ExtractQuantityFromTransaction(targetTicket.TransactionInst, 2);
		}

		private TransactionInstance CreateOrderBoardTransactionBasedOnQuantityList(IList<QuantityItem> qList)
		{
			TransactionInstance transactionInstance = new TransactionInstance();
			transactionInstance.Inputs = qList;
			int num = 0;
			float num2 = 0f;
			float num3 = 1f;
			int quantity = (int)playerService.GetQuantity(StaticItem.LEVEL_ID);
			foreach (BlackMarketBoardMultiplierDefinition item in board.Definition.LevelBandXP)
			{
				if (quantity >= item.Level)
				{
					num3 = item.Multiplier;
				}
			}
			foreach (QuantityItem q in qList)
			{
				IngredientsItemDefinition ingredientsItemDefinition = definitionService.Get<IngredientsItemDefinition>(q.ID);
				num += ingredientsItemDefinition.BaseGrindCost * (int)q.Quantity;
				num2 += (float)(ingredientsItemDefinition.BaseXP * (int)q.Quantity) * num3;
			}
			uint quantity2 = (uint)Math.Round(num2);
			transactionInstance.Outputs = new List<QuantityItem>();
			transactionInstance.Outputs.Add(new QuantityItem(0, (uint)num));
			transactionInstance.Outputs.Add(new QuantityItem(2, quantity2));
			playerService.AssignNextInstanceId(transactionInstance);
			return transactionInstance;
		}

		private void SetIngredientsQty(IEnumerable<QuantityItem> qList, BlackMarketBoardSlotDefinition currentSlotValues)
		{
			foreach (QuantityItem q in qList)
			{
				int quantity = RollDice(currentSlotValues.MinQuantity, currentSlotValues.MaxQuantity + 1);
				q.Quantity = (uint)quantity;
			}
		}

		private List<QuantityItem> GetUniqueIngredients(int orderBoardIndex)
		{
			IList<IngredientsItemDefinition> unlockedDefsByType = playerService.GetUnlockedDefsByType<IngredientsItemDefinition>();
			IList<IngredientsItemDefinition> availableIngredients = AvailableItems(unlockedDefsByType);
			return PickTicketIngredient(availableIngredients, orderBoardIndex);
		}

		private IList<IngredientsItemDefinition> AvailableItems(IList<IngredientsItemDefinition> unlockedItems)
		{
			List<IngredientsItemDefinition> list = new List<IngredientsItemDefinition>();
			foreach (IngredientsItemDefinition unlockedItem in unlockedItems)
			{
				if (!playerService.HasPurchasedBuildingAssociatedWithItem(unlockedItem))
				{
					list.Add(unlockedItem);
				}
			}
			foreach (IngredientsItemDefinition item in list)
			{
				unlockedItems.Remove(item);
			}
			return unlockedItems;
		}

		private List<QuantityItem> PickTicketIngredient(IList<IngredientsItemDefinition> availableIngredients, int orderBoardIndex)
		{
			List<int> list = new List<int>();
			List<QuantityItem> list2 = new List<QuantityItem>();
			foreach (IngredientsItemDefinition availableIngredient in availableIngredients)
			{
				int tier = availableIngredient.Tier;
				int iD = availableIngredient.ID;
				if (orderBoardIndex < 3 && tier == 0)
				{
					list.Add(iD);
				}
				else if (orderBoardIndex < 6 && orderBoardIndex >= 3 && tier <= 1)
				{
					list.Add(iD);
				}
				else if (orderBoardIndex >= 6 && tier >= 1)
				{
					list.Add(iD);
				}
			}
			int id = ((list.Count >= 1) ? list[RollDice(0, list.Count)] : availableIngredients[RollDice(0, availableIngredients.Count)].ID);
			list2.Add(new QuantityItem(id, 1u));
			return list2;
		}

		private int RollDice(int minValue, int maxValue)
		{
			return randomService.NextInt(minValue, maxValue);
		}
	}
}
