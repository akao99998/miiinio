using System.Collections.Generic;
using Kampai.Util;
using strange.extensions.context.api;
using strange.extensions.injector.api;

namespace Kampai.Game
{
	public class QuestController : IQuestController
	{
		private Quest quest;

		private List<IQuestStepController> questStepControllers = new List<IQuestStepController>();

		private IKampaiLogger logger;

		private IPlayerService playerService;

		private IDefinitionService definitionService;

		private IPrestigeService prestigeService;

		private IQuestScriptService questScriptService;

		private ICrossContextCapable gameContext;

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

		public int StepCount
		{
			get
			{
				return quest.Steps.Count;
			}
		}

		public int QuestIconTrackedInstanceID
		{
			get
			{
				return quest.QuestIconTrackedInstanceId;
			}
		}

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

		public QuestDefinition Definition
		{
			get
			{
				return quest.GetActiveDefinition();
			}
		}

		public QuestState State
		{
			get
			{
				return quest.state;
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

		protected QuestSurfaceType surfaceType
		{
			get
			{
				return Definition.SurfaceType;
			}
		}

		public QuestController(Quest quest, IKampaiLogger logger, IPlayerService playerService, IPrestigeService prestigeService, IDefinitionService definitionService, IQuestScriptService questScriptService, ICrossContextCapable gameContext, Environment environment)
		{
			this.quest = quest;
			this.logger = logger;
			this.playerService = playerService;
			this.prestigeService = prestigeService;
			this.definitionService = definitionService;
			this.questScriptService = questScriptService;
			this.gameContext = gameContext;
			if (Definition.QuestSteps == null)
			{
				return;
			}
			for (int i = 0; i < Definition.QuestSteps.Count; i++)
			{
				IQuestStepController questStepController = CreateController(Definition.QuestSteps[i].Type, quest, i, environment);
				if (questStepController != null)
				{
					questStepControllers.Add(questStepController);
				}
			}
		}

		private IQuestStepController CreateController(QuestStepType stepType, Quest quest, int questStepIndex, Environment environment)
		{
			IQuestStepController result = null;
			switch (stepType)
			{
			case QuestStepType.StageRepair:
			case QuestStepType.CabanaRepair:
			case QuestStepType.WelcomeHutRepair:
			case QuestStepType.FountainRepair:
			case QuestStepType.StorageRepair:
			case QuestStepType.LairPortalRepair:
			case QuestStepType.MinionUpgradeBuildingRepair:
				result = new BuildingRepairQuestStepController(quest, questStepIndex, questScriptService, playerService, gameContext, logger);
				break;
			case QuestStepType.Construction:
				result = new ConstuctionQuestStepController(quest, questStepIndex, questScriptService, playerService, gameContext, logger);
				break;
			case QuestStepType.Delivery:
				result = new DeliveryQuestStepController(quest, questStepIndex, questScriptService, playerService, definitionService, gameContext, logger);
				break;
			case QuestStepType.Harvest:
				result = new HarvestQuestStepController(quest, questStepIndex, questScriptService, playerService, definitionService, gameContext, logger);
				break;
			case QuestStepType.Leisure:
				result = new LeisureQuestStepController(quest, questStepIndex, questScriptService, playerService, gameContext, logger);
				break;
			case QuestStepType.Mignette:
				result = new MignetteQuestStepController(quest, questStepIndex, questScriptService, playerService, gameContext, logger);
				break;
			case QuestStepType.OrderBoard:
				result = new OrderBoardQuestStepController(quest, questStepIndex, questScriptService, playerService, gameContext, logger);
				break;
			case QuestStepType.ThrowParty:
				result = new ThrowPartyQuestStepController(quest, questStepIndex, questScriptService, playerService, gameContext, logger);
				break;
			case QuestStepType.BridgeRepair:
				result = new BridgeRepairQuestStepController(quest, questStepIndex, questScriptService, playerService, definitionService, gameContext, logger, environment);
				break;
			case QuestStepType.MinionTask:
				result = new MinionTaskQuestStepController(quest, questStepIndex, questScriptService, playerService, gameContext, logger);
				break;
			case QuestStepType.MinionUpgrade:
				result = new UpgradeMinionQuestStepController(quest, questStepIndex, questScriptService, playerService, gameContext, logger);
				break;
			case QuestStepType.HaveUpgradedMinions:
				result = new MinionUpgradeToLevelQuestStepController(quest, questStepIndex, questScriptService, playerService, gameContext, logger);
				break;
			case QuestStepType.PlayAnyLeisure:
			case QuestStepType.HarvestAnyLeisure:
				result = new AnyLeisureQuestStepController(quest, questStepIndex, questScriptService, playerService, gameContext, logger);
				break;
			case QuestStepType.MysteryBoxOnboarding:
				result = new MysteryBoxOnboardingQuestStepController(quest, questStepIndex, questScriptService, playerService, gameContext, logger);
				break;
			default:
				logger.Error("Using unused QuestStepType: {0}, your quest won't have this step", stepType);
				break;
			}
			return result;
		}

		public int GetIdleTime()
		{
			return gameContext.injectionBinder.GetInstance<IPlayerDurationService>().GetGameTimeDuration(quest);
		}

		public IList<QuantityItem> GetRequiredQuantityItems()
		{
			IList<QuantityItem> list = new List<QuantityItem>();
			for (int i = 0; i < StepCount; i++)
			{
				IQuestStepController stepController = GetStepController(i);
				uint amountNeeded = (uint)stepController.AmountNeeded;
				if (amountNeeded != 0)
				{
					QuantityItem item = new QuantityItem(stepController.ItemDefinitionID, amountNeeded);
					list.Add(item);
				}
			}
			return list;
		}

		public void SetUpTracking()
		{
			if (!NeedQuestTracking())
			{
				return;
			}
			AssignBuildingTrackIdToAllQuestStep();
			switch (surfaceType)
			{
			case QuestSurfaceType.Building:
				if (!AssignQuestIconTrackedBuildingInstanceID(quest))
				{
					logger.Log(KampaiLogLevel.Error, "Quest tracking instance is not set! This quest: {0} won't have any icons in the game world", quest.GetActiveDefinition().ID.ToString());
				}
				break;
			case QuestSurfaceType.Character:
				if (!AssignQuestIconTrackedCharacterInstanceID(quest))
				{
					logger.Log(KampaiLogLevel.Error, "Quest tracking instance is not set! This quest: {0} won't have any icons in the game world", quest.GetActiveDefinition().ID.ToString());
				}
				break;
			case QuestSurfaceType.Automatic:
			case QuestSurfaceType.LimitedEvent:
			case QuestSurfaceType.TimedEvent:
			case QuestSurfaceType.Bridge:
				AssignQuestIconTrackedInstanceID(quest);
				break;
			}
			gameContext.injectionBinder.GetInstance<UpdateQuestWorldIconsSignal>().Dispatch(quest);
		}

		private bool NeedQuestTracking()
		{
			if (quest.Steps == null || quest.Steps.Count == 0)
			{
				if (Definition.SurfaceID > 0 && Definition.SurfaceType == QuestSurfaceType.Automatic)
				{
					return true;
				}
				return false;
			}
			return true;
		}

		private void AssignBuildingTrackIdToAllQuestStep()
		{
			foreach (IQuestStepController questStepController in questStepControllers)
			{
				questStepController.SetupTracking();
			}
		}

		private void AssignQuestIconTrackedInstanceID(Quest quest)
		{
			if (!AssignQuestIconTrackedBuildingInstanceID(quest) && !AssignQuestIconTrackedCharacterInstanceID(quest))
			{
				logger.Log(KampaiLogLevel.Error, "Quest tracking instance is not set! This quest: {0} won't have any icons in the game world", quest.GetActiveDefinition().ID.ToString());
			}
		}

		private bool AssignQuestIconTrackedCharacterInstanceID(Quest quest)
		{
			Prestige prestige = prestigeService.GetPrestige(quest.GetActiveDefinition().SurfaceID);
			if (prestige != null)
			{
				quest.QuestIconTrackedInstanceId = prestige.trackedInstanceId;
				return true;
			}
			logger.Log(KampaiLogLevel.Error, "Character doesn't exist for the quest surface id: {0}. This quest: {1} won't have any icons in the game world", quest.GetActiveDefinition().SurfaceID, quest.GetActiveDefinition().ID.ToString());
			return false;
		}

		private bool AssignQuestIconTrackedBuildingInstanceID(Quest quest)
		{
			ICollection<Building> byDefinitionId = playerService.GetByDefinitionId<Building>(quest.GetActiveDefinition().SurfaceID);
			foreach (Building item in byDefinitionId)
			{
				BuildingState state = item.State;
				if (state != BuildingState.Complete && state != BuildingState.Construction && state != BuildingState.Inventory)
				{
					quest.QuestIconTrackedInstanceId = item.ID;
					return true;
				}
			}
			return false;
		}

		public int IsTrackingOneOffCraftable(int itemDefinitionID)
		{
			int num = 0;
			foreach (IQuestStepController questStepController in questStepControllers)
			{
				num += questStepController.IsTrackingOneOffCraftable(itemDefinitionID);
			}
			return num;
		}

		public bool IsTrackingThisBuilding(int buildingID, QuestStepType StepType)
		{
			if (State == QuestState.RunningTasks)
			{
				foreach (IQuestStepController questStepController in questStepControllers)
				{
					if (questStepController.StepState == QuestStepState.Notstarted && questStepController.StepInstanceTrackedID == buildingID && questStepController.StepType == StepType)
					{
						return true;
					}
				}
			}
			return false;
		}

		public IQuestStepController GetStepController(int stepIndex)
		{
			if (stepIndex >= questStepControllers.Count)
			{
				logger.Error("Step Controller doesn't exist for step index {0}", stepIndex);
				return null;
			}
			return questStepControllers[stepIndex];
		}

		public void DeleteQuest()
		{
			if (quest != null)
			{
				playerService.Remove(quest);
			}
		}

		public void OnQuestScriptComplete(QuestScriptInstance questScriptInstance)
		{
			if (quest.state == QuestState.RunningTasks)
			{
				int questStepID = questScriptInstance.QuestStepID;
				if (questStepID < 0 || questStepID > quest.Steps.Count)
				{
					logger.Error("QuestService:OnQuestScriptComplete: QuestStepId {0} is out of range! Can't mark as complete!", questStepID);
				}
				else if (quest.Steps[questScriptInstance.QuestStepID].state == QuestStepState.RunningStartScript)
				{
					quest.Steps[questScriptInstance.QuestStepID].state = QuestStepState.Inprogress;
				}
				else
				{
					quest.Steps[questScriptInstance.QuestStepID].state = QuestStepState.Complete;
					CheckAndUpdateQuestCompleteState();
				}
			}
			else if (quest.state != QuestState.Harvestable)
			{
				gameContext.injectionBinder.GetInstance<GoToNextQuestStateSignal>().Dispatch(Definition.ID);
			}
		}

		public void RushQuestStep(int stepIndex)
		{
			IQuestStepController questStepController = questStepControllers[stepIndex];
			questStepController.GoToNextState(true);
		}

		public void UpdateTask(QuestStepType stepType, QuestTaskTransition questTaskTransition = QuestTaskTransition.Start, Building building = null, int buildingDefId = 0, int itemDefId = 0)
		{
			foreach (IQuestStepController questStepController in questStepControllers)
			{
				if (questStepController.StepType == stepType && questStepController.StepState != QuestStepState.Complete)
				{
					questStepController.UpdateTask(questTaskTransition, building, buildingDefId, itemDefId);
				}
			}
		}

		public void CheckAndUpdateQuestCompleteState()
		{
			foreach (QuestStep step in quest.Steps)
			{
				if (step.state != QuestStepState.Complete)
				{
					quest.AutoGrantReward = false;
					gameContext.injectionBinder.GetInstance<UpdateQuestWorldIconsSignal>().Dispatch(quest);
					return;
				}
			}
			gameContext.injectionBinder.GetInstance<GoToNextQuestStateSignal>().Dispatch(Definition.ID);
		}

		public void GoToQuestState(QuestState targetState)
		{
			ICrossContextInjectionBinder injectionBinder = gameContext.injectionBinder;
			quest.state = targetState;
			switch (targetState)
			{
			case QuestState.Notstarted:
				break;
			case QuestState.RunningStartScript:
				questScriptService.StartQuestScript(quest, true);
				injectionBinder.GetInstance<UpdateQuestWorldIconsSignal>().Dispatch(quest);
				break;
			case QuestState.RunningTasks:
				UpdateTask(QuestStepType.Construction);
				UpdateTask(QuestStepType.Harvest);
				UpdateTask(QuestStepType.MinionUpgrade);
				UpdateTask(QuestStepType.HaveUpgradedMinions);
				UpdateTask(QuestStepType.MysteryBoxOnboarding);
				injectionBinder.GetInstance<UpdateQuestWorldIconsSignal>().Dispatch(quest);
				gameContext.injectionBinder.GetInstance<StartGameTimeTrackingSignal>().Dispatch(quest);
				break;
			case QuestState.RunningCompleteScript:
				questScriptService.StartQuestScript(quest, false);
				break;
			case QuestState.Harvestable:
				injectionBinder.GetInstance<QuestHarvestableSignal>().Dispatch(quest);
				break;
			case QuestState.Complete:
				if (quest.GetActiveDefinition().GetReward(definitionService) != null)
				{
					injectionBinder.GetInstance<RemoveQuestWorldIconSignal>().Dispatch(quest);
				}
				questScriptService.StartQuestScript(quest, false, true);
				injectionBinder.GetInstance<QuestCompleteSignal>().Dispatch(quest);
				break;
			}
		}

		public void ProcessAutomaticQuest()
		{
			if (!questScriptService.HasScript(quest, true))
			{
				if (Definition.QuestSteps == null || Definition.QuestSteps.Count == 0)
				{
					if (!questScriptService.HasScript(quest, false))
					{
						GoToQuestState(QuestState.Complete);
					}
					else
					{
						GoToQuestState(QuestState.RunningCompleteScript);
					}
				}
				else
				{
					GoToQuestState(QuestState.RunningTasks);
				}
			}
			else
			{
				GoToQuestState(QuestState.RunningStartScript);
			}
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
	}
}
