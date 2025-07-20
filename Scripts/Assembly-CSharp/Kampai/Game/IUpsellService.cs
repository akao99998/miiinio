using System.Collections.Generic;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game
{
	public interface IUpsellService
	{
		QuantityItem GetItemDef(int index, SalePackDefinition saleDefinition);

		QuantityItem GetItemDef(int index, IList<QuantityItem> outputs);

		uint SumOutput(IList<QuantityItem> inputs, int id);

		int GetInputsCount(SalePackDefinition saleDefinition);

		void SetupFreeItemButton(LocalizeView localizeView, ButtonView buttonView, string buttonLocKey, RuntimeAnimatorController freeCollectButtonAnimator);

		Sale GetSaleInstanceFromID(List<Sale> playerSales, int id);

		void SetupBuyButton(IList<QuantityItem> inputs, KampaiImage inputItemIcon, Definition definition);

		SalePackDefinition GetSalePackDefinition(int upsellDefId);

		void ScheduleSale(Sale saleInstance);

		void ScheduleSales(IList<Sale> saleInstances);

		bool IsSaleUpsellInstance(Sale saleInstance);

		Sale AddNewSaleInstance(SalePackDefinition salePackDefinition);
	}
}
