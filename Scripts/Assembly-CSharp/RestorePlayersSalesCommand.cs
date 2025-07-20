using System.Collections.Generic;
using Kampai.Game;
using strange.extensions.command.impl;

public class RestorePlayersSalesCommand : Command
{
	[Inject]
	public IPlayerService playerService { get; set; }

	[Inject]
	public ITimeEventService timeEventService { get; set; }

	[Inject]
	public ITimeService timeService { get; set; }

	[Inject]
	public MarketplaceItemSoldSignal marketplaceItemSoldSignal { get; set; }

	[Inject]
	public MarketplaceUpdateSoldItemsSignal updateSoldItemsSignal { get; set; }

	public override void Execute()
	{
		IList<MarketplaceSaleItem> instancesByType = playerService.GetInstancesByType<MarketplaceSaleItem>();
		foreach (MarketplaceSaleItem item in instancesByType)
		{
			int num = timeService.CurrentTime() - item.SaleStartTime;
			if (num > item.LengthOfSale)
			{
				item.state = MarketplaceSaleItem.State.SOLD;
			}
			else
			{
				timeEventService.AddEvent(item.ID, item.SaleStartTime, item.LengthOfSale, marketplaceItemSoldSignal);
			}
		}
		updateSoldItemsSignal.Dispatch(true);
	}
}
