using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class StorageBuildingObject : AnimatingBuildingObject
	{
		public StorageBuilding storageBuilding { get; set; }

		internal override void Init(Building building, IKampaiLogger logger, IDictionary<string, RuntimeAnimatorController> controllers, IDefinitionService definitionService)
		{
			base.Init(building, logger, controllers, definitionService);
			storageBuilding = (StorageBuilding)building;
			base.gameObject.AddComponent<StorageBuildingView>();
			SetupWaterEmitter();
		}

		private void SetupWaterEmitter()
		{
			CustomFMOD_StudioEventEmitter customFMOD_StudioEventEmitter = base.gameObject.AddComponent<CustomFMOD_StudioEventEmitter>();
			customFMOD_StudioEventEmitter.id = "StorageWaterEmitter";
			customFMOD_StudioEventEmitter.shiftPosition = false;
			customFMOD_StudioEventEmitter.staticSound = false;
			customFMOD_StudioEventEmitter.path = base.fmodService.GetGuid("Play_water_stream_light_01");
			customFMOD_StudioEventEmitter.Play();
		}

		internal void SetOpen()
		{
			if (!IsInAnimatorState(GetHashAnimationState("Base Layer.Open")) || !IsInAnimatorState(GetHashAnimationState("Base Layer.Opening")))
			{
				EnqueueAction(new TriggerBuildingAnimationAction(this, "OnOpen", logger));
			}
		}

		internal void SetClose()
		{
			if (!IsInAnimatorState(GetHashAnimationState("Base Layer.Closing")) || !IsInAnimatorState(GetHashAnimationState("Base Layer.Idle")))
			{
				EnqueueAction(new TriggerBuildingAnimationAction(this, "OnClose", logger));
			}
		}

		public override void SetState(BuildingState newState)
		{
			base.SetState(newState);
			if (buildingState != newState)
			{
				buildingState = newState;
				switch (newState)
				{
				case BuildingState.Inactive:
				case BuildingState.Idle:
					SetClose();
					break;
				case BuildingState.Working:
					SetOpen();
					break;
				case BuildingState.Construction:
				case BuildingState.Complete:
					break;
				}
			}
		}
	}
}
