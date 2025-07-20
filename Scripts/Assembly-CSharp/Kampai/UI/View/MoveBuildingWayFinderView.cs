using Kampai.Game;
using Kampai.Game.View;
using UnityEngine;

namespace Kampai.UI.View
{
	public class MoveBuildingWayFinderView : AbstractWayFinderView
	{
		protected override string UIName
		{
			get
			{
				return "MoveBuildingWayFinder";
			}
		}

		protected override string WayFinderDefaultIcon
		{
			get
			{
				return wayFinderDefinition.ConnectableIcon;
			}
		}

		protected override bool OnCanUpdate()
		{
			return true;
		}

		protected override void OnInvisible(Vector3 direction)
		{
			Snappable = true;
			AvoidsHUD = false;
			isForceHideEnabled = false;
			base.OnInvisible(direction);
		}

		protected override void OnVisible()
		{
			base.OnVisible();
			isForceHideEnabled = true;
		}

		protected override void LoadModalData(WorldToGlassUIModal modal)
		{
			base.LoadModalData(modal);
			if (targetObject == null)
			{
				BuildingManagerMediator component = gameContext.injectionBinder.GetInstance<GameObject>(GameElement.BUILDING_MANAGER).GetComponent<BuildingManagerMediator>();
				targetObject = component.GetCurrentDummyBuilding();
			}
		}

		public override void SetForceHide(bool forceHide)
		{
		}

		public override Vector3 GetIndicatorPosition()
		{
			BuildingDefinitionObject buildingDefinitionObject = targetObject as BuildingDefinitionObject;
			if (buildingDefinitionObject != null)
			{
				return buildingDefinitionObject.ResourceIconPosition;
			}
			return Vector3.zero;
		}

		public BuildingDefinitionObject GetTargetObject()
		{
			return targetObject as BuildingDefinitionObject;
		}
	}
}
