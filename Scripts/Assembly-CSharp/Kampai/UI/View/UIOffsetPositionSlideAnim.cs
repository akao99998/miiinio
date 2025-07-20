using System;
using UnityEngine;

namespace Kampai.UI.View
{
	public class UIOffsetPositionSlideAnim : UIAnim
	{
		private Vector2 offsetMinDestination;

		private Vector2 offsetMaxDestination;

		public UIOffsetPositionSlideAnim(Transform transform, float slideSpeed, Vector2 offsetMinDestination, Vector2 offsetMaxDestination, GoEaseType easeType, Action onAnimationComplete = null)
		{
			base.transform = transform;
			duration = slideSpeed;
			this.offsetMinDestination = offsetMinDestination;
			this.offsetMaxDestination = offsetMaxDestination;
			base.onAnimationComplete = onAnimationComplete;
			base.easeType = easeType;
		}

		protected override void ConfigAnimation(ref GoTween tween, GoTweenConfig tweenConfig)
		{
			tweenConfig.easeType = easeType;
			tweenConfig.vector2Prop("offsetMin", offsetMinDestination);
			tweenConfig.vector2Prop("offsetMax", offsetMaxDestination);
		}
	}
}
