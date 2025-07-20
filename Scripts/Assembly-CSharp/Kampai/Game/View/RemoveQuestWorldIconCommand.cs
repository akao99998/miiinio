using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Game.View
{
	public class RemoveQuestWorldIconCommand : Command
	{
		[Inject]
		public Quest Quest { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		[Inject]
		public RemoveQuestFromExistingWayFinderSignal removeQuestFromExistingWayFinderSignal { get; set; }

		public override void Execute()
		{
			if (Quest.state != QuestState.Complete && Quest.GetActiveDefinition().QuestVersion != -1)
			{
				int questIconTrackedInstanceId = Quest.QuestIconTrackedInstanceId;
				int num = 0;
				if (questIconTrackedInstanceId != 0)
				{
					num = prestigeService.ResolveTrackedId(questIconTrackedInstanceId);
				}
				int type = ((num != 0) ? num : questIconTrackedInstanceId);
				removeQuestFromExistingWayFinderSignal.Dispatch(Quest.GetActiveDefinition().ID, type);
			}
		}
	}
}
