using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game.View
{
	public class RepairLairResourcePlotCommand : Command
	{
		[Inject]
		public VillainLairResourcePlot resourcePlot { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject]
		public ParentLairResourcePlotSignal parentLairResourcePlotSignal { get; set; }

		[Inject]
		public BuildingChangeStateSignal buildingChangeStateSignal { get; set; }

		public override void Execute()
		{
			BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
			VillainLairResourcePlotObjectView villainLairResourcePlotObjectView = component.GetBuildingObject(resourcePlot.ID) as VillainLairResourcePlotObjectView;
			GameObject gameObject = villainLairResourcePlotObjectView.gameObject;
			GameObject gameObject2 = gameObject.FindChild(string.Format("{0}(Clone)", resourcePlot.Definition.brokenPrefab_loaded));
			if (gameObject2 != null)
			{
				Object.DestroyImmediate(gameObject2);
			}
			GameObject original = KampaiResources.Load(resourcePlot.Definition.prefab_loaded) as GameObject;
			GameObject type = Object.Instantiate(original, (Vector3)resourcePlot.Location, Quaternion.Euler(0f, resourcePlot.rotation, 0f)) as GameObject;
			buildingChangeStateSignal.Dispatch(resourcePlot.ID, BuildingState.Idle);
			parentLairResourcePlotSignal.Dispatch(resourcePlot, type);
			villainLairResourcePlotObjectView.InitializeAnimators();
		}
	}
}
