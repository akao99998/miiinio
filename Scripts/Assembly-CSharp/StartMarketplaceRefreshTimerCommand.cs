using Kampai.Game;
using strange.extensions.command.impl;

public class StartMarketplaceRefreshTimerCommand : Command
{
	[Inject]
	public IPlayerService playerService { get; set; }

	[Inject]
	public IDefinitionService definitionService { get; set; }

	[Inject]
	public ITimeService timeService { get; set; }

	[Inject]
	public bool isRush { get; set; }

	public override void Execute()
	{
		MarketplaceRefreshTimerDefinition def = definitionService.Get<MarketplaceRefreshTimerDefinition>(1000008093);
		MarketplaceRefreshTimer marketplaceRefreshTimer = playerService.GetFirstInstanceByDefinitionId<MarketplaceRefreshTimer>(1000008093);
		if (marketplaceRefreshTimer == null)
		{
			marketplaceRefreshTimer = new MarketplaceRefreshTimer(def);
			marketplaceRefreshTimer.UTCStartTime = timeService.CurrentTime();
			playerService.Add(marketplaceRefreshTimer);
		}
		if (isRush)
		{
			marketplaceRefreshTimer.UTCStartTime = timeService.CurrentTime();
		}
	}
}
