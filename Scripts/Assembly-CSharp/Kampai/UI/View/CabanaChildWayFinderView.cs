namespace Kampai.UI.View
{
	public class CabanaChildWayFinderView : AbstractChildWayFinderView
	{
		protected override string UIName
		{
			get
			{
				return "CabanaChildWayFinder";
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
