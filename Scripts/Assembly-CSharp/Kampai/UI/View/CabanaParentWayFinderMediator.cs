using Kampai.Game;
using Kampai.Util;

namespace Kampai.UI.View
{
	public class CabanaParentWayFinderMediator : AbstractParentWayFinderMediator
	{
		[Inject]
		public CabanaParentWayFinderView CabanaParentWayFinderView { get; set; }

		[Inject]
		public CameraAutoMoveToPositionSignal CameraAutoMoveToPositionSignal { get; set; }

		public override IWayFinderView View
		{
			get
			{
				return CabanaParentWayFinderView;
			}
		}

		protected override void PanToInstance()
		{
			CameraAutoMoveToPositionSignal.Dispatch(View.GetIndicatorPosition() + GameConstants.CAMERA_OFFSET_WAYFINDER_CABANA, 0.3f, false);
		}
	}
}
