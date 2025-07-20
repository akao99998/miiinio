using Kampai.Game;
using Kampai.Game.View;
using strange.extensions.command.impl;

namespace Kampai.Tools.AnimationToolKit
{
	public class RemoveCharacterCommand : Command
	{
		[Inject]
		public IPlayerService PlayerService { get; set; }

		[Inject]
		public BuildingObject BuildingObject { get; set; }

		[Inject]
		public CharacterObject CharacterObject { get; set; }

		public override void Execute()
		{
			Building byInstanceId = PlayerService.GetByInstanceId<Building>(BuildingObject.ID);
			TaskableBuilding taskableBuilding = byInstanceId as TaskableBuilding;
			if (taskableBuilding != null)
			{
				taskableBuilding.RemoveMinion(CharacterObject.ID, 0);
			}
			TikiBarBuildingObjectView tikiBarBuildingObjectView = BuildingObject as TikiBarBuildingObjectView;
			if (tikiBarBuildingObjectView != null)
			{
				tikiBarBuildingObjectView.UnlinkChild(CharacterObject.ID);
			}
		}
	}
}
