using System.Collections;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class StartMinionTaskCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("StartMinionTaskCommand") as IKampaiLogger;

		[Inject]
		public Tuple<int, MinionObject, int> parameters { get; set; }

		public int buildingID { get; set; }

		public MinionObject minion { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		public int startTime { get; set; }

		[Inject]
		public StartMinionRouteSignal startMinionRouteSignal { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject]
		public PathFinder pathFinder { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public MinionStateChangeSignal minionStateChangeSignal { get; set; }

		[Inject]
		public BuildingChangeStateSignal changeState { get; set; }

		[Inject]
		public TeleportMinionToBuildingSignal teleportMinionToBuildingSignal { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public MinionTaskCompleteSignal taskCompleteSignal { get; set; }

		public override void Execute()
		{
			buildingID = parameters.Item1;
			minion = parameters.Item2;
			startTime = parameters.Item3;
			BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
			if (buildingManager == null || component == null)
			{
				logger.Fatal(FatalCode.CMD_NULL_REF, 0);
			}
			TaskableBuildingObject taskableBuildingObject = component.GetBuildingObject(buildingID) as TaskableBuildingObject;
			object obj;
			if (taskableBuildingObject != null)
			{
				TaskableBuilding byInstanceId = playerService.GetByInstanceId<TaskableBuilding>(taskableBuildingObject.ID);
				obj = byInstanceId;
			}
			else
			{
				obj = null;
			}
			TaskableBuilding taskableBuilding = (TaskableBuilding)obj;
			if (taskableBuilding != null)
			{
				HandleAddingMinion(taskableBuilding, taskableBuildingObject);
			}
		}

		private void HandleAddingMinion(TaskableBuilding building, TaskableBuildingObject taskableBuildingObject)
		{
			BuildingState state = building.State;
			if (state != BuildingState.Idle && state != BuildingState.Working && state != BuildingState.Harvestable)
			{
				return;
			}
			int minionsInBuilding = building.GetMinionsInBuilding();
			int minionSlotsOwned = building.GetMinionSlotsOwned();
			if (minionsInBuilding < minionSlotsOwned)
			{
				int num = HandlePathing(minion.gameObject, taskableBuildingObject, building);
				changeState.Dispatch(building.ID, BuildingState.Working);
				building.StateStartTime = num;
				building.AddMinion(minion.ID, num);
				if (questService == null)
				{
					logger.Fatal(FatalCode.CMD_NULL_REF, 2);
				}
				IQuestService obj = questService;
				int iD = building.Definition.ID;
				obj.UpdateAllQuestsWithQuestStepType(QuestStepType.MinionTask, QuestTaskTransition.Complete, null, iD, minion.Level);
			}
		}

		private int HandlePathing(GameObject minionGo, TaskableBuildingObject tbo, TaskableBuilding building)
		{
			Vector3 position = minionGo.transform.position;
			Vector3 routePosition = tbo.GetRoutePosition(building.GetMinionsInBuilding(), building, position);
			Vector3 routeRotation = tbo.GetRouteRotation(building.GetMinionsInBuilding());
			IList<Vector3> list = pathFinder.FindPath(position, routePosition, 4, true);
			if (list == null)
			{
				List<Vector3> list2 = new List<Vector3>();
				list2.Add(routePosition);
				list = list2;
			}
			RouteInstructions type = default(RouteInstructions);
			type.minion = minion;
			type.Path = list;
			type.Rotation = routeRotation.y;
			type.TargetBuilding = building;
			type.StartTime = startTime;
			startMinionRouteSignal.Dispatch(type);
			return SetupMinionState(minion.ID, minionGo, building);
		}

		private int SetupMinionState(int minionID, GameObject minionGo, TaskableBuilding building)
		{
			MinionObject component = minionGo.GetComponent<MinionObject>();
			float num = component.GetAction<ConstantSpeedPathAction>().Duration();
			int num2 = timeService.CurrentTime();
			Minion minion = component.GetMinion();
			if (minion == null)
			{
				logger.Fatal(FatalCode.CMD_NO_SUCH_MINION, "{0}", minionID);
			}
			minion.BuildingID = buildingID;
			if (building is MignetteBuilding)
			{
				minionStateChangeSignal.Dispatch(minion.ID, MinionState.PlayingMignette);
				routineRunner.StartCoroutine(WaitThenTeleportMinion(minion.ID));
			}
			else
			{
				minion.TaskDuration = BuildingUtil.GetHarvestTimeForTaskableBuilding(building, definitionService);
				minion.UTCTaskStartTime = num2 + (int)num + 1;
				if (!(building is DebrisBuilding))
				{
					timeEventService.AddEvent(minion.ID, minion.UTCTaskStartTime, minion.TaskDuration, taskCompleteSignal, (building is ResourceBuilding) ? TimeEventType.ProductionBuff : TimeEventType.Default);
				}
				minionStateChangeSignal.Dispatch(minion.ID, MinionState.Tasking);
			}
			return num2;
		}

		private IEnumerator WaitThenTeleportMinion(int minionID)
		{
			yield return new WaitForSeconds(0.2f);
			teleportMinionToBuildingSignal.Dispatch(minionID);
		}
	}
}
