using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class GenerateBuyItemsCommand : Command
	{
		private enum ItemCategory
		{
			craftableType = 0,
			baseResourceType = 1,
			dropType = 2
		}

		public IKampaiLogger logger = LogManager.GetClassLogger("GenerateBuyItemsCommand") as IKampaiLogger;

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IRandomService randomService { get; set; }

		[Inject]
		public IMarketplaceService marketplaceService { get; set; }

		public override void Execute()
		{
			List<MarketplaceBuyItem> instancesByType = playerService.GetInstancesByType<MarketplaceBuyItem>();
			foreach (MarketplaceBuyItem item in instancesByType)
			{
				playerService.Remove(item);
			}
			IList<IngredientsItemDefinition> unlockedDefsByType = playerService.GetUnlockedDefsByType<IngredientsItemDefinition>();
			if (unlockedDefsByType.Count <= 0)
			{
				logger.Error("No items are unlocked yet, so no Marketplace should exist.");
				return;
			}
			IList<int> craftableItems = FindUnlockedInCategory("Craftable", unlockedDefsByType);
			IList<int> resourceItems = FindUnlockedInCategory("Base Resource", unlockedDefsByType);
			IList<DropItemDefinition> all = definitionService.GetAll<DropItemDefinition>();
			MarketplaceDefinition marketplaceDefinition = definitionService.Get<MarketplaceDefinition>();
			int totalBuyAds = marketplaceDefinition.TotalBuyAds;
			for (int i = 0; i < totalBuyAds; i++)
			{
				ItemCategory categoryPicked = PickItemCategory(marketplaceDefinition, craftableItems, resourceItems, all.Count);
				int itemID = ((marketplaceService.DebugSelectedItem <= 0) ? PickExactItemType(categoryPicked, craftableItems, resourceItems, all) : marketplaceService.DebugSelectedItem);
				int num = FindSizeOfStack(categoryPicked, marketplaceDefinition);
				MarketplaceItemDefinition itemDefinition;
				marketplaceService.GetItemDefinitionByItemID(itemID, out itemDefinition);
				int num2 = randomService.NextInt(itemDefinition.MinStrikePrice, itemDefinition.MaxStrikePrice + 1);
				int buyPrice = num2 * num;
				MarketplaceBuyItem i2 = CreateMarketplaceBuyItem(itemDefinition, num, buyPrice);
				playerService.Add(i2);
			}
		}

		private IList<int> FindUnlockedInCategory(string category, IList<IngredientsItemDefinition> unlockedItems)
		{
			IList<int> list = new List<int>();
			if (unlockedItems != null)
			{
				foreach (IngredientsItemDefinition unlockedItem in unlockedItems)
				{
					if (unlockedItem.TaxonomySpecific == category)
					{
						int iD = unlockedItem.ID;
						MarketplaceItemDefinition itemDefinition;
						if (!marketplaceService.GetItemDefinitionByItemID(iD, out itemDefinition))
						{
							logger.Error("Marketplace item doesn't exists {0}.", iD);
						}
						else
						{
							int probabilityWeight = itemDefinition.ProbabilityWeight;
							for (int i = 0; i < probabilityWeight; i++)
							{
								list.Add(iD);
							}
						}
					}
				}
				return list;
			}
			return null;
		}

		private ItemCategory PickItemCategory(MarketplaceDefinition marketplaceDefinition, IList<int> craftableItems, IList<int> resourceItems, int dropCount)
		{
			ItemCategory itemCategory = ItemCategory.craftableType;
			int craftableWeight = marketplaceDefinition.CraftableWeight;
			int baseResourceWeight = marketplaceDefinition.BaseResourceWeight;
			int dropWeight = marketplaceDefinition.DropWeight;
			int num = craftableWeight + baseResourceWeight + dropWeight;
			int num2 = randomService.NextInt(0, num);
			if (num2 < dropWeight && dropCount > 0)
			{
				return ItemCategory.dropType;
			}
			if (num2 < craftableWeight + dropWeight && craftableItems.Count > 0)
			{
				return ItemCategory.craftableType;
			}
			if (num2 < num && resourceItems.Count > 0)
			{
				return ItemCategory.baseResourceType;
			}
			IList<IngredientsItemDefinition> unlockedDefsByType = playerService.GetUnlockedDefsByType<IngredientsItemDefinition>();
			craftableItems = FindUnlockedInCategory("Craftable", unlockedDefsByType);
			resourceItems = FindUnlockedInCategory("Base Resource", unlockedDefsByType);
			logger.Error("No item category was picked. There's an error! Look into it.");
			return ItemCategory.baseResourceType;
		}

		private int PickExactItemType(ItemCategory categoryPicked, IList<int> craftableItems, IList<int> resourceItems, IList<DropItemDefinition> dropItems)
		{
			int num = 0;
			int result = 0;
			switch (categoryPicked)
			{
			case ItemCategory.craftableType:
				num = randomService.NextInt(craftableItems.Count);
				result = craftableItems[num];
				craftableItems.Remove(num);
				break;
			case ItemCategory.baseResourceType:
				num = randomService.NextInt(resourceItems.Count);
				result = resourceItems[num];
				resourceItems.Remove(num);
				break;
			case ItemCategory.dropType:
				num = randomService.NextInt(dropItems.Count);
				result = dropItems[num].ID;
				dropItems.Remove(dropItems[num]);
				break;
			default:
				logger.Error("This should never happens. It means no category was picked or at least no new item will be found.");
				break;
			}
			return result;
		}

		private int FindSizeOfStack(ItemCategory categoryPicked, MarketplaceDefinition marketplaceDefinition)
		{
			int num = 1;
			if (categoryPicked != ItemCategory.dropType)
			{
				return randomService.NextInt(1, marketplaceDefinition.MaxSellQuantity + 1);
			}
			return randomService.NextInt(1, marketplaceDefinition.MaxDropQuantity + 1);
		}

		private MarketplaceBuyItem CreateMarketplaceBuyItem(MarketplaceItemDefinition marketplaceItemDefinition, int stackSize, int buyPrice)
		{
			MarketplaceBuyItem marketplaceBuyItem = new MarketplaceBuyItem(marketplaceItemDefinition);
			marketplaceBuyItem.BuyQuantity = stackSize;
			marketplaceBuyItem.BuyPrice = buyPrice;
			marketplaceBuyItem.BoughtFlag = false;
			return marketplaceBuyItem;
		}
	}
}
