using Kampai.Common;
using Kampai.Game;
using Kampai.Main;
using Kampai.UI.View;
using strange.extensions.command.impl;

public class RefreshSaleItemsCommand : Command
{
	private MarketplaceRefreshTimer refreshTimer;

	[Inject]
	public GenerateBuyItemsSignal generateBuyItemsSignal { get; set; }

	[Inject]
	public IPlayerService playerService { get; set; }

	[Inject]
	public ITimeService timeService { get; set; }

	[Inject]
	public SetPremiumCurrencySignal setPremiumCurrencySignal { get; set; }

	[Inject]
	public RefreshSaleItemsSuccessSignal refreshSuccessSignal { get; set; }

	[Inject]
	public RushRefreshTimerSuccessSignal rushSuccessSignal { get; set; }

	[Inject]
	public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

	[Inject]
	public ITelemetryService telemetryService { get; set; }

	[Inject]
	public RefreshSaleItemsSignalArgs args { get; set; }

	public override void Execute()
	{
		refreshTimer = playerService.GetFirstInstanceByDefinitionId<MarketplaceRefreshTimer>(1000008093);
		int cost = ((!args.RushCost.HasValue) ? refreshTimer.Definition.RushCost : args.RushCost.Value);
		if (args.RefreshItems)
		{
			playSFXSignal.Dispatch("Play_marketplace_slotStart_01");
			RefreshMarketplace();
		}
		else if (!args.StopSpinning)
		{
			playerService.ProcessRefreshMarket(cost, true, RushTransactionCallback);
		}
	}

	private void RushTransactionCallback(PendingCurrencyTransaction pct)
	{
		if (pct.Success)
		{
			refreshTimer.UTCStartTime = timeService.CurrentTime() - refreshTimer.Definition.RefreshTimeSeconds;
			rushSuccessSignal.Dispatch();
			setPremiumCurrencySignal.Dispatch();
		}
	}

	private void RefreshMarketplace()
	{
		generateBuyItemsSignal.Dispatch();
		refreshSuccessSignal.Dispatch();
		telemetryService.Send_Telemtry_EVT_MARKETPLACE_VIEWED("REFRESH");
	}
}
