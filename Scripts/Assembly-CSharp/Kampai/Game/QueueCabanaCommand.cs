using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class QueueCabanaCommand : Command
	{
		[Inject]
		public Prestige prestige { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		public override void Execute()
		{
			if (prestigeService.GetEmptyCabana() != null)
			{
				prestigeService.ChangeToPrestigeState(prestige, PrestigeState.Questing);
			}
			else
			{
				playerService.QueueVillain(prestige);
			}
		}
	}
}
