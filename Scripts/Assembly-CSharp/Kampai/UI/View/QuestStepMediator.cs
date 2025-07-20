using Elevation.Logging;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.context.api;
using strange.extensions.injector.api;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class QuestStepMediator : Mediator
	{
		public const float PLACEMENT_ZOOM_LEVEL = 0.4f;

		public IKampaiLogger logger = LogManager.GetClassLogger("QuestStepMediator") as IKampaiLogger;

		private IQuestController questController;

		private IQuestStepController questStepController;

		[Inject]
		public QuestStepView view { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public CloseQuestBookSignal closeSignal { get; set; }

		[Inject]
		public QuestStepRushSignal stepRushSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal globalSFXSignal { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public FTUEQuestFinished ftueQuestFinished { get; set; }

		[Inject]
		public DeliverTaskItemSignal deliverTaskItemSignal { get; set; }

		[Inject]
		public RushRevealBuildingSignal rushRevealBuildingSignal { get; set; }

		[Inject]
		public ConstructionCompleteSignal constructionCompleteSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject]
		public IGoToService goToService { get; set; }

		[Inject]
		public CreateMasterPlanComponentSignal createComponentSignal { get; set; }

		[Inject]
		public CreateMasterPlanSignal createMasterPlanSignal { get; set; }

		[Inject]
		public IMasterPlanService masterPlanService { get; set; }

		[Inject]
		public VillainLairModel villainLairModel { get; set; }

		[Inject]
		public UIModel uiModel { get; set; }

		public override void OnRegister()
		{
			view.Init(playerService, timeEventService, localService);
			view.taskPurchaseButton.ClickedSignal.AddListener(TaskButtonClicked);
			view.buildButton.ClickedSignal.AddListener(TaskButtonClicked);
			view.taskActionButton.ClickedSignal.AddListener(TaskButtonClicked);
			view.taskStateButton.ClickedSignal.AddListener(GoToClicked);
			view.taskIconImage.ClickedSignal.AddListener(GoToClicked);
			view.taskSecondaryImage.ClickedSignal.AddListener(GoToClicked);
			view.goToLairButton.ClickedSignal.AddListener(GoToClicked);
			constructionCompleteSignal.AddListener(ConstructionComplete);
			closeSignal.AddListener(OnCloseQuestBook);
			Init();
		}

		public override void OnRemove()
		{
			view.taskActionButton.ClickedSignal.RemoveListener(TaskButtonClicked);
			view.buildButton.ClickedSignal.RemoveListener(TaskButtonClicked);
			view.taskPurchaseButton.ClickedSignal.RemoveListener(TaskButtonClicked);
			view.taskStateButton.ClickedSignal.RemoveListener(GoToClicked);
			view.taskIconImage.ClickedSignal.RemoveListener(GoToClicked);
			view.taskSecondaryImage.ClickedSignal.RemoveListener(GoToClicked);
			view.goToLairButton.ClickedSignal.RemoveListener(GoToClicked);
			constructionCompleteSignal.RemoveListener(ConstructionComplete);
			closeSignal.RemoveListener(OnCloseQuestBook);
		}

		private void Init()
		{
			if (QuestControllerValidating())
			{
				view.SetupStepAction(questStepController.GetStepAction(localService));
				view.SetupStepDesc(questStepController.GetStepDescription(localService, definitionService));
				Sprite mainSprite = null;
				Sprite maskSprite = null;
				questStepController.GetStepDescIcon(definitionService, out mainSprite, out maskSprite);
				view.SetupTaskDescIcon(mainSprite, maskSprite, questStepController.StepType == QuestStepType.Leisure);
				UpdateQuestState();
				CheckSizing();
			}
		}

		private void CheckSizing()
		{
			BuildingRepairQuestStepController buildingRepairQuestStepController = questStepController as BuildingRepairQuestStepController;
			if (buildingRepairQuestStepController == null)
			{
				return;
			}
			if (questStepController.StepState == QuestStepState.Complete || questStepController.NeedActiveProgressBar)
			{
				if (view.taskIconFullSize)
				{
					view.ReducePanelWithIconSize();
				}
			}
			else
			{
				view.FillPanelWIthIcon();
			}
		}

		private bool QuestControllerValidating()
		{
			IQuestController obj = this.questController ?? questService.GetQuestControllerByInstanceID(view.questInstanceID);
			IQuestController questController = obj;
			this.questController = obj;
			if (questController == null)
			{
				logger.Error("Quest Controller Doesn't exist for instance {0}", view.questInstanceID);
				return false;
			}
			questStepController = questStepController ?? this.questController.GetStepController(view.stepNumber);
			if (questStepController == null)
			{
				logger.Error("Step Controller Doesn't Exist For Quest {0} Index {1}", this.questController.ID, view.stepNumber);
				return false;
			}
			return true;
		}

		private void ConstructionComplete(int instanceId)
		{
			if (QuestControllerValidating() && questStepController.StepInstanceTrackedID == instanceId && questStepController.StepType == QuestStepType.Construction)
			{
				view.rushProgressPanel.gameObject.SetActive(false);
				UpdateQuestState();
			}
		}

		private void UpdateQuestState()
		{
			if (QuestControllerValidating())
			{
				QuestStepState stepState = questStepController.StepState;
				view.UpdateTaskStateButton(questStepController, villainLairModel);
				bool needActiveProgressBar = questStepController.NeedActiveProgressBar;
				bool needActiveDeliverButton = questStepController.NeedActiveDeliverButton;
				view.UpdateDeliverButton(needActiveDeliverButton);
				view.progressBar.gameObject.SetActive(needActiveProgressBar);
				if (needActiveProgressBar)
				{
					view.UpdateProgressBar(questStepController.ProgressBarAmount, questStepController.ProgressBarTotal);
				}
				if (needActiveDeliverButton)
				{
					SetupDeliverButton();
				}
				else if (stepState == QuestStepState.WaitComplete)
				{
					view.UpdateTaskButton(false, true, 0, "Done");
				}
				if (questController != null && !questController.AreAllStepsComplete && stepState == QuestStepState.Complete)
				{
					globalSFXSignal.Dispatch("Play_completePartQuest_01");
				}
			}
		}

		private void SetupDeliverButton()
		{
			ItemDefinition definition;
			if (!definitionService.TryGet<ItemDefinition>(questStepController.ItemDefinitionID, out definition) && (questStepController.StepType == QuestStepType.MasterPlanComponentBuild || questStepController.StepType == QuestStepType.MasterPlanBuild))
			{
				view.UpdateTaskButton(false, true, 0, questStepController.DeliverButtonLocKey);
				if (!questStepController.NeedGoToButton)
				{
					view.ToggleGoToButton(false);
				}
				return;
			}
			int amountNeeded = questStepController.AmountNeeded;
			bool flag = amountNeeded > 0;
			int cost = (flag ? Mathf.FloorToInt((float)amountNeeded * definition.BasePremiumCost) : 0);
			view.UpdateTaskButton(flag, true, cost, questStepController.DeliverButtonLocKey);
			if (flag)
			{
				view.purchaseDeliverLeftText.text = string.Format("+{0}", amountNeeded);
			}
			else
			{
				view.purchaseDeliverLeftText.gameObject.SetActive(false);
			}
			UIUtils.SetItemIcon(view.purchaseDeliverIconImage, definition);
			view.CheckIfItemIsOneOffCraftable(definition);
		}

		private void GoToClicked()
		{
			if (goToService != null && !uiModel.GoToClicked)
			{
				uiModel.GoToClicked = true;
				goToService.GoToClicked(view.step, view.stepDefinition, questController, view.stepNumber);
			}
		}

		private void ConstructionTransactionCallback(PendingCurrencyTransaction pct)
		{
			if (pct.Success)
			{
				globalSFXSignal.Dispatch("Play_button_premium_01");
				timeEventService.RushEvent(view.step.TrackedID);
				if (questController.GetStepController(view.stepNumber).StepState == QuestStepState.Ready)
				{
					questController.AutoGrantReward = true;
				}
				stepRushSignal.Dispatch(new Tuple<int, int>(questController.Definition.ID, view.stepNumber));
				UpdateQuestState();
				rushRevealBuildingSignal.Dispatch(view.step.TrackedID);
			}
		}

		private void TaskButtonClicked()
		{
			if ((!view.taskPurchaseButton.gameObject.activeSelf || view.taskPurchaseButton.isDoubleConfirmed()) && !OnMasterPlanQuestClick())
			{
				if (view.stepDefinition.Type == QuestStepType.Construction && view.step.state == QuestStepState.Inprogress)
				{
					playerService.ProcessRush(view.constructionRushCost, true, ConstructionTransactionCallback, view.stepDefinition.ItemDefinitionID);
				}
				else
				{
					deliverTaskItemSignal.Dispatch(new Tuple<int, int>(questController.ID, view.stepNumber));
					UpdateQuestState();
					ftueQuestFinished.Dispatch();
				}
				soundFXSignal.Dispatch("Play_button_click_01");
			}
		}

		private bool OnMasterPlanQuestClick()
		{
			ICrossContextInjectionBinder injectionBinder = gameContext.injectionBinder;
			if (view.stepDefinition.Type == QuestStepType.MasterPlanComponentBuild || view.stepDefinition.Type == QuestStepType.MasterPlanBuild)
			{
				MasterPlanQuestType.Component component = view.quest as MasterPlanQuestType.Component;
				if (component != null)
				{
					MasterPlanDefinition definition = component.masterPlan.Definition;
					closeSignal.Dispatch();
					if (component.component == null)
					{
						createMasterPlanSignal.Dispatch(definition);
						UpdateQuestState();
						injectionBinder.GetInstance<QuestHarvestableSignal>().Dispatch(view.quest);
						UpdateWayFinder();
						return true;
					}
					createComponentSignal.Dispatch(definition, component.index);
					UpdateQuestState();
					injectionBinder.GetInstance<QuestHarvestableSignal>().Dispatch(view.quest);
					UpdateWayFinder();
					return true;
				}
			}
			MasterPlanQuestStepController masterPlanQuestStepController = questStepController as MasterPlanQuestStepController;
			if (masterPlanQuestStepController != null)
			{
				MasterPlanComponent component2 = masterPlanQuestStepController.Component;
				if (view.taskPurchaseButton.gameObject.activeSelf)
				{
					injectionBinder.GetInstance<MasterPlanRushTaskSignal>().Dispatch(component2, view.stepNumber);
				}
				else
				{
					injectionBinder.GetInstance<MasterPlanTaskCompleteSignal>().Dispatch(component2, view.stepNumber);
				}
				gameContext.injectionBinder.GetInstance<MasterPlanComponentTaskUpdatedSignal>().Dispatch(component2);
				if (view.quest.state == QuestState.Harvestable)
				{
					injectionBinder.GetInstance<QuestHarvestableSignal>().Dispatch(view.quest);
				}
				UpdateQuestState();
				return true;
			}
			return false;
		}

		private void UpdateWayFinder()
		{
			masterPlanService.SetWayfinderState();
		}

		private void OnCloseQuestBook()
		{
			view.taskActionButton.GetComponent<Button>().interactable = false;
			view.buildButton.GetComponent<Button>().interactable = false;
			view.taskPurchaseButton.GetComponent<Button>().interactable = false;
			view.taskStateButton.GetComponent<Button>().interactable = false;
			view.taskIconImage.EnableClick(false);
			view.taskSecondaryImage.EnableClick(false);
			view.goToLairButton.GetComponent<Button>().interactable = false;
		}
	}
}
