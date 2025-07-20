using Kampai.Game;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Tools.AnimationToolKit
{
	public class AddMinionCommand : Command
	{
		[Inject]
		public IPlayerService PlayerService { get; set; }

		[Inject]
		public BuildingObject BuildingObject { get; set; }

		[Inject]
		public MinionObject MinionObject { get; set; }

		[Inject]
		public int RouteIndex { get; set; }

		public override void Execute()
		{
			Building byInstanceId = PlayerService.GetByInstanceId<Building>(BuildingObject.ID);
			TaskableBuilding taskableBuilding = byInstanceId as TaskableBuilding;
			LeisureBuilding leisureBuilding = byInstanceId as LeisureBuilding;
			if (taskableBuilding == null && leisureBuilding == null)
			{
				return;
			}
			TaskableBuildingObject taskableBuildingObject = BuildingObject as TaskableBuildingObject;
			LeisureBuildingObjectView leisureBuildingObjectView = BuildingObject as LeisureBuildingObjectView;
			if (taskableBuildingObject == null && leisureBuildingObjectView == null)
			{
				return;
			}
			TaskingMinionObject taskingMinionObject = new TaskingMinionObject(MinionObject, RouteIndex);
			if (taskingMinionObject != null)
			{
				taskingMinionObject.RoutingIndex = RouteIndex;
				AnimatingBuildingDefinition animatingBuildingDefinition = null;
				if (taskableBuilding != null)
				{
					taskableBuilding.AddMinion(MinionObject.ID, 0);
					animatingBuildingDefinition = taskableBuilding.Definition;
				}
				else if (leisureBuilding != null)
				{
					leisureBuilding.AddMinion(MinionObject.ID, 0);
					animatingBuildingDefinition = leisureBuilding.Definition;
				}
				RuntimeAnimatorController runtimeAnimatorController = KampaiResources.Load<RuntimeAnimatorController>(animatingBuildingDefinition.AnimationDefinitions[0].MinionController);
				MinionObject.SetAnimController(runtimeAnimatorController);
				if (taskableBuildingObject != null)
				{
					taskableBuildingObject.MoveToRoutingPosition(MinionObject, RouteIndex);
					taskableBuildingObject.TrackChild(MinionObject, runtimeAnimatorController, false);
				}
				else if (leisureBuildingObjectView != null)
				{
					leisureBuildingObjectView.MoveToRoutingPosition(MinionObject, RouteIndex);
					leisureBuildingObjectView.TrackChild(MinionObject, runtimeAnimatorController, RouteIndex);
				}
			}
		}
	}
}
