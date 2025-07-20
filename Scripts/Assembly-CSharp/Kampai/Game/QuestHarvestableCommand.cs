using Elevation.Logging;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class QuestHarvestableCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("QuestHarvestableCommand") as IKampaiLogger;

		[Inject]
		public UpdateQuestBookBadgeSignal updateBadgeSignal { get; set; }

		[Inject(UIElement.CONTEXT)]
		public ICrossContextCapable uiContext { get; set; }

		[Inject]
		public UpdateQuestWorldIconsSignal updateQuestWorldIconsSignal { get; set; }

		[Inject]
		public Quest quest { get; set; }

		public override void Execute()
		{
			if (quest.state != QuestState.Harvestable)
			{
				logger.Warning("Trying to harvest quest when state is: {0}", quest.state);
				return;
			}
			updateBadgeSignal.Dispatch();
			if (quest.GetActiveDefinition().SurfaceType == QuestSurfaceType.Automatic || (quest.AutoGrantReward && quest.GetActiveDefinition().SurfaceType != QuestSurfaceType.ProcedurallyGenerated))
			{
				uiContext.injectionBinder.GetInstance<ShowQuestRewardSignal>().Dispatch(quest.ID);
			}
			else
			{
				updateQuestWorldIconsSignal.Dispatch(quest);
			}
		}
	}
}
