using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public abstract class QuestStepController : IQuestStepController
	{
		protected IPlayerService playerService;

		protected IQuestScriptService questScriptService;

		protected ICrossContextCapable gameContext;

		protected IKampaiLogger logger;

		private Quest quest;

		protected readonly int stepIndex;

		public QuestStepType StepType
		{
			get
			{
				return questStepDefinition.Type;
			}
		}

		public virtual QuestStepState StepState
		{
			get
			{
				return questStep.state;
			}
		}

		public int StepInstanceTrackedID
		{
			get
			{
				return questStep.TrackedID;
			}
		}

		public int ItemDefinitionID
		{
			get
			{
				return questStepDefinition.ItemDefinitionID;
			}
		}

		public virtual string DeliverButtonLocKey
		{
			get
			{
				if (StepState == QuestStepState.WaitComplete)
				{
					return "Done";
				}
				return string.Empty;
			}
		}

		public virtual bool NeedActiveDeliverButton
		{
			get
			{
				return false;
			}
		}

		public virtual bool NeedActiveProgressBar
		{
			get
			{
				return true;
			}
		}

		public virtual bool NeedGoToButton
		{
			get
			{
				return true;
			}
		}

		public int ProgressBarAmount
		{
			get
			{
				if (StepState == QuestStepState.Complete || StepState == QuestStepState.WaitComplete)
				{
					return questStepDefinition.ItemAmount;
				}
				return questStep.AmountCompleted;
			}
		}

		public int ProgressBarTotal
		{
			get
			{
				return questStepDefinition.ItemAmount;
			}
		}

		public virtual int AmountNeeded
		{
			get
			{
				return 0;
			}
		}

		protected QuestStepDefinition questStepDefinition
		{
			get
			{
				return quest.GetActiveDefinition().QuestSteps[stepIndex];
			}
		}

		protected QuestStep questStep
		{
			get
			{
				return quest.Steps[stepIndex];
			}
			set
			{
				quest.Steps[stepIndex] = value;
			}
		}

		protected bool isProceduralQuest
		{
			get
			{
				return quest.GetActiveDefinition().SurfaceType == QuestSurfaceType.ProcedurallyGenerated;
			}
		}

		protected int QuestInstanceID
		{
			get
			{
				return quest.ID;
			}
		}

		public QuestStepController(Quest quest, int stepIndex, IQuestScriptService questScriptService, IPlayerService playerService, ICrossContextCapable gameContext, IKampaiLogger logger)
		{
			this.quest = quest;
			this.stepIndex = stepIndex;
			this.playerService = playerService;
			this.questScriptService = questScriptService;
			this.gameContext = gameContext;
			this.logger = logger;
			if (stepIndex >= quest.Steps.Count)
			{
				logger.Warning("Step Index is out of range for your current quest! {0} Setting it to 0", stepIndex);
				stepIndex = 0;
			}
		}

		public int IsTrackingOneOffCraftable(int itemDefinitionID)
		{
			int result = 0;
			if (questStepDefinition.ItemDefinitionID == itemDefinitionID && questStep.state == QuestStepState.Inprogress)
			{
				result = questStepDefinition.ItemAmount;
			}
			return result;
		}

		public void GoToNextState(bool isTaskComplete = false)
		{
			if (isTaskComplete)
			{
				quest.Steps[stepIndex].state = QuestStepState.Ready;
			}
			switch (quest.Steps[stepIndex].state)
			{
			case QuestStepState.Notstarted:
				if (questScriptService == null || !questScriptService.HasScript(quest, true, stepIndex, true))
				{
					InprogressStateCheck();
				}
				else
				{
					GoToTaskState(QuestStepState.RunningStartScript);
				}
				break;
			case QuestStepState.RunningStartScript:
				InprogressStateCheck();
				break;
			case QuestStepState.Inprogress:
				GoToTaskState(QuestStepState.Ready);
				break;
			case QuestStepState.Ready:
			case QuestStepState.WaitComplete:
				if (questScriptService == null || !questScriptService.HasScript(quest, false, stepIndex, true))
				{
					GoToTaskState(QuestStepState.Complete);
				}
				else
				{
					GoToTaskState(QuestStepState.RunningCompleteScript);
				}
				break;
			case QuestStepState.RunningCompleteScript:
				GoToTaskState(QuestStepState.Complete);
				break;
			case QuestStepState.Complete:
				break;
			}
		}

		public void GoToTaskState(QuestStepState targetState)
		{
			if (questScriptService != null)
			{
				QuestStepState state = questStep.state;
				questStep.state = targetState;
				switch (targetState)
				{
				case QuestStepState.RunningStartScript:
					questScriptService.StartQuestScript(quest, true, false, stepIndex, true);
					break;
				case QuestStepState.RunningCompleteScript:
					questScriptService.StartQuestScript(quest, false, false, stepIndex, true);
					break;
				}
				gameContext.injectionBinder.GetInstance<QuestTaskStateChangeSignal>().Dispatch(quest, stepIndex, state);
			}
		}

		private void InprogressStateCheck()
		{
			GoToTaskState(QuestStepState.Inprogress);
			if (StepType == QuestStepType.Construction && questStep.AmountCompleted >= questStepDefinition.ItemAmount)
			{
				GoToNextState(true);
			}
		}

		public virtual void SetupTracking()
		{
			questStep.TrackedID = questStepDefinition.ItemDefinitionID;
			if (questStep.TrackedID == 0)
			{
				logger.Fatal(FatalCode.QS_NO_SUCH_TRACKED_ID, "Item definition id not found for {0} Type quests", questStepDefinition.Type);
			}
		}

		public abstract void UpdateTask(QuestTaskTransition questTaskTransition, Building building, int buildingDefId, int itemDefId);

		public abstract string GetStepAction(ILocalizationService localService);

		public abstract string GetStepDescription(ILocalizationService localService, IDefinitionService defService);

		public abstract void GetStepDescIcon(IDefinitionService defService, out Sprite mainSprite, out Sprite maskSprite);
	}
}
