using System.Collections.Generic;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class ReconcileSalesCommand : Command
	{
		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public IUpsellService upsellService { get; set; }

		[Inject]
		public int triggerRewardSaleDefID { get; set; }

		[Inject]
		public UpdateSaleBadgeSignal updateSaleBadgeSignal { get; set; }

		public override void Execute()
		{
			List<Sale> instancesByType = playerService.GetInstancesByType<Sale>();
			List<SalePackDefinition> all = definitionService.GetAll<SalePackDefinition>();
			List<Sale> salesToSchedule = new List<Sale>();
			int quantity = (int)playerService.GetQuantity(StaticItem.LEVEL_ID);
			if (quantity < 1)
			{
				return;
			}
			foreach (SalePackDefinition item in all)
			{
				ProcessSale(item, instancesByType, ref salesToSchedule);
			}
			upsellService.ScheduleSales(salesToSchedule);
			updateSaleBadgeSignal.Dispatch();
		}

		private void ProcessSale(SalePackDefinition def, List<Sale> playerSales, ref List<Sale> salesToSchedule)
		{
			SalePackType type = def.Type;
			if (type == SalePackType.Redeemable)
			{
				return;
			}
			int iD = def.ID;
			Sale sale = upsellService.GetSaleInstanceFromID(playerSales, iD);
			if (sale != null && sale.Finished && !sale.Viewed)
			{
				sale.Viewed = true;
			}
			if (type == SalePackType.Upsell && PackUtil.HasPurchasedEnough(def, playerService))
			{
				if (sale != null)
				{
					playerService.Remove(sale);
				}
				definitionService.Remove(def);
				return;
			}
			if (def.UTCEndDate != 0 && timeService.CurrentTime() > def.UTCEndDate)
			{
				if (sale == null)
				{
					definitionService.Remove(def);
					return;
				}
				if (def.Duration <= 0 || sale.Finished)
				{
					playerService.Remove(sale);
					definitionService.Remove(def);
					return;
				}
			}
			if (sale == null)
			{
				if (def.UnlockByTrigger && triggerRewardSaleDefID != iD)
				{
					return;
				}
				sale = upsellService.AddNewSaleInstance(def);
			}
			else if (sale.Finished && !PackUtil.HasPurchasedEnough(def, playerService))
			{
				if (def.UnlockByTrigger && triggerRewardSaleDefID != iD)
				{
					return;
				}
				sale.Started = false;
				sale.UTCUserStartTime = 0;
				sale.Finished = false;
			}
			if (upsellService.IsSaleUpsellInstance(sale))
			{
				salesToSchedule.Add(sale);
			}
		}
	}
}
