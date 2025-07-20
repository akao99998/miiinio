using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game.View
{
	public class CameraAutoMoveToMignetteCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("CameraAutoMoveToMignetteCommand") as IKampaiLogger;

		[Inject]
		public int definitionId { get; set; }

		[Inject]
		public bool bypassModal { get; set; }

		[Inject]
		public PanInstructions panInstructions { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public CameraAutoMoveToBuildingDefSignal moveToBuildingDefSignal { get; set; }

		[Inject]
		public PanAndOpenModalSignal panAndOpenModalSignal { get; set; }

		[Inject]
		public PopupMessageSignal popupMessageSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal globalSFXSignal { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public ShowNeedXMinionsSignal showNeedXMinionsSignal { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		public override void Execute()
		{
			Building firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<Building>(definitionId);
			MignetteBuilding mignetteBuilding = firstInstanceByDefinitionId as MignetteBuilding;
			int num = 0;
			if (firstInstanceByDefinitionId != null && mignetteBuilding != null)
			{
				if (!OpenBuildingMenuCommand.HasEnoughFreeMinionsToAssignToBuilding(playerService, mignetteBuilding))
				{
					showNeedXMinionsSignal.Dispatch(mignetteBuilding.GetMinionSlotsOwned());
					return;
				}
				num = mignetteBuilding.Definition.LevelUnlocked;
				if (playerService.GetQuantity(StaticItem.LEVEL_ID) >= num)
				{
					panAndOpenModalSignal.Dispatch(firstInstanceByDefinitionId.ID, bypassModal);
					return;
				}
			}
			IList<AspirationalBuildingDefinition> all = definitionService.GetAll<AspirationalBuildingDefinition>();
			int i = 0;
			for (int count = all.Count; i < count; i++)
			{
				AspirationalBuildingDefinition aspirationalBuildingDefinition = all[i];
				if (aspirationalBuildingDefinition.BuildingDefinitionID != definitionId)
				{
					continue;
				}
				BuildingDefinition buildingDefinition = definitionService.Get<BuildingDefinition>(aspirationalBuildingDefinition.BuildingDefinitionID);
				if (buildingDefinition != null)
				{
					MignetteBuildingDefinition mignetteBuildingDefinition = definitionService.Get<MignetteBuildingDefinition>(aspirationalBuildingDefinition.BuildingDefinitionID);
					if (mignetteBuildingDefinition != null)
					{
						string aspirationalMessage = mignetteBuildingDefinition.AspirationalMessage;
						BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
						BuildingObject buildingObject = component.GetBuildingObject(firstInstanceByDefinitionId.ID);
						Vector3 zoomCenter = buildingObject.ZoomCenter;
						moveToBuildingDefSignal.Dispatch(buildingDefinition, zoomCenter, panInstructions);
						globalSFXSignal.Dispatch("Play_action_locked_01");
						popupMessageSignal.Dispatch(localizationService.GetString(aspirationalMessage, num), PopupMessageType.NORMAL);
						return;
					}
				}
			}
			logger.Error("CameraAutoMoveToMignetteCommand: Failed to find mignette with definition ID {0}", definitionId);
		}
	}
}
