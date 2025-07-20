using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class SendMinionToLeisureCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("SendMinionToLeisureCommand") as IKampaiLogger;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(GameElement.MINION_MANAGER)]
		public GameObject minionManager { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject]
		public PathFinder pathFinder { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public MinionStateChangeSignal minionStateChangeSignal { get; set; }

		[Inject]
		public BuildingChangeStateSignal buildingStateChangeSignal { get; set; }

		[Inject]
		public RouteMinionToLeisureSignal routeMinionToLeisureSignal { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playMinionNoAnimAudioSignal { get; set; }

		[Inject]
		public int buildingId { get; set; }

		public override void Execute()
		{
			MinionManagerView component = minionManager.GetComponent<MinionManagerView>();
			BuildingManagerView component2 = buildingManager.GetComponent<BuildingManagerView>();
			LeisureBuildingObjectView leisureBuildingObjectView = component2.GetBuildingObject(buildingId) as LeisureBuildingObjectView;
			LeisureBuilding leisureBuilding = leisureBuildingObjectView.leisureBuilding;
			if (leisureBuilding == null || leisureBuildingObjectView == null)
			{
				return;
			}
			Queue<int> minionListSortedByDistanceAndState = component.GetMinionListSortedByDistanceAndState(leisureBuildingObjectView.transform.position);
			if (minionListSortedByDistanceAndState.Count >= leisureBuilding.Definition.WorkStations)
			{
				playMinionNoAnimAudioSignal.Dispatch("Play_minion_confirm_pathToBldg_01");
				for (int i = 0; i < leisureBuilding.Definition.WorkStations; i++)
				{
					int objectId = minionListSortedByDistanceAndState.Dequeue();
					MinionObject minionObject = component.Get(objectId);
					HandlePathing(minionObject, leisureBuildingObjectView, leisureBuilding);
				}
				IQuestService obj = questService;
				int iD = leisureBuilding.Definition.ID;
				obj.UpdateAllQuestsWithQuestStepType(QuestStepType.Leisure, QuestTaskTransition.Complete, null, iD);
				questService.UpdateAllQuestsWithQuestStepType(QuestStepType.PlayAnyLeisure);
				buildingStateChangeSignal.Dispatch(leisureBuilding.ID, BuildingState.Working);
			}
		}

		private void HandlePathing(MinionObject minionObject, LeisureBuildingObjectView leisureBuildingObject, LeisureBuilding building)
		{
			Minion byInstanceId = playerService.GetByInstanceId<Minion>(minionObject.ID);
			if (byInstanceId == null)
			{
				logger.Fatal(FatalCode.CMD_NO_SUCH_MINION, "{0}", minionObject.ID);
			}
			byInstanceId.BuildingID = buildingId;
			minionStateChangeSignal.Dispatch(byInstanceId.ID, MinionState.Leisure);
			int minionsInBuilding = building.GetMinionsInBuilding();
			Vector3 position = minionObject.transform.position;
			Vector3 routePosition = leisureBuildingObject.GetRoutePosition(minionsInBuilding, building, position);
			Vector3 routeRotation = leisureBuildingObject.GetRouteRotation(minionsInBuilding);
			IList<Vector3> list = pathFinder.FindPath(position, routePosition, 4, true);
			if (list == null)
			{
				List<Vector3> list2 = new List<Vector3>();
				list2.Add(routePosition);
				list = list2;
			}
			RouteInstructions type = default(RouteInstructions);
			type.minion = minionObject;
			type.Path = list;
			type.Rotation = routeRotation.y;
			type.TargetBuilding = building;
			type.StartTime = timeService.CurrentTime();
			routeMinionToLeisureSignal.Dispatch(minionObject, type, minionsInBuilding);
			float num = minionObject.GetAction<ConstantSpeedPathAction>().Duration();
			building.AddMinion(minionObject.ID, type.StartTime + (int)num + 1);
		}
	}
}
