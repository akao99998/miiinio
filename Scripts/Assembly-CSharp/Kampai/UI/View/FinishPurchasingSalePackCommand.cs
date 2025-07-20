using Elevation.Logging;
using Kampai.Game;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class FinishPurchasingSalePackCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("FinishPurchasingSalePackCommand") as IKampaiLogger;

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public UpdatePurchasedSalesSignal updatePurchasedSalesSignal { get; set; }

		[Inject]
		public ReconcileSalesSignal reconcileSalesSignal { get; set; }

		[Inject]
		public RefreshMTXStoreSignal refreshMTXStoreSignal { get; set; }

		[Inject]
		public RemoveSalePackSignal removeSalePackSignal { get; set; }

		[Inject]
		public int saleDefinitionId { get; set; }

		public override void Execute()
		{
			PackDefinition packDefinition = definitionService.Get<PackDefinition>(saleDefinitionId);
			if (packDefinition == null)
			{
				logger.Error("Unable to find the sale definition with id: {0}", saleDefinitionId);
				return;
			}
			playerService.AddUpsellToPurchased(saleDefinitionId);
			SalePackDefinition salePackDefinition = packDefinition as SalePackDefinition;
			if (salePackDefinition != null)
			{
				Sale firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<Sale>(saleDefinitionId);
				if (firstInstanceByDefinitionId == null)
				{
					logger.Error("Sale instance not found for definition id {0}", saleDefinitionId);
					return;
				}
				removeSalePackSignal.Dispatch(firstInstanceByDefinitionId.ID);
				UpdateServerSales(salePackDefinition);
				playerService.Remove(firstInstanceByDefinitionId);
			}
			refreshMTXStoreSignal.Dispatch();
			reconcileSalesSignal.Dispatch(0);
		}

		private void UpdateServerSales(SalePackDefinition salePackDefinition)
		{
			if (!string.IsNullOrEmpty(salePackDefinition.ServerSaleId))
			{
				updatePurchasedSalesSignal.Dispatch(salePackDefinition.ServerSaleId);
			}
		}
	}
}
