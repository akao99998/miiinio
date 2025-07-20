namespace Kampai.UI.View
{
	public class OrderBoardWayFinderView : AbstractWayFinderView
	{
		protected override string UIName
		{
			get
			{
				return "OrderBoardWayFinder";
			}
		}

		protected override string WayFinderDefaultIcon
		{
			get
			{
				return wayFinderDefinition.OrderBoardDefaultIcon;
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
