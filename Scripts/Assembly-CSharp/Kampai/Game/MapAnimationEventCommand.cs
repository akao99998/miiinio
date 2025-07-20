using Elevation.Logging;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class MapAnimationEventCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("MapAnimationEventCommand") as IKampaiLogger;

		[Inject]
		public AnimEventHandler animEventHandler { get; set; }

		[Inject]
		public int buildingDefinitionId { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		public override void Execute()
		{
			Building firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<Building>(buildingDefinitionId);
			VFXScript vFXScriptForBuilding = buildingManager.GetComponent<BuildingManagerView>().GetVFXScriptForBuilding(firstInstanceByDefinitionId.ID);
			if (vFXScriptForBuilding != null)
			{
				logger.Info("Binding AnimEventHandler to {0}", buildingDefinitionId);
				animEventHandler.SetSiblingVFXScript(vFXScriptForBuilding);
			}
		}
	}
}
