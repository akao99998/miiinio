using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class StartGameTimeTrackingCommand : Command
	{
		[Inject]
		public IPlayerDurationService playerDurationService { get; set; }

		[Inject]
		public IGameTimeTracker gameTimeTracker { get; set; }

		public override void Execute()
		{
			gameTimeTracker.StartGameTime = playerDurationService.TotalGamePlaySeconds;
		}
	}
}
