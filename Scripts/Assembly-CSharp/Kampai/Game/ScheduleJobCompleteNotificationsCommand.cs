using System.Collections.Generic;
using Kampai.Game.Transaction;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class ScheduleJobCompleteNotificationsCommand : Command
	{
		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ScheduleNotificationSignal notificationSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public IDevicePrefsService notifPrefs { get; set; }

		[Inject]
		public IMarketplaceService marketplaceService { get; set; }

		public override void Execute()
		{
			if (!Native.AreNotificationsEnabled())
			{
				return;
			}
			ScheduleMinionNotifications();
			ICollection<Building> instancesByType = playerService.GetInstancesByType<Building>();
			int num = int.MaxValue;
			int num2 = int.MinValue;
			foreach (Building item in instancesByType)
			{
				int num3 = timeEventService.GetTimeRemaining(item.ID);
				if (num3 <= 0)
				{
					continue;
				}
				if (item.State == BuildingState.Construction)
				{
					if (num3 < num)
					{
						num = num3;
					}
					continue;
				}
				CraftingBuilding craftingBuilding = item as CraftingBuilding;
				if (craftingBuilding == null)
				{
					continue;
				}
				IList<int> recipeInQueue = craftingBuilding.RecipeInQueue;
				if (recipeInQueue.Count > 1)
				{
					for (int i = 1; i < recipeInQueue.Count; i++)
					{
						IngredientsItemDefinition ingredientsItemDefinition = definitionService.Get<IngredientsItemDefinition>(recipeInQueue[i]);
						if (ingredientsItemDefinition != null)
						{
							num3 += IngredientsItemUtil.GetHarvestTimeFromIngredientDefinition(ingredientsItemDefinition, definitionService);
						}
					}
				}
				if (num3 > num2)
				{
					num2 = num3;
				}
			}
			if (num2 > int.MinValue)
			{
				ScheduleCraftingNotification(num2);
			}
			if (num < int.MaxValue)
			{
				ScheduleConstructionNotification(num);
			}
			ScheduleBlackMarketNotifications();
			ScheduleMarketPlaceNotifications();
		}

		private int CalculateFirstNewFulfillableOrder()
		{
			List<TransactionDefinition> unfillableOrders = new List<TransactionDefinition>();
			Dictionary<int, uint> possibleInventory = new Dictionary<int, uint>();
			List<Tuple<int, int>> list = new List<Tuple<int, int>>();
			GetUnfillableOrders(unfillableOrders, possibleInventory);
			GetAllHarvestingBuildings(list);
			AddItemsFromCraftingBuildings(list);
			list.Sort((Tuple<int, int> a, Tuple<int, int> b) => a.Item1.CompareTo(b.Item1));
			return FindFulfillableOrder(unfillableOrders, possibleInventory, list);
		}

		private void ScheduleMinionNotifications()
		{
			if (!notifPrefs.GetDevicePrefs().BaseResourceNotif)
			{
				return;
			}
			ICollection<Minion> instancesByType = playerService.GetInstancesByType<Minion>();
			int num = -1;
			foreach (Minion item in instancesByType)
			{
				int timeRemaining = timeEventService.GetTimeRemaining(item.ID);
				if (timeRemaining > num)
				{
					num = timeRemaining;
				}
			}
			if (num > 0)
			{
				NotificationDefinition notificationDefinition = definitionService.Get(10007) as NotificationDefinition;
				if (notificationDefinition != null)
				{
					notificationDefinition.Seconds = num;
					notificationSignal.Dispatch(notificationDefinition);
				}
			}
		}

		private void ScheduleCraftingNotification(int maxCraftingTime)
		{
			if (notifPrefs.GetDevicePrefs().CraftingNotif && maxCraftingTime > 0)
			{
				NotificationDefinition notificationDefinition = definitionService.Get(10011) as NotificationDefinition;
				if (notificationDefinition != null)
				{
					notificationDefinition.Seconds = maxCraftingTime;
					notificationSignal.Dispatch(notificationDefinition);
				}
			}
		}

		private void ScheduleConstructionNotification(int maxBuildingTime)
		{
			if (notifPrefs.GetDevicePrefs().ConstructionNotif && maxBuildingTime > 0)
			{
				NotificationDefinition notificationDefinition = definitionService.Get(10008) as NotificationDefinition;
				if (notificationDefinition != null)
				{
					notificationDefinition.Seconds = maxBuildingTime;
					notificationSignal.Dispatch(notificationDefinition);
				}
			}
		}

		private void ScheduleBlackMarketNotifications()
		{
			if (!notifPrefs.GetDevicePrefs().BlackMarketNotif)
			{
				return;
			}
			int num = -1;
			for (int i = 1; i <= 20; i++)
			{
				int timeRemaining = timeEventService.GetTimeRemaining(-i);
				if (timeRemaining > 0 && (num < 0 || timeRemaining < num))
				{
					num = timeRemaining;
				}
			}
			if (num > 0)
			{
				NotificationDefinition notificationDefinition = definitionService.Get(10009) as NotificationDefinition;
				if (notificationDefinition != null)
				{
					notificationDefinition.Seconds = num;
					notificationSignal.Dispatch(notificationDefinition);
				}
			}
			int num2 = CalculateFirstNewFulfillableOrder();
			if (num2 > 0)
			{
				NotificationDefinition notificationDefinition2 = definitionService.Get(10010) as NotificationDefinition;
				if (notificationDefinition2 != null)
				{
					notificationDefinition2.Seconds = num2;
					notificationSignal.Dispatch(notificationDefinition2);
				}
			}
		}

		private void ScheduleMarketPlaceNotifications()
		{
			if (!notifPrefs.GetDevicePrefs().MarketPlaceNotif)
			{
				return;
			}
			ICollection<MarketplaceSaleItem> instancesByType = playerService.GetInstancesByType<MarketplaceSaleItem>();
			if (instancesByType == null)
			{
				return;
			}
			int num = -1;
			int track = -1;
			foreach (MarketplaceSaleItem item in instancesByType)
			{
				int timeRemaining = timeEventService.GetTimeRemaining(item.ID);
				if (timeRemaining > 0 && (num < 0 || timeRemaining < num))
				{
					num = timeRemaining;
					track = marketplaceService.GetSlotIndex(marketplaceService.GetSlotByItem(item));
				}
			}
			if (num > 0)
			{
				NotificationDefinition notificationDefinition = definitionService.Get(10013) as NotificationDefinition;
				if (notificationDefinition != null)
				{
					notificationDefinition.Seconds = num;
					notificationDefinition.Track = track;
					notificationSignal.Dispatch(notificationDefinition);
				}
			}
		}

		private void GetUnfillableOrders(ICollection<TransactionDefinition> UnfillableOrders, IDictionary<int, uint> PossibleInventory)
		{
			foreach (Building item in playerService.GetInstancesByType<Building>())
			{
				OrderBoard orderBoard = item as OrderBoard;
				if (orderBoard == null || orderBoard.tickets == null)
				{
					continue;
				}
				foreach (OrderBoardTicket ticket in orderBoard.tickets)
				{
					if (playerService.VerifyTransaction(ticket.TransactionInst.ToDefinition()))
					{
						continue;
					}
					UnfillableOrders.Add(ticket.TransactionInst.ToDefinition());
					foreach (QuantityItem input in ticket.TransactionInst.Inputs)
					{
						if (!PossibleInventory.ContainsKey(input.ID))
						{
							PossibleInventory.Add(input.ID, playerService.GetQuantityByDefinitionId(input.ID));
						}
					}
				}
			}
		}

		private void GetAllHarvestingBuildings(ICollection<Tuple<int, int>> FutureTransactions)
		{
			foreach (Minion item in playerService.GetInstancesByType<Minion>())
			{
				if (item.State != MinionState.Tasking || item.BuildingID <= 0)
				{
					continue;
				}
				int timeRemaining = timeEventService.GetTimeRemaining(item.ID);
				if (timeRemaining > 0)
				{
					TaskableBuilding byInstanceId = playerService.GetByInstanceId<TaskableBuilding>(item.BuildingID);
					if (byInstanceId != null)
					{
						FutureTransactions.Add(new Tuple<int, int>(timeRemaining, byInstanceId.GetTransactionID(definitionService)));
					}
				}
			}
		}

		private void AddItemsFromCraftingBuildings(ICollection<Tuple<int, int>> FutureTransactions)
		{
			foreach (Building item in playerService.GetInstancesByType<Building>())
			{
				CraftingBuilding craftingBuilding = item as CraftingBuilding;
				if (craftingBuilding == null)
				{
					continue;
				}
				int num = timeEventService.GetTimeRemaining(craftingBuilding.ID);
				if (num <= 0)
				{
					continue;
				}
				bool flag = true;
				foreach (int item2 in craftingBuilding.RecipeInQueue)
				{
					IngredientsItemDefinition ingredientsItemDefinition = definitionService.Get<IngredientsItemDefinition>(item2);
					if (!flag)
					{
						num += IngredientsItemUtil.GetHarvestTimeFromIngredientDefinition(ingredientsItemDefinition, definitionService);
					}
					FutureTransactions.Add(new Tuple<int, int>(num, ingredientsItemDefinition.TransactionId));
					flag = false;
				}
			}
		}

		private int FindFulfillableOrder(ICollection<TransactionDefinition> UnfillableOrders, IDictionary<int, uint> PossibleInventory, ICollection<Tuple<int, int>> FutureTransactions)
		{
			foreach (Tuple<int, int> FutureTransaction in FutureTransactions)
			{
				TransactionDefinition transactionDefinition = definitionService.Get<TransactionDefinition>(FutureTransaction.Item2);
				foreach (QuantityItem output in transactionDefinition.Outputs)
				{
					uint value = 0u;
					PossibleInventory.TryGetValue(output.ID, out value);
					value += output.Quantity;
					PossibleInventory[output.ID] = value;
				}
				foreach (TransactionDefinition UnfillableOrder in UnfillableOrders)
				{
					bool flag = true;
					foreach (QuantityItem input in UnfillableOrder.Inputs)
					{
						if (PossibleInventory[input.ID] < input.Quantity)
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						return FutureTransaction.Item1;
					}
				}
			}
			return -1;
		}
	}
}
