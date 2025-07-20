using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class StartProductionBuffCommand : Command
	{
		[Inject]
		public float multiplier { get; set; }

		[Inject]
		public int currentTime { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		public override void Execute()
		{
			timeEventService.StartBuff(TimeEventType.ProductionBuff, multiplier, currentTime);
		}
	}
}
