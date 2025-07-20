using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class QuestPanelView : PopupMenuView
	{
		public RectTransform taskScrollViewTransform;

		public RectTransform questTabScrollViewTransform;

		public RectTransform rewardsPopupTransform;

		public Text questName;

		public QuestPanelProgressBarView questPanelProgressBar;

		public QuestLineProgressBarView questLineProgressBar;

		public CurrentQuestView currentQuestView;

		public RectTransform currencyRewardList;

		public RectTransform itemRewardList;

		public QuestPopupButtonView RewardItemButton;

		internal IQuestService questService;

		internal ILocalizationService localizationService;

		internal ITimeService timeService;

		internal ModalSettings modalSettings;

		private float taskPanelWidth;

		private float normalHeight;

		private float questBookPadding;

		private GameObject taskPanelPrefab;

		private IList<GameObject> questStepViews = new List<GameObject>();

		private IList<QuestView> questTabs = new List<QuestView>();

		internal void CreateQuestSteps(Quest quest, TransactionDefinition rewardTransaction, IDefinitionService definitionService)
		{
			foreach (GameObject questStepView in questStepViews)
			{
				Object.Destroy(questStepView);
			}
			questStepViews.Clear();
			SetupQuestPanelInfo(quest.GetActiveDefinition(), rewardTransaction, definitionService);
			if (quest.GetActiveDefinition().SurfaceType == QuestSurfaceType.LimitedEvent || quest.GetActiveDefinition().SurfaceType == QuestSurfaceType.TimedEvent)
			{
				questPanelProgressBar.gameObject.SetActive(true);
				SetupQuestProgressBar(quest);
			}
			else
			{
				questPanelProgressBar.gameObject.SetActive(false);
			}
			IList<QuestStep> steps = quest.Steps;
			if (steps == null || steps.Count == 0)
			{
				return;
			}
			int count = quest.GetActiveDefinition().QuestSteps.Count;
			for (int i = 0; i < count; i++)
			{
				GameObject gameObject = Object.Instantiate(taskPanelPrefab);
				Transform transform = gameObject.transform;
				transform.SetParent(taskScrollViewTransform, false);
				RectTransform rectTransform = transform as RectTransform;
				rectTransform.localPosition = Vector3.zero;
				rectTransform.localScale = Vector3.one;
				rectTransform.offsetMin = new Vector2(taskPanelWidth * (float)i, 0f);
				rectTransform.offsetMax = new Vector2(taskPanelWidth * (float)(i + 1), 0f);
				QuestStepView component = gameObject.GetComponent<QuestStepView>();
				component.questInstanceID = quest.ID;
				component.stepNumber = i;
				component.quest = quest;
				component.stepDefinition = quest.GetActiveDefinition().QuestSteps[i];
				component.step = steps[i];
				component.HighlightGoTo(true);
				if (modalSettings.enableDeliverThrob)
				{
					component.HighlightDeliver(true);
				}
				if (!modalSettings.enablePurchaseButtons)
				{
					component.SetTaskButtonState(true);
				}
				questStepViews.Add(gameObject);
			}
			taskScrollViewTransform.offsetMin = new Vector2(0f, 0f);
			taskScrollViewTransform.offsetMax = new Vector2((int)((float)quest.GetActiveDefinition().QuestSteps.Count * taskPanelWidth), 0f);
		}

		internal void SetupQuestProgressBar(Quest quest)
		{
			TimedQuestDefinition timedQuestDefinition = quest.GetActiveDefinition() as TimedQuestDefinition;
			if (timedQuestDefinition != null)
			{
				questPanelProgressBar.Init(quest.UTCQuestStartTime + timedQuestDefinition.Duration, timeService, localizationService);
			}
			LimitedQuestDefinition limitedQuestDefinition = quest.GetActiveDefinition() as LimitedQuestDefinition;
			if (limitedQuestDefinition != null)
			{
				questPanelProgressBar.Init(limitedQuestDefinition.ServerStopTimeUTC, timeService, localizationService);
			}
		}

		internal void RemoveCharacters()
		{
			currentQuestView.RemoveCoroutine();
			foreach (QuestView questTab in questTabs)
			{
				questTab.RemoveCoroutine();
			}
		}

		internal void CloseView()
		{
			base.Close();
		}

		private void ClearTabs()
		{
			foreach (QuestView questTab in questTabs)
			{
				Object.Destroy(questTab.gameObject);
			}
			questTabs.Clear();
		}

		internal void InitCurrentQuestImage(int characterDefId, IFancyUIService fancyUIService, DummyCharacterType characterType)
		{
			currentQuestView.Init(characterDefId, fancyUIService, characterType);
		}

		internal void SetCurrentQuestImage(int characterDefId, DummyCharacterType characterType)
		{
			currentQuestView.UpdateQuest(characterDefId, characterType);
		}

		internal void InitQuestTabs(List<Quest> quests, int selectedQuestID)
		{
			ClearTabs();
			GameObject gameObject = KampaiResources.Load("cmp_QuestBookIcon") as GameObject;
			QuestView component = gameObject.GetComponent<QuestView>();
			questBookPadding = component.PaddingInPixels;
			RectTransform rectTransform = gameObject.transform as RectTransform;
			normalHeight = rectTransform.sizeDelta.y;
			int num = 0;
			List<Quest> list = QuestUtils.ResolveQuests(quests);
			foreach (Quest item in list)
			{
				if (item.state == QuestState.RunningTasks && item.GetActiveDefinition().SurfaceType != QuestSurfaceType.ProcedurallyGenerated && item.ID != selectedQuestID)
				{
					InitQuestBookPrefab(gameObject, normalHeight, num, item);
					num++;
				}
			}
			questTabScrollViewTransform.sizeDelta = new Vector2(0f, (float)num * (normalHeight + questBookPadding));
		}

		internal void SwapQuest(Quest newQuest, int oldQuestID)
		{
			foreach (QuestView questTab in questTabs)
			{
				questTab.button.GetComponent<Button>().interactable = false;
				if (questTab.quest.ID == oldQuestID)
				{
					questTab.quest = newQuest;
					questTab.UpdateQuest(MoveHalfWayComplete);
				}
			}
		}

		private void MoveHalfWayComplete()
		{
			foreach (QuestView questTab in questTabs)
			{
				questTab.button.GetComponent<Button>().interactable = true;
			}
		}

		private void InitQuestBookPrefab(GameObject prefab, float height, int index, Quest quest)
		{
			GameObject gameObject = Object.Instantiate(prefab);
			Transform transform = gameObject.transform;
			transform.SetParent(questTabScrollViewTransform);
			RectTransform rectTransform = transform as RectTransform;
			rectTransform.localPosition = Vector3.zero;
			rectTransform.localScale = Vector3.one;
			float num = (normalHeight + questBookPadding) * (float)index;
			rectTransform.offsetMin = new Vector2(0f, num);
			rectTransform.offsetMax = new Vector2(0f, num + height);
			QuestView component = gameObject.GetComponent<QuestView>();
			component.quest = quest;
			questTabs.Add(component);
		}

		public override void Init()
		{
			base.Init();
			RectTransform rectTransform = base.transform as RectTransform;
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;
			float num = rectTransform.anchorMax.x - rectTransform.anchorMin.x;
			rectTransform = taskScrollViewTransform.parent as RectTransform;
			taskPanelWidth = (rectTransform.anchorMax.x - rectTransform.anchorMin.x) * num * (float)Screen.width / UIUtils.GetHeightScale() / 3f;
			taskPanelPrefab = KampaiResources.Load("cmp_TaskPanel") as GameObject;
			base.Open();
		}

		private void SetupQuestPanelInfo(QuestDefinition def, TransactionDefinition rewardTransaction, IDefinitionService definitionService)
		{
			if (def.LocalizedKey != null)
			{
				questName.text = questService.GetQuestName(def.LocalizedKey);
			}
			else
			{
				questName.text = " ";
			}
			if (rewardTransaction == null)
			{
				return;
			}
			string path = "cmp_QuestReward";
			GameObject gameObject = KampaiResources.Load(path) as GameObject;
			if (gameObject == null)
			{
				logger.Error("Unable to load QuestReward prefab.");
				return;
			}
			CleanUp();
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			foreach (QuantityItem output in rewardTransaction.Outputs)
			{
				if (num3 >= def.RewardDisplayCount)
				{
					continue;
				}
				DisplayableDefinition displayableDefinition = definitionService.Get<DisplayableDefinition>(output.ID);
				GameObject gameObject2 = Object.Instantiate(gameObject);
				GameObject gameObject3 = gameObject2.FindChild("txt_RewardCount");
				if (gameObject3 != null)
				{
					Text component = gameObject3.GetComponent<Text>();
					if (component != null)
					{
						switch (output.ID)
						{
						case 0:
							component.text = UIUtils.FormatLargeNumber(TransactionUtil.SumOutputsForStaticItem(rewardTransaction, StaticItem.GRIND_CURRENCY_ID));
							break;
						case 2:
							component.text = UIUtils.FormatLargeNumber(TransactionUtil.SumOutputsForStaticItem(rewardTransaction, StaticItem.XP_ID));
							break;
						default:
							component.text = UIUtils.FormatLargeNumber((int)output.Quantity);
							break;
						}
					}
				}
				GameObject gameObject4 = gameObject2.FindChild("icn_RewardItem");
				if (gameObject4 != null)
				{
					KampaiImage component2 = gameObject4.GetComponent<KampaiImage>();
					if (component2 != null)
					{
						component2.sprite = UIUtils.LoadSpriteFromPath(displayableDefinition.Image);
						component2.maskSprite = UIUtils.LoadSpriteFromPath(displayableDefinition.Mask);
					}
				}
				if (displayableDefinition.ID <= 2)
				{
					num++;
					gameObject2.transform.SetParent(currencyRewardList, false);
				}
				else
				{
					num2++;
					gameObject2.transform.SetParent(itemRewardList, false);
					RewardItemButton.questRewards.Add(displayableDefinition);
				}
				num3++;
			}
			currencyRewardList.gameObject.SetActive(num > 0);
			itemRewardList.gameObject.SetActive(num2 > 0);
		}

		private void CleanUp()
		{
			RewardItemButton.questRewards.Clear();
			foreach (Transform itemReward in itemRewardList)
			{
				Object.Destroy(itemReward.gameObject);
			}
			foreach (Transform currencyReward in currencyRewardList)
			{
				Object.Destroy(currencyReward.gameObject);
			}
		}
	}
}
