using Kampai.Common;
using Kampai.Game;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class RewardedVideoHUDMediator : EventMediator
	{
		[Inject]
		public RewardedVideoHUDView view { get; set; }

		[Inject]
		public OpenRewardedAdWatchModalSignal openRewadedAdModalSignal { get; set; }

		[Inject]
		public PickControllerModel pickModel { get; set; }

		[Inject]
		public IZoomCameraModel zoomCameraModel { get; set; }

		[Inject]
		public IPositionService positionService { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			view.Init(positionService);
			view.ShowAdButton.ClickedSignal.AddListener(OnShowAdButtonClicked);
		}

		public override void OnRemove()
		{
			view.ShowAdButton.ClickedSignal.RemoveListener(OnShowAdButtonClicked);
		}

		internal void OnShowAdButtonClicked()
		{
			if (!pickModel.PanningCameraBlocked && !pickModel.ZoomingCameraBlocked && !zoomCameraModel.ZoomInProgress)
			{
				openRewadedAdModalSignal.Dispatch(view.AdPlacementInstance);
			}
		}
	}
}
