namespace Kampai.UI.View
{
	public class StageBuildingWayFinderView : AbstractWayFinderView
	{
		protected override string UIName
		{
			get
			{
				return "StageBuildingWayFinder";
			}
		}

		protected override string WayFinderDefaultIcon
		{
			get
			{
				return wayFinderDefinition.StageBuildingIcon;
			}
		}

		protected override bool OnCanUpdate()
		{
			if (zoomCameraModel.ZoomedIn)
			{
				return false;
			}
			return true;
		}
	}
}
