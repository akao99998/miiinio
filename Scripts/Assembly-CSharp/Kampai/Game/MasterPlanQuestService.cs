using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Util;

namespace Kampai.Game
{
	public class MasterPlanQuestService : IMasterPlanQuestService
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("MasterPlanQuestService") as IKampaiLogger;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public IMasterPlanService masterPlanService { get; set; }

		public QuestLine ConvertComponentToQuestLine(MasterPlanComponent component)
		{
			if (component == null)
			{
				return null;
			}
			MasterPlan byInstanceId = playerService.GetByInstanceId<MasterPlan>(component.planTrackingInstance);
			List<QuestDefinition> list = new List<QuestDefinition>();
			list.Add(GetBuildComponentQuest(component, component.ID));
			list.Add(ConvertMasterPlanComponentDefToQuestDef(component, component.ID));
			IList<QuestDefinition> quests = list;
			MasterPlanQuestType.PlanDefinition planDefinition = new MasterPlanQuestType.PlanDefinition();
			planDefinition.GivenByCharacterID = byInstanceId.Definition.VillainCharacterDefID;
			planDefinition.Quests = quests;
			planDefinition.component = component;
			planDefinition.plan = byInstanceId;
			return planDefinition;
		}

		public QuestLine ConvertMasterPlanToQuestLine(MasterPlan masterPlan)
		{
			if (masterPlan == null)
			{
				return null;
			}
			List<MasterPlanComponent> instancesByType = playerService.GetInstancesByType<MasterPlanComponent>();
			MasterPlanQuestType.PlanDefinition planDefinition = new MasterPlanQuestType.PlanDefinition();
			planDefinition.GivenByCharacterID = masterPlan.Definition.VillainCharacterDefID;
			planDefinition.components = new List<MasterPlanComponent>();
			planDefinition.plan = masterPlan;
			planDefinition.Quests = new List<QuestDefinition> { CreateMasterPlanQuestDefinition(masterPlan) };
			MasterPlanQuestType.PlanDefinition planDefinition2 = planDefinition;
			for (int i = 0; i < instancesByType.Count; i++)
			{
				MasterPlanComponent masterPlanComponent = instancesByType[i];
				if (masterPlanComponent.planTrackingInstance == masterPlan.ID)
				{
					planDefinition2.components.Add(masterPlanComponent);
				}
			}
			return planDefinition2;
		}

		public Quest GetQuestByInstanceId(int id)
		{
			return questService.GetQuestByInstanceId(id);
		}

		private MasterPlanComponent GetActiveComponent()
		{
			IList<MasterPlanComponent> instancesByType = playerService.GetInstancesByType<MasterPlanComponent>();
			for (int i = 0; i < instancesByType.Count; i++)
			{
				MasterPlanComponent masterPlanComponent = instancesByType[i];
				if (masterPlanComponent.State > MasterPlanComponentState.NotStarted && masterPlanComponent.State < MasterPlanComponentState.Complete)
				{
					return masterPlanComponent;
				}
			}
			return null;
		}

		public List<Quest> GetQuests()
		{
			List<Quest> instancesByType = playerService.GetInstancesByType<Quest>();
			MasterPlan currentMasterPlan = masterPlanService.CurrentMasterPlan;
			if (currentMasterPlan != null)
			{
				bool flag = masterPlanService.AllComponentsAreComplete(currentMasterPlan.Definition.ID);
				MasterPlanComponentBuilding firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<MasterPlanComponentBuilding>(currentMasterPlan.Definition.BuildingDefID);
				if (flag && (firstInstanceByDefinitionId == null || firstInstanceByDefinitionId.State == BuildingState.Complete))
				{
					Quest questByInstanceId = GetQuestByInstanceId(currentMasterPlan.ID);
					instancesByType.Add(questByInstanceId);
					return instancesByType;
				}
			}
			MasterPlanComponent activeComponent = GetActiveComponent();
			if (activeComponent == null)
			{
				return instancesByType;
			}
			int id = ((activeComponent.State < MasterPlanComponentState.TasksCollected) ? activeComponent.ID : 711);
			Quest questByInstanceId2 = questService.GetQuestByInstanceId(id);
			instancesByType.Add(questByInstanceId2);
			return instancesByType;
		}

		public Quest ConvertMasterPlanToQuest(MasterPlan masterPlan)
		{
			if (masterPlan == null)
			{
				return null;
			}
			Building firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<Building>(masterPlan.Definition.BuildingDefID);
			MasterPlanQuestType.Component component = new MasterPlanQuestType.Component(CreateMasterPlanQuestDefinition(masterPlan));
			component.ID = masterPlan.ID;
			component.Steps = new List<QuestStep> { GernerateBuildMasterPlanQuestStep(masterPlan) };
			component.state = QuestState.RunningTasks;
			component.masterPlan = masterPlan;
			component.buildDefId = masterPlan.Definition.BuildingDefID;
			component.component = null;
			component.components = new List<MasterPlanComponent>();
			component.isBuildingCompete = firstInstanceByDefinitionId != null && firstInstanceByDefinitionId.State == BuildingState.Idle;
			MasterPlanQuestType.Component component2 = component;
			List<MasterPlanComponent> instancesByType = playerService.GetInstancesByType<MasterPlanComponent>();
			for (int i = 0; i < instancesByType.Count; i++)
			{
				MasterPlanComponent masterPlanComponent = instancesByType[i];
				if (masterPlanComponent.planTrackingInstance == masterPlan.ID)
				{
					component2.components.Add(masterPlanComponent);
				}
			}
			return component2;
		}

		private MasterPlanQuestType.ComponentDefinition CreateMasterPlanQuestDefinition(MasterPlan masterPlan)
		{
			MasterPlanQuestType.ComponentDefinition componentDefinition = new MasterPlanQuestType.ComponentDefinition();
			componentDefinition.ID = masterPlan.Definition.ID;
			componentDefinition.LocalizedKey = masterPlan.Definition.LocalizedKey;
			componentDefinition.QuestSteps = new List<QuestStepDefinition> { GernerateBuildMasterPlanQuestStepDef(masterPlan.Definition) };
			componentDefinition.type = QuestType.MasterPlan;
			componentDefinition.SurfaceID = 40001;
			componentDefinition.QuestLineID = masterPlan.ID;
			componentDefinition.NarrativeOrder = 0;
			componentDefinition.SurfaceType = QuestSurfaceType.Character;
			componentDefinition.RewardTransaction = masterPlan.Definition.RewardTransactionID;
			return componentDefinition;
		}

		public Quest ConvertMasterPlanComponentToQuest(MasterPlanComponent component, bool buildTask)
		{
			if (component == null)
			{
				return null;
			}
			QuestDefinition questDefinition = ConvertMasterPlanComponentDefToQuestDef(component, component.ID);
			MasterPlan currentMasterPlan = masterPlanService.CurrentMasterPlan;
			QuestState state = QuestState.Notstarted;
			MasterPlanQuestType.Component component2;
			if (buildTask)
			{
				component2 = new MasterPlanQuestType.Component(GetBuildComponentQuest(component, component.ID));
				component2.ID = 711;
				component2.Steps = new List<QuestStep> { GernerateBuildQuestStep(component) };
				component2.component = component;
				component2.state = state;
				component2.masterPlan = currentMasterPlan;
				component2.index = currentMasterPlan.Definition.ComponentDefinitionIDs.IndexOf(component.Definition.ID);
				component2.buildDefId = component.buildingDefID;
				return component2;
			}
			IList<QuestStep> list = new List<QuestStep>(component.tasks.Count);
			for (int i = 0; i < component.tasks.Count; i++)
			{
				list.Add(ConvertMasterPlanComponentTaskToQuestStep(component.tasks[i]));
			}
			component2 = new MasterPlanQuestType.Component(questDefinition as MasterPlanQuestType.ComponentDefinition);
			component2.ID = component.ID;
			component2.Steps = list;
			component2.state = state;
			component2.component = component;
			component2.masterPlan = currentMasterPlan;
			component2.index = currentMasterPlan.Definition.ComponentDefinitionIDs.IndexOf(component.Definition.ID);
			component2.buildDefId = component.buildingDefID;
			return component2;
		}

		public Quest ConvertMasterPlanComponentToQuest(MasterPlanComponent component)
		{
			return ConvertMasterPlanComponentToQuest(component, false);
		}

		public QuestDefinition ConvertMasterPlanComponentDefToQuestDef(MasterPlanComponent component, int questLineId)
		{
			if (component == null)
			{
				return null;
			}
			int iD = component.ID;
			IList<QuestStepDefinition> list = new List<QuestStepDefinition>();
			for (int i = 0; i < component.tasks.Count; i++)
			{
				list.Add(ConvertMasterPlanComponentTaskDefToQuestStepDef(component.tasks[i].Definition));
			}
			MasterPlanComponentBuildingDefinition masterPlanComponentBuildingDefinition = definitionService.Get<MasterPlanComponentBuildingDefinition>(component.buildingDefID);
			MasterPlanQuestType.ComponentDefinition componentDefinition = new MasterPlanQuestType.ComponentDefinition();
			componentDefinition.ID = iD;
			componentDefinition.LocalizedKey = masterPlanComponentBuildingDefinition.LocalizedKey;
			componentDefinition.type = QuestType.MasterPlan;
			componentDefinition.QuestLineID = questLineId;
			componentDefinition.isBuildQuest = false;
			componentDefinition.reward = component.reward.Definition;
			componentDefinition.QuestSteps = list;
			componentDefinition.SurfaceType = QuestSurfaceType.Character;
			componentDefinition.NarrativeOrder = 0;
			componentDefinition.SurfaceID = 40001;
			return componentDefinition;
		}

		private MasterPlanQuestType.ComponentDefinition GetBuildComponentQuest(MasterPlanComponent component, int questLineId)
		{
			MasterPlanComponentBuildingDefinition masterPlanComponentBuildingDefinition = definitionService.Get<MasterPlanComponentBuildingDefinition>(component.buildingDefID);
			MasterPlanQuestType.ComponentDefinition componentDefinition = new MasterPlanQuestType.ComponentDefinition();
			componentDefinition.ID = 711;
			componentDefinition.LocalizedKey = masterPlanComponentBuildingDefinition.LocalizedKey;
			componentDefinition.type = QuestType.MasterPlan;
			componentDefinition.QuestLineID = questLineId;
			componentDefinition.NarrativeOrder = 1;
			componentDefinition.isBuildQuest = true;
			componentDefinition.reward = component.reward.Definition;
			componentDefinition.SurfaceType = QuestSurfaceType.Character;
			componentDefinition.QuestSteps = new List<QuestStepDefinition> { GernerateBuildQuestStepDef(component.Definition) };
			componentDefinition.SurfaceID = 40001;
			return componentDefinition;
		}

		public QuestStep GernerateBuildMasterPlanQuestStep(MasterPlan masterPlan)
		{
			if (masterPlan == null)
			{
				return null;
			}
			MasterPlanQuestType.ComponentTask componentTask = new MasterPlanQuestType.ComponentTask();
			componentTask.state = ((!masterPlan.displayCooldownAlert) ? QuestStepState.Inprogress : QuestStepState.Complete);
			componentTask.TrackedID = masterPlan.Definition.BuildingDefID;
			componentTask.AmountCompleted = 0;
			componentTask.AmountReady = 0;
			return componentTask;
		}

		public QuestStepDefinition GernerateBuildMasterPlanQuestStepDef(MasterPlanDefinition task)
		{
			if (task == null)
			{
				return null;
			}
			MasterPlanQuestType.ComponentTaskDefinition componentTaskDefinition = new MasterPlanQuestType.ComponentTaskDefinition();
			componentTaskDefinition.Type = QuestStepType.MasterPlanBuild;
			componentTaskDefinition.ItemDefinitionID = task.BuildingDefID;
			componentTaskDefinition.ItemAmount = 1;
			return componentTaskDefinition;
		}

		public QuestStep GernerateBuildQuestStep(MasterPlanComponent component)
		{
			if (component == null)
			{
				return null;
			}
			QuestStepState state = QuestStepState.Notstarted;
			switch (component.State)
			{
			case MasterPlanComponentState.InProgress:
				state = QuestStepState.Notstarted;
				break;
			case MasterPlanComponentState.TasksComplete:
				state = QuestStepState.Notstarted;
				break;
			case MasterPlanComponentState.TasksCollected:
				state = QuestStepState.Inprogress;
				break;
			case MasterPlanComponentState.Scaffolding:
				state = QuestStepState.Inprogress;
				break;
			case MasterPlanComponentState.Built:
				state = QuestStepState.Ready;
				break;
			case MasterPlanComponentState.Complete:
				state = QuestStepState.Complete;
				break;
			default:
				logger.Warning("No master plan component state defined for: {0}", component.State);
				break;
			}
			MasterPlanQuestType.ComponentTask componentTask = new MasterPlanQuestType.ComponentTask();
			componentTask.state = state;
			componentTask.TrackedID = component.buildingDefID;
			componentTask.AmountCompleted = 0;
			componentTask.AmountReady = 0;
			return componentTask;
		}

		public QuestStepDefinition GernerateBuildQuestStepDef(MasterPlanComponentDefinition task)
		{
			if (task == null)
			{
				return null;
			}
			MasterPlanComponent firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<MasterPlanComponent>(task.ID);
			MasterPlanQuestType.ComponentTaskDefinition componentTaskDefinition = new MasterPlanQuestType.ComponentTaskDefinition();
			componentTaskDefinition.Type = QuestStepType.MasterPlanComponentBuild;
			componentTaskDefinition.ItemDefinitionID = firstInstanceByDefinitionId.buildingDefID;
			componentTaskDefinition.ItemAmount = 1;
			return componentTaskDefinition;
		}

		public QuestStep ConvertMasterPlanComponentTaskToQuestStep(MasterPlanComponentTask task)
		{
			if (task == null)
			{
				return null;
			}
			MasterPlanQuestType.ComponentTask componentTask = new MasterPlanQuestType.ComponentTask();
			componentTask.AmountCompleted = (int)task.earnedQuantity;
			componentTask.state = ((!task.isComplete) ? QuestStepState.Inprogress : QuestStepState.Complete);
			componentTask.TrackedID = 0;
			componentTask.AmountReady = (int)task.earnedQuantity;
			componentTask.task = task;
			return componentTask;
		}

		public QuestStepDefinition ConvertMasterPlanComponentTaskDefToQuestStepDef(MasterPlanComponentTaskDefinition task)
		{
			if (task == null)
			{
				return null;
			}
			MasterPlanQuestType.ComponentTaskDefinition componentTaskDefinition = new MasterPlanQuestType.ComponentTaskDefinition();
			componentTaskDefinition.Type = QuestStepType.MasterPlanTask;
			componentTaskDefinition.ItemDefinitionID = task.requiredItemId;
			componentTaskDefinition.ItemAmount = (int)task.requiredQuantity;
			componentTaskDefinition.ShowWayfinder = task.ShowWayfinder;
			componentTaskDefinition.taskDefinition = task;
			return componentTaskDefinition;
		}
	}
}
