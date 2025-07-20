using System.Collections.Generic;
using Kampai.Game;
using Kampai.UI.View;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.context.api;

namespace Kampai.Util
{
	public class QuestDebugButtonView : ButtonView
	{
		public Text ButtonName;

		public Image Background;

		private IQuestService questService;

		private QuestDefinition questDefinition;

		private IQuestController questController;

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		public override void OnClickEvent()
		{
			if (questController != null)
			{
				questController.GoToQuestState(QuestState.Complete);
				return;
			}
			Quest quest = new Quest(questDefinition);
			quest.Initialize();
			quest.state = QuestState.Complete;
			questController = questService.AddQuest(quest);
			gameContext.injectionBinder.GetInstance<GetNewQuestSignal>().Dispatch();
			SetState(questController);
		}

		public void UpdateQuestInfo()
		{
			Dictionary<int, IQuestController> questMap = questService.GetQuestMap();
			int iD = questDefinition.ID;
			if (!questMap.ContainsKey(iD))
			{
				Background.color = new Color(0f, 0f, 0f, 0.4f);
				SetText(string.Format("{0}\n{1}", iD.ToString(), "Doesn't exist"));
			}
			else
			{
				questController = questMap[iD];
				SetState(questController);
			}
		}

		public void AddQuest(QuestDefinition questDefinition, IQuestService questService)
		{
			this.questService = questService;
			this.questDefinition = questDefinition;
			Dictionary<int, IQuestController> questMap = questService.GetQuestMap();
			int iD = questDefinition.ID;
			if (!questMap.ContainsKey(iD))
			{
				Background.color = new Color(0f, 0f, 0f, 0.4f);
				SetText(string.Format("{0}\n{1}", iD.ToString(), "Doesn't exist"));
			}
			else
			{
				questController = questMap[iD];
				SetState(questController);
			}
		}

		private void SetState(IQuestController questCon)
		{
			switch (questCon.State)
			{
			case QuestState.Notstarted:
				Background.color = Color.gray;
				break;
			case QuestState.RunningStartScript:
			case QuestState.RunningTasks:
			case QuestState.RunningCompleteScript:
				Background.color = Color.green;
				break;
			case QuestState.Complete:
				Background.color = Color.blue;
				break;
			}
			SetText(string.Format("{0}\n{1}", questCon.Definition.ID.ToString(), questCon.State.ToString()));
		}

		public void SetText(string text)
		{
			ButtonName.text = text;
		}
	}
}
