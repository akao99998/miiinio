using Kampai.Game;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class DisplayMinionLevelTokenCommand : Command
	{
		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		public override void Execute()
		{
			VillainLairEntranceBuilding byInstanceId = playerService.GetByInstanceId<VillainLairEntranceBuilding>(374);
			uint quantity = playerService.GetQuantity(StaticItem.PENDING_MINION_LEVEL_TOKEN);
			if (byInstanceId != null && quantity != 0)
			{
				IGUICommand command = guiService.BuildCommand(GUIOperation.Load, "cmp_MinionLevelToken");
				guiService.Execute(command);
			}
		}
	}
}
