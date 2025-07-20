using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class RushMinionTaskCommand : Command
	{
		[Inject]
		public int minionID { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		public override void Execute()
		{
			timeEventService.RushEvent(minionID);
		}
	}
}
