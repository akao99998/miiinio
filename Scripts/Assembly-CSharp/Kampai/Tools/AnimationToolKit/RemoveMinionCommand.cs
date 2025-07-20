using Kampai.Game;
using Kampai.Game.View;
using strange.extensions.command.impl;

namespace Kampai.Tools.AnimationToolKit
{
	public class RemoveMinionCommand : Command
	{
		[Inject]
		public BuildingObject BuildingObject { get; set; }

		[Inject]
		public MinionObject MinionObject { get; set; }

		[Inject]
		public IPlayerService PlayerService { get; set; }

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
			if (!(taskableBuildingObject == null) || !(leisureBuildingObjectView == null))
			{
				if (taskableBuilding != null)
				{
					taskableBuilding.AddToCompletedMinions(MinionObject.ID, 0);
					taskableBuilding.HarvestFromCompleteMinions();
					taskableBuilding.RemoveMinion(MinionObject.ID, 0);
					taskableBuildingObject.UntrackChild(MinionObject.ID, taskableBuilding);
				}
				else if (leisureBuildingObjectView != null)
				{
					leisureBuilding.CleanMinionQueue();
					leisureBuildingObjectView.FreeAllMinions(MinionObject.ID);
				}
			}
		}
	}
}
