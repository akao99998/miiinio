namespace Kampai.UI.View
{
	public class WayFinderView : AbstractWayFinderView
	{
		protected override string UIName
		{
			get
			{
				return "WayFinder";
			}
		}

		protected override string WayFinderDefaultIcon
		{
			get
			{
				return wayFinderDefinition.DefaultIcon;
			}
		}

		protected override void OnLoadWayFinderModal(WayFinderModal wayFinderModal)
		{
			wayFinderModal.SpecificModel.gameObject.SetActive(false);
			wayFinderModal.GenericModel.gameObject.SetActive(true);
			m_CenterImage = wayFinderModal.GenericModel.CenterImage;
			m_NoRotationTransform = wayFinderModal.GenericModel.NoRotationTransform;
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
