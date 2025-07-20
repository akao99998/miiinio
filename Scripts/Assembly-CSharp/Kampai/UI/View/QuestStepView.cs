using System.Collections.Generic;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class QuestStepView : KampaiView
	{
		[Header("Task Name / Icon")]
		public Text taskNameText;

		public Text taskActionText;

		public KampaiClickableImage taskIconImage;

		public KampaiClickableImage taskSecondaryImage;

		public Vector2 taskIconFullSizeMin;

		public Vector2 taskIconFullSizeMax;

		private Vector2 originalTaskIconMinAnchor;

		private Vector2 originalTaskIconMaxAnchor;

		[Header("Task State Button")]
		public ScrollableButtonView taskStateButton;

		[Header("Deliver/Harvest Panel")]
		public GameObject deliverPanel;

		public KampaiImage purchaseDeliverIconImage;

		public Text purchaseDeliverLeftText;

		[Header("Rush Progress Panel")]
		public RectTransform rushProgressPanel;

		public Text rushProgressText;

		[Header("Progress Bar")]
		public RectTransform progressBar;

		public Image progressFill;

		public Text progressText;

		[Header("Task")]
		public ScrollableButtonView taskPurchaseButton;

		public ScrollableButtonView taskActionButton;

		public Image taskButtonCurrencyImage;

		public Text taskButtonCostText;

		public Text taskButtonCompleteText;

		[Header("Complete")]
		public Text completeText;

		[Header("Build Master Plan")]
		public ScrollableButtonView buildButton;

		public ScrollableButtonView goToLairButton;

		internal int stepNumber;

		internal int questInstanceID;

		internal Quest quest;

		internal QuestStep step;

		internal QuestStepDefinition stepDefinition;

		internal int constructionRushCost;

		internal int collectionRushCost;

		internal int collectionAmountNeeded;

		private IPlayerService playerService;

		private ITimeEventService timeEventService;

		private ILocalizationService localService;

		private Vector3 originalGoToScale;

		private Vector3 originalDeliverScale;

		private bool hideTaskButton;

		internal bool taskIconFullSize;

		internal void Init(IPlayerService playerService, ITimeEventService timeEventService, ILocalizationService localService)
		{
			RectTransform rectTransform = taskIconImage.rectTransform;
			originalTaskIconMinAnchor = rectTransform.anchorMin;
			originalTaskIconMaxAnchor = rectTransform.anchorMax;
			this.playerService = playerService;
			this.timeEventService = timeEventService;
			this.localService = localService;
			rushProgressPanel.gameObject.SetActive(false);
			progressBar.gameObject.SetActive(true);
			taskActionButton.gameObject.SetActive(false);
			taskPurchaseButton.gameObject.SetActive(false);
			completeText.gameObject.SetActive(false);
			buildButton.gameObject.SetActive(false);
			goToLairButton.gameObject.SetActive(false);
			originalGoToScale = taskStateButton.transform.localScale;
			originalDeliverScale = taskActionButton.transform.localScale;
			taskPurchaseButton.EnableDoubleConfirm();
		}

		internal void FillPanelWIthIcon()
		{
			RectTransform rectTransform = taskIconImage.rectTransform;
			rectTransform.anchorMin = taskIconFullSizeMin;
			rectTransform.anchorMax = taskIconFullSizeMax;
			taskIconFullSize = true;
		}

		internal void ReducePanelWithIconSize()
		{
			if (taskIconFullSize)
			{
				RectTransform rectTransform = taskIconImage.rectTransform;
				rectTransform.anchorMin = originalTaskIconMinAnchor;
				rectTransform.anchorMax = originalTaskIconMaxAnchor;
			}
		}

		internal void SetTaskButtonState(bool hideTaskButton)
		{
			this.hideTaskButton = hideTaskButton;
		}

		public void SetupTaskDescIcon(Sprite mainSprite, Sprite maskSprite, bool isLeisure = false)
		{
			taskIconImage.sprite = mainSprite;
			taskIconImage.maskSprite = maskSprite;
			taskSecondaryImage.gameObject.SetActive(isLeisure);
			if (isLeisure)
			{
				taskSecondaryImage.sprite = UIUtils.LoadSpriteFromPath("img_throwparty_fill");
				taskSecondaryImage.maskSprite = UIUtils.LoadSpriteFromPath("img_throwparty_mask");
			}
		}

		public void Update()
		{
			if (step.TrackedID != 0 && step.state == QuestStepState.Inprogress && stepDefinition.Type == QuestStepType.Construction && stepDefinition.ItemAmount == 1)
			{
				int timeRemaining = timeEventService.GetTimeRemaining(step.TrackedID);
				if (UpdateRushProgressBar(timeRemaining))
				{
					int eventDuration = timeEventService.GetEventDuration(step.TrackedID);
					int num = eventDuration - timeRemaining;
					UpdateProgressBar(num, eventDuration, true);
				}
			}
		}

		internal void SetupStepAction(string actionText)
		{
			taskActionText.text = actionText;
		}

		internal void SetupStepDesc(string desc)
		{
			taskNameText.text = desc;
		}

		private void EnableStateButton(bool enable)
		{
			bool flag = EnableProgressiveGotoButton(enable);
			taskStateButton.gameObject.SetActive(flag);
			taskIconImage.EnableClick(flag);
			taskSecondaryImage.EnableClick(flag);
		}

		private int GetCompleteStepCount()
		{
			IList<QuestStep> steps = quest.Steps;
			for (int num = steps.Count - 1; num >= 0; num--)
			{
				QuestStep questStep = steps[num];
				if (questStep.state == QuestStepState.Complete)
				{
					return num + 1;
				}
			}
			return 0;
		}

		private bool EnableProgressiveGotoButton(bool enable)
		{
			if (!quest.Definition.ProgressiveGoto)
			{
				return enable;
			}
			if (stepNumber == GetCompleteStepCount())
			{
				return enable;
			}
			return false;
		}

		public void UpdateTaskStateButton(IQuestStepController questStepController, VillainLairModel villainLairModel)
		{
			EnableStateButton(true);
			switch (questStepController.StepState)
			{
			case QuestStepState.Notstarted:
				EnableStateButton(true);
				break;
			case QuestStepState.Inprogress:
				break;
			case QuestStepState.Ready:
				UpdateTaskReadyStateButton(questStepController, villainLairModel);
				break;
			case QuestStepState.WaitComplete:
				ToggleGoToButton(false);
				break;
			case QuestStepState.Complete:
				EnableStateButton(false);
				completeText.text = localService.GetString("Complete");
				completeText.gameObject.SetActive(true);
				break;
			case QuestStepState.RunningStartScript:
			case QuestStepState.RunningCompleteScript:
				break;
			}
		}

		private void UpdateTaskReadyStateButton(IQuestStepController questStepController, VillainLairModel villainLairModel)
		{
			switch (questStepController.StepType)
			{
			case QuestStepType.Delivery:
				EnableStateButton(false);
				break;
			case QuestStepType.Construction:
				completeText.text = localService.GetString("Ready");
				completeText.gameObject.SetActive(true);
				break;
			case QuestStepType.MasterPlanComponentBuild:
			case QuestStepType.MasterPlanBuild:
				if (villainLairModel.currentActiveLair != null)
				{
					buildButton.gameObject.SetActive(true);
					goToLairButton.gameObject.SetActive(false);
					taskStateButton.gameObject.SetActive(false);
				}
				else
				{
					taskStateButton.gameObject.SetActive(true);
					buildButton.gameObject.SetActive(false);
					goToLairButton.gameObject.SetActive(true);
				}
				break;
			case QuestStepType.MasterPlanTask:
				ToggleGoToButton(questStepController.NeedGoToButton);
				break;
			}
		}

		public void ToggleGoToButton(bool isEnabled)
		{
			bool active = EnableProgressiveGotoButton(isEnabled);
			taskStateButton.gameObject.SetActive(active);
		}

		public void UpdateTaskButton(bool isPremium, bool show = true, int cost = 0, string locKey = null)
		{
			if (isPremium)
			{
				taskActionButton.gameObject.SetActive(false);
			}
			else
			{
				taskPurchaseButton.gameObject.SetActive(false);
			}
			if (!show || hideTaskButton)
			{
				taskPurchaseButton.gameObject.SetActive(false);
				taskActionButton.gameObject.SetActive(false);
			}
			else if (!isPremium)
			{
				taskActionButton.gameObject.SetActive(true);
				taskButtonCompleteText.text = localService.GetString(locKey);
				taskButtonCompleteText.gameObject.SetActive(true);
				taskButtonCostText.gameObject.SetActive(false);
				taskButtonCurrencyImage.gameObject.SetActive(false);
			}
			else
			{
				taskPurchaseButton.gameObject.SetActive(true);
				taskButtonCostText.text = cost.ToString();
				taskButtonCurrencyImage.gameObject.SetActive(true);
				taskButtonCompleteText.gameObject.SetActive(false);
				taskButtonCostText.gameObject.SetActive(true);
			}
		}

		public void UpdateDeliverButton(bool isActive)
		{
			deliverPanel.gameObject.SetActive(isActive);
			if (!isActive)
			{
				UpdateTaskButton(false, false);
			}
		}

		public void CheckIfItemIsOneOffCraftable(ItemDefinition itemDef)
		{
			DynamicIngredientsDefinition dynamicIngredientsDefinition = itemDef as DynamicIngredientsDefinition;
			if (dynamicIngredientsDefinition == null)
			{
				return;
			}
			ICollection<CraftingBuilding> byDefinitionId = playerService.GetByDefinitionId<CraftingBuilding>(dynamicIngredientsDefinition.CraftingBuildingId);
			foreach (CraftingBuilding item in byDefinitionId)
			{
				if (item.RecipeInQueue.Contains(itemDef.ID) || item.CompletedCrafts.Contains(itemDef.ID))
				{
					taskPurchaseButton.gameObject.SetActive(false);
					taskActionButton.gameObject.SetActive(false);
					break;
				}
			}
		}

		private bool UpdateRushProgressBar(int timeRemaining)
		{
			if (rushProgressPanel == null || rushProgressText == null)
			{
				return false;
			}
			rushProgressPanel.gameObject.SetActive(true);
			if (timeRemaining < 0)
			{
				UpdateTaskButton(false, false);
				rushProgressText.text = UIUtils.FormatTime(0, localService);
				return false;
			}
			rushProgressText.text = UIUtils.FormatTime(timeRemaining, localService);
			constructionRushCost = timeEventService.CalculateRushCostForTimer(timeRemaining, RushActionType.CONSTRUCTION);
			int cost = constructionRushCost;
			UpdateTaskButton(true, true, cost);
			return true;
		}

		public void UpdateProgressBar(float progress, float complete, bool isTimer = false)
		{
			float num = Mathf.Min(progress, complete);
			float num2 = num / complete;
			if (isTimer)
			{
				progressText.text = string.Format("{0}%", (int)(num2 * 100f));
			}
			else
			{
				progressText.text = localService.GetString("OfComplete", num, complete);
				progressText.text = progressText.text.Replace(" ", string.Empty);
			}
			progressFill.rectTransform.anchorMax = new Vector2(num2, 1f);
		}

		internal void HighlightGoTo(bool isHighlighted)
		{
			Animator[] componentsInChildren = taskStateButton.GetComponentsInChildren<Animator>();
			if (isHighlighted)
			{
				Animator[] array = componentsInChildren;
				foreach (Animator animator in array)
				{
					animator.enabled = false;
				}
				TweenUtil.Throb(taskStateButton.transform, 0.85f, 0.5f, out originalGoToScale);
				return;
			}
			Go.killAllTweensWithTarget(taskStateButton.transform);
			taskStateButton.transform.localScale = originalGoToScale;
			Animator[] array2 = componentsInChildren;
			foreach (Animator animator2 in array2)
			{
				animator2.enabled = true;
			}
		}

		internal void HighlightDeliver(bool isHighlighted)
		{
			Animator[] componentsInChildren = taskActionButton.GetComponentsInChildren<Animator>();
			if (isHighlighted)
			{
				Animator[] array = componentsInChildren;
				foreach (Animator animator in array)
				{
					animator.enabled = false;
				}
				TweenUtil.Throb(taskActionButton.transform, 0.85f, 0.5f, out originalDeliverScale);
				return;
			}
			Go.killAllTweensWithTarget(taskActionButton.transform);
			taskActionButton.transform.localScale = originalDeliverScale;
			Animator[] array2 = componentsInChildren;
			foreach (Animator animator2 in array2)
			{
				animator2.enabled = true;
			}
		}
	}
}
