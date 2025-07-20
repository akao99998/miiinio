using System.Collections.Generic;
using Kampai.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.Util
{
	public class DebugQuestLineView : KampaiView
	{
		public Text QuestLineInfo;

		public Image background;

		public RectTransform Content;

		private QuestLine questLine;

		private float questWidth = 100f;

		private IDefinitionService defService;

		private List<QuestDebugButtonView> views = new List<QuestDebugButtonView>();

		public void AddQuestLine(QuestLine questLine, GameObject prefab, IQuestService questService, IDefinitionService defService)
		{
			this.defService = defService;
			this.questLine = questLine;
			SetQuestLineState(questLine.state);
			IList<QuestDefinition> quests = questLine.Quests;
			int count = quests.Count;
			Content.sizeDelta = new Vector2(questWidth * (float)count, 0f);
			for (int i = 0; i < count; i++)
			{
				GameObject gameObject = Object.Instantiate(prefab);
				RectTransform rectTransform = gameObject.transform as RectTransform;
				rectTransform.SetParent(Content, false);
				rectTransform.anchoredPosition = new Vector2((float)i * questWidth, 0f);
				rectTransform.sizeDelta = new Vector2(questWidth, 0f);
				QuestDebugButtonView component = gameObject.GetComponent<QuestDebugButtonView>();
				component.AddQuest(quests[count - 1 - i], questService);
				views.Add(component);
			}
			SetQuestLineInfo();
		}

		public void UpdateQuestLine()
		{
			foreach (QuestDebugButtonView view in views)
			{
				view.UpdateQuestInfo();
			}
			SetQuestLineState(questLine.state);
			SetQuestLineInfo();
		}

		private void SetQuestLineInfo()
		{
			int questLineID = questLine.Quests[0].QuestLineID;
			string text = questLine.state.ToString();
			if (questLine.GivenByCharacterID != 0)
			{
				PrestigeDefinition prestigeDefinition = defService.Get<PrestigeDefinition>(questLine.GivenByCharacterID);
				if (prestigeDefinition != null)
				{
					SetText(string.Format("{0}\n{1} {2} \n{3}", questLineID, prestigeDefinition.LocalizedKey, questLine.GivenByCharacterPrestigeLevel, text));
				}
				else
				{
					SetText(string.Format("{0}\n{1}", questLineID, text));
				}
			}
			else
			{
				SetText(string.Format("{0}\n{1}", questLineID, text));
			}
		}

		private void SetQuestLineState(QuestLineState state)
		{
			switch (state)
			{
			case QuestLineState.NotStarted:
				background.color = new Color(0f, 0f, 0f, 0.4f);
				break;
			case QuestLineState.Started:
				background.color = new Color(0f, 1f, 0f, 0.4f);
				break;
			case QuestLineState.Finished:
				background.color = new Color(1f, 1f, 1f, 0.4f);
				break;
			}
		}

		public void SetText(string text)
		{
			QuestLineInfo.text = text;
		}
	}
}
