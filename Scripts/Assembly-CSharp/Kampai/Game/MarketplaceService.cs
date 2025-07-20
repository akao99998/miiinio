using System.Collections.Generic;

namespace Kampai.Game
{
	public class MarketplaceService : IMarketplaceService
	{
		private enum CostType
		{
			minStrike = 0,
			maxStrike = 1
		}

		private float debugMultiplier = 1f;

		[Inject]
		public IConfigurationsService configurationsService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(SocialServices.FACEBOOK)]
		public ISocialService facebookService { get; set; }

		public bool IsDebugMode { get; set; }

		public float DebugMultiplier
		{
			get
			{
				return debugMultiplier;
			}
			set
			{
				debugMultiplier = value;
			}
		}

		public int DebugSelectedItem { get; set; }

		public bool DebugFacebook { get; set; }

		public bool isServerKillSwitchEnabled
		{
			get
			{
				return configurationsService.isKillSwitchOn(KillSwitch.MARKETPLACESERVER);
			}
		}

		public bool GetItemDefinitionByItemID(int itemID, out MarketplaceItemDefinition itemDefinition)
		{
			MarketplaceDefinition marketplaceDefinition = definitionService.Get<MarketplaceDefinition>();
			bool result = false;
			itemDefinition = null;
			if (marketplaceDefinition != null && marketplaceDefinition.itemDefinitions != null)
			{
				foreach (MarketplaceItemDefinition itemDefinition2 in marketplaceDefinition.itemDefinitions)
				{
					if (itemDefinition2.ItemID == itemID)
					{
						itemDefinition = itemDefinition2;
						result = true;
						break;
					}
				}
			}
			return result;
		}

		private void SetItemPrice(int itemID, int price, CostType costType)
		{
			MarketplaceDefinition marketplaceDefinition = definitionService.Get<MarketplaceDefinition>();
			if (marketplaceDefinition == null || marketplaceDefinition.itemDefinitions == null)
			{
				return;
			}
			foreach (MarketplaceItemDefinition itemDefinition in marketplaceDefinition.itemDefinitions)
			{
				if (itemDefinition.ItemID != itemID)
				{
					continue;
				}
				switch (costType)
				{
				case CostType.minStrike:
					itemDefinition.MinStrikePrice = price;
					break;
				case CostType.maxStrike:
					itemDefinition.MaxStrikePrice = price;
					break;
				}
				break;
			}
		}

		public void SetMinStrikePrice(int itemID, int price)
		{
			SetItemPrice(itemID, price, CostType.minStrike);
		}

		public void SetMaxStrikePrice(int itemID, int price)
		{
			SetItemPrice(itemID, price, CostType.maxStrike);
		}

		public MarketplaceSaleSlot GetSlotByItem(MarketplaceSaleItem item)
		{
			IList<MarketplaceSaleSlot> instancesByType = playerService.GetInstancesByType<MarketplaceSaleSlot>();
			foreach (MarketplaceSaleSlot item2 in instancesByType)
			{
				MarketplaceSaleItem byInstanceId = playerService.GetByInstanceId<MarketplaceSaleItem>(item2.itemId);
				if (byInstanceId == item)
				{
					return item2;
				}
			}
			return null;
		}

		public MarketplaceSaleSlot GetNextAvailableSlot()
		{
			IList<MarketplaceSaleSlot> instancesByType = playerService.GetInstancesByType<MarketplaceSaleSlot>();
			foreach (MarketplaceSaleSlot item in instancesByType)
			{
				MarketplaceSaleItem byInstanceId = playerService.GetByInstanceId<MarketplaceSaleItem>(item.itemId);
				if (byInstanceId == null && item.state == MarketplaceSaleSlot.State.UNLOCKED && IsSlotVisible(item))
				{
					return item;
				}
			}
			return null;
		}

		public int GetSlotIndex(MarketplaceSaleSlot slot)
		{
			IList<MarketplaceSaleSlot> instancesByType = playerService.GetInstancesByType<MarketplaceSaleSlot>();
			if (instancesByType.Contains(slot))
			{
				return instancesByType.IndexOf(slot);
			}
			return -1;
		}

		public bool AreThereValidItemsInStorage()
		{
			foreach (Item sellableItem in playerService.GetSellableItems())
			{
				DynamicIngredientsDefinition dynamicIngredientsDefinition = sellableItem.Definition as DynamicIngredientsDefinition;
				if (dynamicIngredientsDefinition == null)
				{
					MarketplaceItemDefinition itemDefinition;
					GetItemDefinitionByItemID(sellableItem.Definition.ID, out itemDefinition);
					if (itemDefinition != null)
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool IsUnlocked()
		{
			MarketplaceDefinition marketplaceDefinition = definitionService.Get<MarketplaceDefinition>();
			return playerService.GetQuantity(StaticItem.LEVEL_ID) >= marketplaceDefinition.LevelGate;
		}

		public bool AreThereSoldItems()
		{
			IList<MarketplaceSaleSlot> instancesByType = playerService.GetInstancesByType<MarketplaceSaleSlot>();
			foreach (MarketplaceSaleSlot item in instancesByType)
			{
				if (item.state != 0 && IsSlotVisible(item))
				{
					MarketplaceSaleItem byInstanceId = playerService.GetByInstanceId<MarketplaceSaleItem>(item.itemId);
					if (byInstanceId != null && byInstanceId.state == MarketplaceSaleItem.State.SOLD)
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool IsSlotVisible(MarketplaceSaleSlot slot)
		{
			if (slot.Definition.type == MarketplaceSaleSlotDefinition.SlotType.PREMIUM_UNLOCKABLE && !facebookService.isLoggedIn && !DebugFacebook)
			{
				return false;
			}
			return true;
		}

		public bool AreTherePendingItems()
		{
			IList<MarketplaceSaleSlot> instancesByType = playerService.GetInstancesByType<MarketplaceSaleSlot>();
			foreach (MarketplaceSaleSlot item in instancesByType)
			{
				MarketplaceSaleItem byInstanceId = playerService.GetByInstanceId<MarketplaceSaleItem>(item.itemId);
				if (byInstanceId != null && byInstanceId.state == MarketplaceSaleItem.State.PENDING)
				{
					return true;
				}
			}
			return false;
		}
	}
}
