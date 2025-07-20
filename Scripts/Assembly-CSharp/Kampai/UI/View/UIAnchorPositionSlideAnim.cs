using System;
using UnityEngine;

namespace Kampai.UI.View
{
	public class UIAnchorPositionSlideAnim : UIAnim
	{
		private Vector2 destination;

		public UIAnchorPositionSlideAnim(Transform transform, float slideSpeed, Vector2 destination, GoEaseType easeType, Action onAnimationComplete = null)
		{
			base.transform = transform;
			duration = slideSpeed;
			this.destination = destination;
			base.onAnimationComplete = onAnimationComplete;
			base.easeType = easeType;
		}

		protected override void ConfigAnimation(ref GoTween tween, GoTweenConfig tweenConfig)
		{
			tweenConfig.easeType = easeType;
			tweenConfig.vector2Prop("anchoredPosition", destination);
		}
	}
}
