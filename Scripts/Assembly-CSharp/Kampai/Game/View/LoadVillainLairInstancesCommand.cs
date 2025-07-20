using System;
using Elevation.Logging;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game.View
{
	public class LoadVillainLairInstancesCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("LoadVillainLairInstancesCommand") as IKampaiLogger;

		private int lairDefinitionID;

		private VillainLair currentLair;

		[Inject]
		public int villainLairInstanceID { get; set; }

		[Inject]
		public Action callback { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IMasterPlanService masterPlanService { get; set; }

		[Inject]
		public VillainLairModel villainLairModel { get; set; }

		[Inject]
		public ParentLairResourcePlotSignal parentLairResourcePlotSignal { get; set; }

		[Inject(GameElement.VILLAIN_LAIR_PARENT)]
		public GameObject villainLairParent { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		public override void Execute()
		{
			currentLair = playerService.GetByInstanceId<VillainLair>(villainLairInstanceID);
			if (currentLair == null)
			{
				logger.Error("Trying to load instanves for a  villain lair that doesn't exist: {0}", villainLairInstanceID);
				return;
			}
			lairDefinitionID = currentLair.Definition.ID;
			if (!villainLairModel.areLairAssetsLoaded)
			{
				logger.Fatal(FatalCode.CMD_INCOMPLETE_VILLAIN_LAIR_ASSETS_RESOURCES_UI, "Assets for Villain Lair {0} are not loaded", currentLair.ID);
			}
			LoadInstances();
		}

		private void LoadInstances()
		{
			if (villainLairModel.villainLairInstances.Keys.Contains(villainLairInstanceID))
			{
				return;
			}
			foreach (int value in Enum.GetValues(typeof(VillainLairModel.LairPrefabType)))
			{
				LoadInstances((VillainLairModel.LairPrefabType)value);
			}
			InstanceLoadingComplete();
		}

		private void InstanceLoadingComplete()
		{
			callback();
		}

		private void LoadInstances(VillainLairModel.LairPrefabType prefabType)
		{
			switch (prefabType)
			{
			case VillainLairModel.LairPrefabType.LAIR:
				LoadLairInstance(prefabType);
				break;
			case VillainLairModel.LairPrefabType.LOCKED_PLOT:
			case VillainLairModel.LairPrefabType.UNLOCKED_PLOT:
				LoadPlotInstances(prefabType);
				break;
			default:
				logger.Error("Trying to load an unaccounted instance {0}: need a method to load this for the villain lair.");
				break;
			}
		}

		private void LoadPlotInstances(VillainLairModel.LairPrefabType prefabType)
		{
			bool flag = prefabType == VillainLairModel.LairPrefabType.LOCKED_PLOT;
			BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
			for (int i = 0; i < currentLair.resourcePlotInstanceIDs.Count; i++)
			{
				VillainLairResourcePlot byInstanceId = playerService.GetByInstanceId<VillainLairResourcePlot>(currentLair.resourcePlotInstanceIDs[i]);
				if (flag != (byInstanceId.State == BuildingState.Inaccessible))
				{
					continue;
				}
				GameObject gameObject = UnityEngine.Object.Instantiate(villainLairModel.asyncLoadedPrefabs[(int)prefabType], (Vector3)byInstanceId.Location, Quaternion.Euler(0f, byInstanceId.rotation, 0f)) as GameObject;
				if (gameObject == null)
				{
					logger.Fatal(FatalCode.CMD_NULL_PREFAB, "Villain Lair Island Resources prefab is null: {0}", currentLair.Definition.ResourceBuildingDefID);
					break;
				}
				VillainLairResourcePlotObjectView villainLairResourcePlotObjectView = component.GetBuildingObject(byInstanceId.ID) as VillainLairResourcePlotObjectView;
				if (villainLairResourcePlotObjectView != null)
				{
					parentLairResourcePlotSignal.Dispatch(byInstanceId, gameObject);
				}
				else
				{
					component.CreateBuilding(byInstanceId);
					villainLairResourcePlotObjectView = component.GetBuildingObject(byInstanceId.ID) as VillainLairResourcePlotObjectView;
					if (villainLairResourcePlotObjectView != null)
					{
						parentLairResourcePlotSignal.Dispatch(byInstanceId, gameObject);
					}
				}
				villainLairResourcePlotObjectView.InitializeAnimators();
			}
		}

		private void LoadLairInstance(VillainLairModel.LairPrefabType prefabType)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(villainLairModel.asyncLoadedPrefabs[(int)prefabType]);
			if (gameObject == null)
			{
				logger.Fatal(FatalCode.CMD_NULL_PREFAB, "Villain Lair Island prefab is null: {0}", lairDefinitionID);
				return;
			}
			villainLairModel.villainLairInstances[currentLair.ID] = gameObject;
			gameObject.transform.parent = villainLairParent.transform;
			VillainLairLocationView componentInChildren = gameObject.GetComponentInChildren<VillainLairLocationView>();
			MasterPlan currentMasterPlan = masterPlanService.CurrentMasterPlan;
			if (currentMasterPlan == null)
			{
				logger.Error("Master Plan Instance is null on creation of the Villain Lair!");
				return;
			}
			MasterPlanDefinition definition = currentMasterPlan.Definition;
			componentInChildren.SetUpInstanceIDs(definition, playerService, logger);
			gameObject.transform.position = (Vector3)currentLair.Definition.Location;
			MasterPlanObject masterPlanObject = gameObject.AddComponent<MasterPlanObject>();
			masterPlanObject.Init(currentMasterPlan.ID);
			masterPlanService.AddMasterPlanObject(masterPlanObject);
		}
	}
}
