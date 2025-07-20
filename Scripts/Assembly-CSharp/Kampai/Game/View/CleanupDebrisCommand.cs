using System.Collections;
using Elevation.Logging;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game.View
{
	public class CleanupDebrisCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("CleanupDebrisCommand") as IKampaiLogger;

		[Inject]
		public int buildingID { get; set; }

		[Inject]
		public bool showVFX { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public RemoveBuildingSignal removeBuildingSignal { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		public override void Execute()
		{
			routineRunner.StartCoroutine(WaitAFrame());
		}

		private IEnumerator WaitAFrame()
		{
			BuildingManagerView buildingManagerView = buildingManager.GetComponent<BuildingManagerView>();
			BuildingObject buildObj = buildingManagerView.GetBuildingObject(buildingID);
			DebrisBuilding building = playerService.GetByInstanceId<DebrisBuilding>(buildingID);
			logger.Info("Cleaning up debris: {0}", buildingID);
			if (showVFX)
			{
				yield return new WaitForEndOfFrame();
				if (!(buildObj == null) && building != null)
				{
					DebrisBuildingDefinition def = building.Definition;
					for (int i = 0; i < def.VFXPrefabs.Count; i++)
					{
						GameObject clearingGO = Object.Instantiate(KampaiResources.Load<GameObject>(def.VFXPrefabs[i]));
						clearingGO.transform.parent = buildObj.gameObject.transform;
						clearingGO.transform.localPosition = Vector3.zero;
					}
					DebrisBuildingObject debrisObject = buildObj as DebrisBuildingObject;
					if (debrisObject != null)
					{
						debrisObject.FadeDebris();
					}
				}
			}
			else
			{
				buildingManagerView.RemoveBuilding(buildingID);
				removeBuildingSignal.Dispatch(building.Location, definitionService.GetBuildingFootprint(building.Definition.FootprintID));
				playerService.Remove(building);
			}
		}
	}
}
