using System.Collections.Generic;
using Kampai.Game;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.Util
{
	public class DebugQuestView : KampaiView
	{
		public RectTransform content;

		private GameObject QuestLinePrefab;

		private GameObject QuestButtonPrefab;

		private float questLineHeight = 80f;

		private List<DebugQuestLineView> views = new List<DebugQuestLineView>();

		private bool show;

		private bool questLineSetup;

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public IDefinitionService defService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		protected override void Start()
		{
			base.Start();
			RectTransform rectTransform = base.transform as RectTransform;
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;
			QuestLinePrefab = KampaiResources.Load<GameObject>("QuestLineDebugPanel");
			QuestButtonPrefab = KampaiResources.Load<GameObject>("QuestDebugButton");
			gameContext.injectionBinder.GetInstance<UpdateQuestWorldIconsSignal>().AddListener(UpdateQuestState);
			base.gameObject.SetActive(false);
		}

		public void Toggle()
		{
			if (!questLineSetup)
			{
				SetupQuestLines();
				questLineSetup = true;
			}
			if (show)
			{
				gameContext.injectionBinder.GetInstance<UpdateQuestWorldIconsSignal>().RemoveListener(UpdateQuestState);
				base.gameObject.SetActive(false);
				show = false;
			}
			else
			{
				gameContext.injectionBinder.GetInstance<UpdateQuestWorldIconsSignal>().AddListener(UpdateQuestState);
				base.gameObject.SetActive(true);
				show = true;
			}
		}

		private void UpdateQuestState(Quest quest)
		{
			UpdateQuestLines();
		}

		private void UpdateQuestLines()
		{
			foreach (DebugQuestLineView view in views)
			{
				view.UpdateQuestLine();
			}
		}

		private void SetupQuestLines()
		{
			Dictionary<int, QuestLine> questLines = questService.GetQuestLines();
			int count = questLines.Count;
			content.sizeDelta = new Vector2(0f, questLineHeight * (float)count);
			int num = 0;
			foreach (KeyValuePair<int, QuestLine> item in questLines)
			{
				QuestLine value = item.Value;
				GameObject gameObject = Object.Instantiate(QuestLinePrefab);
				RectTransform rectTransform = gameObject.transform as RectTransform;
				rectTransform.SetParent(content, false);
				rectTransform.anchoredPosition = new Vector2(0f, (0f - questLineHeight) * (float)num);
				rectTransform.sizeDelta = new Vector2(0f, questLineHeight);
				DebugQuestLineView component = gameObject.GetComponent<DebugQuestLineView>();
				component.AddQuestLine(value, QuestButtonPrefab, questService, defService);
				views.Add(component);
				num++;
			}
		}
	}
}
