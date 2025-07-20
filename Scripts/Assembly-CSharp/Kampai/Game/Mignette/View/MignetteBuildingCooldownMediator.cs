using Kampai.Game.View;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.Game.Mignette.View
{
	public class MignetteBuildingCooldownMediator : Mediator
	{
		private MignetteBuildingViewObject buildingViewObject;

		private MignetteBuildingObject mignetteBuildingObject;

		private MignetteBuilding mignetteBuilding;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public BuildingCooldownUpdateViewSignal cooldownUpdateViewSignal { get; set; }

		[Inject]
		public BuildingCooldownCompleteSignal cooldownCompleteSignal { get; set; }

		[Inject]
		public PlayLocalAudioSignal localAudioSignal { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			buildingViewObject = GetComponent<MignetteBuildingViewObject>();
			mignetteBuildingObject = GetComponent<MignetteBuildingObject>();
			cooldownUpdateViewSignal.AddListener(OnUpdateCooldownView);
			cooldownCompleteSignal.AddListener(OnCooldownComplete);
			buildingViewObject.ResetCooldownView(localAudioSignal);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			cooldownUpdateViewSignal.RemoveListener(OnUpdateCooldownView);
			cooldownCompleteSignal.RemoveListener(OnCooldownComplete);
		}

		public void Start()
		{
			mignetteBuilding = playerService.GetByInstanceId<MignetteBuilding>(mignetteBuildingObject.ID);
		}

		public void OnCooldownComplete(int buildingID)
		{
			if (mignetteBuildingObject.ID == buildingID)
			{
				buildingViewObject.ResetCooldownView(localAudioSignal);
				buildingViewObject.DestroyDynamicCoolDownObjects();
			}
		}

		public void OnUpdateCooldownView(int buildingID)
		{
			if (mignetteBuildingObject.ID == buildingID)
			{
				if (!buildingViewObject.IsDynamicCooldownObjectsLoaded() && mignetteBuilding.State == BuildingState.Cooldown)
				{
					LoadDynamicCooldownObjects();
				}
				int num = timeService.CurrentTime();
				int stateStartTime = mignetteBuilding.StateStartTime;
				int num2 = num - stateStartTime;
				float pctDone = (float)num2 / (float)mignetteBuilding.GetCooldown();
				if (mignetteBuilding.State == BuildingState.Idle)
				{
					pctDone = 1f;
				}
				buildingViewObject.UpdateCooldownView(localAudioSignal, mignetteBuilding.MignetteData, pctDone);
			}
		}

		private void LoadDynamicCooldownObjects()
		{
			MignetteBuilding byInstanceId = playerService.GetByInstanceId<MignetteBuilding>(mignetteBuildingObject.ID);
			MignetteBuildingDefinition mignetteBuildingDefinition = byInstanceId.MignetteBuildingDefinition;
			Transform parent = base.gameObject.transform;
			if (mignetteBuildingDefinition.CooldownObjects == null)
			{
				return;
			}
			foreach (MignetteChildObjectDefinition cooldownObject in mignetteBuildingDefinition.CooldownObjects)
			{
				GameObject gameObject = Object.Instantiate(KampaiResources.Load<GameObject>(cooldownObject.Prefab));
				buildingViewObject.AddDynamicCoolDownObject(gameObject);
				Transform transform = gameObject.transform;
				transform.parent = parent;
				if (cooldownObject.IsLocal)
				{
					transform.localPosition = cooldownObject.Position;
					transform.localRotation = Quaternion.Euler(0f, cooldownObject.Rotation, 0f);
				}
				else
				{
					transform.position = cooldownObject.Position;
					transform.rotation = Quaternion.Euler(0f, cooldownObject.Rotation, 0f);
				}
			}
		}
	}
}
