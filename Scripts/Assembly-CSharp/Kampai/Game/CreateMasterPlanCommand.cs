using System.Collections.Generic;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.UI.View;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	internal sealed class CreateMasterPlanCommand : Command
	{
		[Inject]
		public MasterPlanDefinition masterPlanDefinition { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public PurchaseNewBuildingSignal purchaseNewBuildingSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject]
		public EnableAllVillainLairCollidersSignal enableAllVillainLairCollidersSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal globalSFXSignal { get; set; }

		[Inject]
		public MoveAudioListenerSignal moveAudioListenerSignal { get; set; }

		[Inject]
		public RemoveWayFinderSignal removeWayfinderSignal { get; set; }

		public override void Execute()
		{
			moveAudioListenerSignal.Dispatch(true, null);
			MasterPlanComponentBuildingDefinition masterPlanComponentBuildingDefinition = definitionService.Get<MasterPlanComponentBuildingDefinition>(masterPlanDefinition.BuildingDefID);
			VillainLairDefinition villainLairDefinition = definitionService.Get<VillainLairDefinition>(3137);
			Building building = masterPlanComponentBuildingDefinition.BuildBuilding();
			building.Location = villainLairDefinition.Platforms[villainLairDefinition.Platforms.Count - 1].placementLocation;
			playerService.Add(building);
			purchaseNewBuildingSignal.Dispatch(building);
			BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
			List<int> compBuildingDefinitionIDs = masterPlanDefinition.CompBuildingDefinitionIDs;
			for (int i = 0; i < compBuildingDefinitionIDs.Count; i++)
			{
				int definitionId = compBuildingDefinitionIDs[i];
				MasterPlanComponentBuilding firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<MasterPlanComponentBuilding>(definitionId);
				if (firstInstanceByDefinitionId != null)
				{
					string buildingRemovalAnimController = villainLairDefinition.Platforms[i].buildingRemovalAnimController;
					MasterPlanComponentBuildingObject masterPlanComponentBuildingObject = component.GetBuildingObject(firstInstanceByDefinitionId.ID) as MasterPlanComponentBuildingObject;
					masterPlanComponentBuildingObject.TriggerMasterPlanCompleteAnimation(buildingRemovalAnimController);
				}
			}
			HandleCleanup();
		}

		private void HandleCleanup()
		{
			Villain firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<Villain>(masterPlanDefinition.VillainCharacterDefID);
			removeWayfinderSignal.Dispatch(firstInstanceByDefinitionId.ID);
			enableAllVillainLairCollidersSignal.Dispatch(true);
			globalSFXSignal.Dispatch("Play_componentsFlyIntoMP_01");
		}
	}
}
