using System.Collections.Generic;
using Kampai.Game.MasterPlanQuest;
using Kampai.Util;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class MasterPlanQuestController : IQuestController
	{
		private readonly IDefinitionService definitionService;

		private readonly ICrossContextCapable gameContext;

		private readonly IKampaiLogger logger;

		private readonly IPlayerService playerService;

		private readonly MasterPlanQuestType.Component quest;

		private readonly List<IQuestStepController> questStepControllers = new List<IQuestStepController>();

		public bool AreAllStepsComplete
		{
			get
			{
				foreach (QuestStep step in quest.Steps)
				{
					if (step.state != QuestStepState.Complete)
					{
						return false;
					}
				}
				return true;
			}
		}

		public bool AutoGrantReward
		{
			get
			{
				return quest.AutoGrantReward;
			}
			set
			{
				quest.AutoGrantReward = value;
			}
		}

		public QuestDefinition Definition
		{
			get
			{
				return quest.GetActiveDefinition();
			}
		}

		public int ID
		{
			get
			{
				return quest.ID;
			}
		}

		public Quest Quest
		{
			get
			{
				return quest;
			}
		}

		public int QuestIconTrackedInstanceID
		{
			get
			{
				return quest.QuestIconTrackedInstanceId;
			}
		}

		public QuestState State
		{
			get
			{
				return quest.state;
			}
		}

		public int StepCount
		{
			get
			{
				return quest.Steps.Count;
			}
		}

		protected QuestSurfaceType surfaceType
		{
			get
			{
				return Definition.SurfaceType;
			}
		}

		public MasterPlanQuestController(MasterPlanComponent component, IKampaiLogger logger, IPlayerService playerService, IDefinitionService definitionService, ICrossContextCapable gameContext, IMasterPlanQuestService masterPlanQuestService, bool isBuild)
		{
			quest = masterPlanQuestService.ConvertMasterPlanComponentToQuest(component, isBuild) as MasterPlanQuestType.Component;
			quest.isBuildQuest = isBuild;
			this.logger = logger;
			this.playerService = playerService;
			this.definitionService = definitionService;
			this.gameContext = gameContext;
			if (Definition.QuestSteps == null)
			{
				return;
			}
			for (int i = 0; i < Definition.QuestSteps.Count; i++)
			{
				object obj;
				if (Definition.QuestSteps[i].Type != QuestStepType.MasterPlanComponentBuild)
				{
					IQuestStepController questStepController = CreateController(quest.component.tasks[i].Definition.Type, quest, i);
					obj = questStepController;
				}
				else
				{
					obj = new BuildComponentQuestStepController(quest, i, playerService, gameContext, logger);
				}
				IQuestStepController questStepController2 = (IQuestStepController)obj;
				if (questStepController2 != null)
				{
					questStepControllers.Add(questStepController2);
				}
			}
		}

		public MasterPlanQuestController(MasterPlan plan, IKampaiLogger logger, IPlayerService playerService, IDefinitionService definitionService, ICrossContextCapable gameContext, IMasterPlanQuestService masterPlanQuestService)
		{
			quest = masterPlanQuestService.ConvertMasterPlanToQuest(plan) as MasterPlanQuestType.Component;
			this.logger = logger;
			this.playerService = playerService;
			this.definitionService = definitionService;
			this.gameContext = gameContext;
			if (Definition.QuestSteps != null)
			{
				for (int i = 0; i < Definition.QuestSteps.Count; i++)
				{
					IQuestStepController item = new BuildComponentQuestStepController(quest, i, playerService, gameContext, logger);
					questStepControllers.Add(item);
				}
			}
		}

		public void CheckAndUpdateQuestCompleteState()
		{
		}

		public void Debug_SetQuestToInProgressIfNotAlready()
		{
			if (quest.state == QuestState.Notstarted)
			{
				quest.state = QuestState.RunningTasks;
			}
			foreach (QuestStep step in quest.Steps)
			{
				step.state = ((step.state != 0) ? step.state : QuestStepState.Inprogress);
			}
		}

		public void DeleteQuest()
		{
		}

		public int GetIdleTime()
		{
			return 0;
		}

		public IList<QuantityItem> GetRequiredQuantityItems()
		{
			return new List<QuantityItem>();
		}

		public IQuestStepController GetStepController(int stepIndex)
		{
			if (stepIndex < questStepControllers.Count)
			{
				return questStepControllers[stepIndex];
			}
			logger.Error("Step Controller doesn't exist for step index {0}", stepIndex);
			return null;
		}

		public void GoToQuestState(QuestState targetState)
		{
		}

		public int IsTrackingOneOffCraftable(int itemDefinitionID)
		{
			return 0;
		}

		public bool IsTrackingThisBuilding(int buildingID, QuestStepType StepType)
		{
			return false;
		}

		public void OnQuestScriptComplete(QuestScriptInstance questScriptInstance)
		{
		}

		public void ProcessAutomaticQuest()
		{
		}

		public void RushQuestStep(int stepIndex)
		{
		}

		public void SetUpTracking()
		{
			AssignBuildingTrackIdToAllQuestStep();
			gameContext.injectionBinder.GetInstance<UpdateQuestWorldIconsSignal>().Dispatch(quest);
		}

		public void UpdateTask(QuestStepType stepType, QuestTaskTransition questTaskTransition = QuestTaskTransition.Start, Building building = null, int buildingDefId = 0, int itemDefId = 0)
		{
			foreach (IQuestStepController questStepController in questStepControllers)
			{
				questStepController.UpdateTask(questTaskTransition, building, buildingDefId, itemDefId);
			}
		}

		private IQuestStepController CreateController(MasterPlanComponentTaskType stepType, Quest quest, int questStepIndex)
		{
			IQuestStepController result = null;
			switch (stepType)
			{
			case MasterPlanComponentTaskType.Deliver:
				result = new DeliverTaskQuestStepController(quest, questStepIndex, definitionService, playerService, gameContext, logger);
				break;
			case MasterPlanComponentTaskType.Collect:
				result = new CollectTaskQuestStepController(quest, questStepIndex, definitionService, playerService, gameContext, logger);
				break;
			case MasterPlanComponentTaskType.CompleteOrders:
				result = new CompleteOrdersQuestStepController(quest, questStepIndex, definitionService, playerService, gameContext, logger);
				break;
			case MasterPlanComponentTaskType.PlayMiniGame:
				result = new PlayMiniGameQuestStepController(quest, questStepIndex, definitionService, playerService, gameContext, logger);
				break;
			case MasterPlanComponentTaskType.MiniGameScore:
				result = new MiniGameScoreQuestStepController(quest, questStepIndex, definitionService, playerService, gameContext, logger);
				break;
			case MasterPlanComponentTaskType.EarnPartyPoints:
				result = new EarnPartyPointsQuestStepController(quest, questStepIndex, definitionService, playerService, gameContext, logger);
				break;
			case MasterPlanComponentTaskType.EarnLeisurePartyPoints:
				result = new EarnLeisurePartyPointsQuestStepController(quest, questStepIndex, definitionService, playerService, gameContext, logger);
				break;
			case MasterPlanComponentTaskType.EarnMignettePartyPoints:
				result = new EarnMignettePartyPointsQuestStepController(quest, questStepIndex, definitionService, playerService, gameContext, logger);
				break;
			case MasterPlanComponentTaskType.EarnSandDollars:
				result = new EarnSandDollarsQuestStepController(quest, questStepIndex, definitionService, playerService, gameContext, logger);
				break;
			default:
				logger.Error("Using unused QuestStepType: {0}, your quest won't have this step", stepType);
				break;
			}
			return result;
		}

		private void AssignBuildingTrackIdToAllQuestStep()
		{
			foreach (IQuestStepController questStepController in questStepControllers)
			{
				questStepController.SetupTracking();
			}
		}
	}
}
