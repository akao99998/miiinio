using Kampai.Main;
using Kampai.Util;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public abstract class MasterPlanQuestStepController : QuestStepController
	{
		protected readonly IDefinitionService definitionService;

		protected readonly MasterPlanComponentTask task;

		protected readonly MasterPlanComponentTaskDefinition taskDefinition;

		protected readonly MasterPlanQuestType.ComponentTaskDefinition taskQuestDef;

		protected int m_amountNeeded;

		protected bool m_needActiveDeliverButton = true;

		protected bool m_needActiveProgressBar = true;

		protected MasterPlanQuestType.Component questComponent;

		protected MasterPlanQuestType.ComponentTask taskQuest;

		public MasterPlanComponent Component
		{
			get
			{
				return (questComponent != null) ? questComponent.component : null;
			}
		}

		public MasterPlanQuestType.Component ComponentDef
		{
			get
			{
				return questComponent;
			}
		}

		public override string DeliverButtonLocKey
		{
			get
			{
				return "Complete";
			}
		}

		public override bool NeedActiveProgressBar
		{
			get
			{
				return m_needActiveProgressBar;
			}
		}

		public override QuestStepState StepState
		{
			get
			{
				return GetStepState();
			}
		}

		protected abstract string DescriptionLocKey { get; }

		protected MasterPlanQuestStepController(Quest quest, int stepIndex, IDefinitionService definitionService, IPlayerService playerService, ICrossContextCapable gameContext, IKampaiLogger logger)
			: base(quest, stepIndex, null, playerService, gameContext, logger)
		{
			questComponent = quest as MasterPlanQuestType.Component;
			this.definitionService = definitionService;
			taskQuestDef = base.questStepDefinition as MasterPlanQuestType.ComponentTaskDefinition;
			taskDefinition = ((taskQuestDef != null) ? taskQuestDef.taskDefinition : null);
			taskQuest = base.questStep as MasterPlanQuestType.ComponentTask;
			task = ((taskQuest != null) ? taskQuest.task : null);
		}

		public override string GetStepAction(ILocalizationService localService)
		{
			string key = string.Empty;
			switch (taskDefinition.Type)
			{
			case MasterPlanComponentTaskType.Deliver:
				key = "MasterPlanTaskDeliverQuestTitle";
				break;
			case MasterPlanComponentTaskType.Collect:
				key = "MasterPlanTaskCollectQuestTitle";
				break;
			case MasterPlanComponentTaskType.CompleteOrders:
				key = "MasterPlanTaskCompleteOrdersQuestTitle";
				break;
			case MasterPlanComponentTaskType.PlayMiniGame:
				key = "MasterPlanTaskPlayMiniGameQuestTitle";
				break;
			case MasterPlanComponentTaskType.MiniGameScore:
				key = "MasterPlanTaskMiniGameScoreQuestTitle";
				break;
			case MasterPlanComponentTaskType.EarnPartyPoints:
				key = "MasterPlanTaskEarnPartyPointsQuestTitle";
				break;
			case MasterPlanComponentTaskType.EarnLeisurePartyPoints:
				key = "MasterPlanTaskEarnLeisurePartyPointsQuestTitle";
				break;
			case MasterPlanComponentTaskType.EarnMignettePartyPoints:
				key = "MasterPlanTaskEarnMignettePartyPointsQuestTitle";
				break;
			case MasterPlanComponentTaskType.EarnSandDollars:
				key = "MasterPlanTaskEarnSandDollarsQuestTitle";
				break;
			}
			return localService.GetStringUpper(key);
		}

		public override string GetStepDescription(ILocalizationService localService, IDefinitionService defService)
		{
			return localService.GetString(DescriptionLocKey, DescriptionArgs(localService));
		}

		public QuestStepState GetStepState()
		{
			if (task.isComplete)
			{
				return QuestStepState.Complete;
			}
			return (!task.isHarvestable) ? QuestStepState.Inprogress : QuestStepState.Ready;
		}

		public override void SetupTracking()
		{
		}

		public override void UpdateTask(QuestTaskTransition questTaskTransition, Building building, int buildingDefId, int itemDefId)
		{
		}

		public IQuestStepController UpdateTaskInfo()
		{
			base.questStep.state = GetStepState();
			return this;
		}

		protected abstract object[] DescriptionArgs(ILocalizationService localizationService);
	}
}
