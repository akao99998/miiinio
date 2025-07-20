using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class UpdatePrestigeListCommand : Command
	{
		[Inject]
		public IPrestigeService prestigeService { get; set; }

		public override void Execute()
		{
			prestigeService.UpdateEligiblePrestigeList();
		}
	}
}
