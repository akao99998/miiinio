using Kampai.Game.View;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class AddStuartToStageCommand : Command
	{
		[Inject]
		public StuartStageAnimationType animType { get; set; }

		[Inject]
		public StuartAddToStageSignal addToStageSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		public override void Execute()
		{
			StageBuilding firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<StageBuilding>(3054);
			if (animType == StuartStageAnimationType.CELEBRATE)
			{
				base.injectionBinder.GetInstance<BuildingZoomSignal>().Dispatch(new BuildingZoomSettings(ZoomType.IN, BuildingZoomType.STAGE, null, false));
			}
			BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
			BuildingObject buildingObject = component.GetBuildingObject(firstInstanceByDefinitionId.ID);
			StageBuildingObject stageBuildingObject = buildingObject as StageBuildingObject;
			if (stageBuildingObject != null)
			{
				Transform stageTransform = stageBuildingObject.GetStageTransform();
				addToStageSignal.Dispatch(stageTransform.position, stageTransform.localRotation, animType);
			}
		}
	}
}
