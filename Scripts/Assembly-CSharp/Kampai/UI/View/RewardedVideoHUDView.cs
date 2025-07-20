using Kampai.Game;
using Kampai.Util;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class RewardedVideoHUDView : KampaiView
	{
		private const string showAnimatorState = "anim_RewardedVideo_slideIn";

		private const string hideAnimatorState = "anim_RewardedVideo_slideOut";

		public ButtonView ShowAdButton;

		public Animator PanelAnimator;

		public Signal SlideOutAnimationCompleteSignal = new Signal();

		public AdPlacementInstance AdPlacementInstance { get; private set; }

		public void Init(IPositionService positionService)
		{
			positionService.AddHUDElementToAvoid(base.gameObject);
		}

		public void InitPlacement(AdPlacementInstance instance)
		{
			AdPlacementInstance = instance;
		}

		public void PlayPanelAnimation(bool show)
		{
			PanelAnimator.Play((!show) ? "anim_RewardedVideo_slideOut" : "anim_RewardedVideo_slideIn");
		}

		public void OnSlideOutAnimationComplete()
		{
			SlideOutAnimationCompleteSignal.Dispatch();
		}
	}
}
