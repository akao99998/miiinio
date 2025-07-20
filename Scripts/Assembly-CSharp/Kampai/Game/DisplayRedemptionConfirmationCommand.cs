using Kampai.Game.Mtx;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class DisplayRedemptionConfirmationCommand : Command
	{
		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService defService { get; set; }

		[Inject]
		public OpenUpSellModalSignal openUpsellModalSignal { get; set; }

		public override void Execute()
		{
			ReceiptValidationResult receiptValidationResult = playerService.topPendingRedemption();
			if (receiptValidationResult == null)
			{
				return;
			}
			foreach (SalePackDefinition item in defService.GetAll<SalePackDefinition>())
			{
				if (ItemUtil.CompareSKU(item.SKU, receiptValidationResult.sku))
				{
					openUpsellModalSignal.Dispatch(item, "REDEMPTION", true);
				}
			}
		}
	}
}
