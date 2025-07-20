using Elevation.Logging;
using Kampai.Game;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Common
{
	public class StuartSpinMicCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("StuartSpinMicCommand") as IKampaiLogger;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		public override void Execute()
		{
			logger.Info("Stuart Spin Mic");
			StageBuilding firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<StageBuilding>(3054);
			if (firstInstanceByDefinitionId != null)
			{
				BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
				BuildingObject buildingObject = component.GetBuildingObject(firstInstanceByDefinitionId.ID);
				StageBuildingObject stageBuildingObject = buildingObject as StageBuildingObject;
				stageBuildingObject.SetSpinMic();
			}
		}
	}
}
