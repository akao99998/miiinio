using System.Collections;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game.View
{
	public class RepairBridgeCommand : Command
	{
		[Inject]
		public BridgeBuilding building { get; set; }

		[Inject]
		public CreateInventoryBuildingSignal createInventroyBuildingSignal { get; set; }

		[Inject]
		public BuildingChangeStateSignal changeState { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public PurchaseLandExpansionSignal landExpansionSignal { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public RemoveBuildingSignal removeBuildingSignal { get; set; }

		[Inject]
		public CameraAutoMoveSignal autoMoveSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal globalAudioSignal { get; set; }

		[Inject]
		public DebugUpdateGridSignal gridSignal { get; set; }

		[Inject]
		public AddFootprintSignal addFootprintSignal { get; set; }

		[Inject]
		public VillainLairModel villainLairModel { get; set; }

		public override void Execute()
		{
			routineRunner.StartCoroutine(WaitAFrame());
		}

		private IEnumerator WaitAFrame()
		{
			yield return new WaitForEndOfFrame();
			int buildingId = building.ID;
			BridgeDefinition bridgeDef = definitionService.GetBridgeDefinition(building.BridgeId);
			BuildingManagerView buildingManagerView = buildingManager.GetComponent<BuildingManagerView>();
			BuildingObject buildObj2 = buildingManagerView.GetBuildingObject(buildingId);
			buildingManagerView.RemoveBuilding(buildingId);
			Location location = building.Location;
			removeBuildingSignal.Dispatch(location, definitionService.GetBuildingFootprint(building.Definition.FootprintID));
			playerService.Remove(building);
			BridgeBuildingDefinition bridgeBuildingDef = definitionService.Get(bridgeDef.RepairedBuildingID) as BridgeBuildingDefinition;
			BridgeBuilding bridge = bridgeBuildingDef.Build() as BridgeBuilding;
			bridge.Location = location;
			bridge.BridgeId = 0;
			playerService.Add(bridge);
			createInventroyBuildingSignal.Dispatch(bridge, location);
			changeState.Dispatch(bridge.ID, BuildingState.Idle);
			gridSignal.Dispatch();
			buildObj2 = buildingManagerView.GetBuildingObject(bridge.ID);
			buildObj2.SetVFXState("RepairBuilding");
			globalAudioSignal.Dispatch("Play_building_repair_01");
			yield return new WaitForSeconds(2f);
			Building buildingBridge = playerService.GetByInstanceId<Building>(bridge.ID);
			if (buildingBridge.IsFootprintable)
			{
				addFootprintSignal.Dispatch(buildingBridge, location);
			}
			if (villainLairModel.currentActiveLair == null && !villainLairModel.goingToLair)
			{
				Vector3 position = new Vector3(bridgeDef.cameraPan.x, bridgeDef.cameraPan.y, bridgeDef.cameraPan.z);
				ScreenPosition sp = new ScreenPosition();
				autoMoveSignal.Dispatch(position, new Boxed<ScreenPosition>(sp), new CameraMovementSettings(CameraMovementSettings.Settings.None, null, null), true);
			}
			landExpansionSignal.Dispatch(bridgeDef.LandExpansionID, true);
			buildObj2.SetVFXState("None");
		}
	}
}
