using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class EnableAllMinionRenderersCommand : Command
	{
		[Inject]
		public bool enable { get; set; }

		[Inject]
		public EnableMinionRendererSignal enableSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		public override void Execute()
		{
			foreach (Minion item in playerService.GetInstancesByType<Minion>())
			{
				enableSignal.Dispatch(item.ID, enable);
			}
		}
	}
}
