using System.Collections.Generic;
using Kampai.Game;
using Kampai.Main;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class QuestLineProgressBarMediator : Mediator
	{
		[Inject]
		public QuestLineProgressBarView view { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public UpdateQuestPanelWithNewQuestSignal updateQuestPanelSignal { get; set; }

		[Inject]
		public UpdateQuestLineProgressSignal updateQuestLineProgressSignal { get; set; }

		[Inject]
		public QuestDetailIDSignal idSignal { get; set; }

		[Inject]
		public IMasterPlanQuestService masterPlanQuestService { get; set; }

		public override void OnRegister()
		{
			updateQuestLineProgressSignal.AddListener(OnQuestSelected);
			updateQuestPanelSignal.AddListener(OnQuestSelected);
			idSignal.AddListener(OnQuestSelected);
		}

		public override void OnRemove()
		{
			updateQuestLineProgressSignal.RemoveListener(OnQuestSelected);
			updateQuestPanelSignal.RemoveListener(OnQuestSelected);
			idSignal.RemoveListener(OnQuestSelected);
		}

		private void OnQuestSelected(int questId)
		{
			Quest questByInstanceId = masterPlanQuestService.GetQuestByInstanceId(questId);
			int questLineID = questByInstanceId.GetActiveDefinition().QuestLineID;
			string @string = localService.GetString("QuestLineTitle3");
			if (questLineID == 0)
			{
				view.UpdateProgress(0, 1);
				view.SetTitle(@string);
				return;
			}
			PrestigeDefinition definition = null;
			if (definitionService.TryGet<PrestigeDefinition>(questByInstanceId.GetActiveDefinition().SurfaceID, out definition))
			{
				string string2 = localService.GetString(definition.LocalizedKey);
				@string = localService.GetString("QuestLineTitle2", string2);
			}
			view.SetTitle(@string);
			Dictionary<int, QuestLine> questLines = questService.GetQuestLines();
			QuestLine questLine = questLines[questLineID];
			int totalCount = 0;
			int completeCount = 0;
			CheckQuestProgress(questLine, questByInstanceId.GetActiveDefinition().ID, out completeCount, out totalCount);
			completeCount += ((questByInstanceId.state == QuestState.Complete) ? 1 : 0);
			view.UpdateProgress(completeCount, totalCount);
		}

		private void CheckQuestProgress(QuestLine questLine, int questDefinitionID, out int completeCount, out int totalCount)
		{
			bool flag = false;
			totalCount = 0;
			completeCount = 0;
			for (int num = questLine.Quests.Count - 1; num >= 0; num--)
			{
				QuestDefinition questDefinition = questLine.Quests[num];
				if (questDefinition.ID == questDefinitionID)
				{
					flag = true;
				}
				if (questDefinition.QuestVersion != -1 && questDefinition.SurfaceType != QuestSurfaceType.Automatic)
				{
					if (!flag)
					{
						completeCount++;
					}
					totalCount++;
				}
			}
		}
	}
}
