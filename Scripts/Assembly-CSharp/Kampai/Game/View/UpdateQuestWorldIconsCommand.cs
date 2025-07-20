using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Game.View
{
	public class UpdateQuestWorldIconsCommand : Command
	{
		private int wayfinderTrackedId;

		[Inject]
		public Quest Quest { get; set; }

		[Inject]
		public IPrestigeService PrestigeService { get; set; }

		[Inject]
		public GetWayFinderSignal GetWayFinderSignal { get; set; }

		[Inject]
		public CreateWayFinderSignal CreateWayFinderSignal { get; set; }

		[Inject]
		public RemoveWayFinderSignal RemoveWayFinderSignal { get; set; }

		[Inject]
		public AddQuestToExistingWayFinderSignal AddQuestToExistingWayFinderSignal { get; set; }

		public override void Execute()
		{
			foreach (QuestStep step in Quest.Steps)
			{
				if (step.state == QuestStepState.Complete && CanShowWayfinder(Quest, step))
				{
					RemoveWayFinderSignal.Dispatch(step.TrackedID);
				}
			}
			if (Quest.state != QuestState.Complete && Quest.GetActiveDefinition().QuestVersion != -1)
			{
				int questIconTrackedInstanceId = Quest.QuestIconTrackedInstanceId;
				if (questIconTrackedInstanceId != 0)
				{
					wayfinderTrackedId = PrestigeService.ResolveTrackedId(questIconTrackedInstanceId);
				}
				GetWayFinderSignal.Dispatch(wayfinderTrackedId, GetWayFinder);
			}
		}

		private bool CanShowWayfinder(Quest quest, QuestStep step)
		{
			QuestDefinition definition = quest.Definition;
			if (definition.QuestSteps == null || definition.QuestSteps.Count <= 0 || quest.state == QuestState.Notstarted)
			{
				return false;
			}
			int num = quest.Steps.IndexOf(step);
			return num >= 0 && definition.QuestSteps[num].ShowWayfinder;
		}

		internal void GetWayFinder(int trackedId, IWayFinderView wayFinderView)
		{
			if (wayFinderView == null)
			{
				if (wayfinderTrackedId != 0)
				{
					if (Quest.Definition.ID == 77777)
					{
						CreateWayFinderSignal.Dispatch(new WayFinderSettings(Quest.Definition.ID, wayfinderTrackedId));
					}
					else
					{
						CreateWayFinderSignal.Dispatch(new WayFinderSettings(Quest.GetActiveDefinition().ID, wayfinderTrackedId));
					}
				}
			}
			else
			{
				AddQuestToExistingWayFinderSignal.Dispatch(Quest.GetActiveDefinition().ID, trackedId);
			}
			foreach (QuestStep step in Quest.Steps)
			{
				if (step.state != QuestStepState.Complete && CanShowWayfinder(Quest, step))
				{
					WayFinderSettings type = new WayFinderSettings(step.TrackedID);
					CreateWayFinderSignal.Dispatch(type);
				}
			}
		}
	}
}
