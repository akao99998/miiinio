using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Util;

public static class MarketplaceUtil
{
	public static int GetPremiumSlotCost(IDefinitionService definitionService, IPlayerService playerService)
	{
		MarketplaceDefinition marketplaceDefinition = definitionService.Get<MarketplaceDefinition>();
		ICollection<MarketplaceSaleSlot> byDefinitionId = playerService.GetByDefinitionId<MarketplaceSaleSlot>(1000008096);
		return byDefinitionId.Count * marketplaceDefinition.PremiumIncrementCost + marketplaceDefinition.PremiumInitialCost;
	}

	public static QuantityItem GetQuantityItem(IDefinitionService definitionService, MarketplaceItemDefinition itemDefinition)
	{
		TransactionDefinition transactionDefinition = definitionService.Get<TransactionDefinition>(itemDefinition.TransactionID);
		if (transactionDefinition.Outputs.Count > 0)
		{
			return transactionDefinition.Outputs[0];
		}
		return null;
	}

	public static int GetRewardValue(QuantityItem quantityItem, MarketplaceSaleItem marketplaceItem)
	{
		return (int)quantityItem.Quantity * marketplaceItem.SalePrice;
	}
}
