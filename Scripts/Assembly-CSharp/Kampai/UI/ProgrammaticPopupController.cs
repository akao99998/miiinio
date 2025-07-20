using System;
using UnityEngine;

namespace Kampai.UI
{
	public class ProgrammaticPopupController : IPopupController
	{
		private Vector2 targetAnchorMin;

		private Vector2 targetAnchorMax;

		private Vector3 targetScale;

		private Vector2 endAnchorPoint;

		private RectTransform myRectTransform;

		private Action onFinishClosing;

		private GoTween tween;

		private float openSpeed;

		private bool opened;

		public bool isOpened
		{
			get
			{
				return opened;
			}
		}

		public ProgrammaticPopupController(RectTransform rectTransform, Vector2 startAnchorPoint, Vector2 endAnchorPoint, float openSpeed, Action onFinishClosing)
		{
			myRectTransform = rectTransform;
			targetAnchorMin = myRectTransform.anchorMin;
			targetAnchorMax = myRectTransform.anchorMax;
			targetScale = myRectTransform.localScale;
			this.endAnchorPoint = endAnchorPoint;
			this.onFinishClosing = onFinishClosing;
			this.openSpeed = openSpeed;
			myRectTransform.anchorMin = startAnchorPoint;
			myRectTransform.anchorMax = startAnchorPoint;
			myRectTransform.localScale = Vector3.zero;
			myRectTransform.offsetMin = Vector2.zero;
			myRectTransform.offsetMax = Vector2.zero;
		}

		public void Open()
		{
			if (!opened)
			{
				if (tween != null)
				{
					tween.destroy();
				}
				tween = new GoTween(myRectTransform, openSpeed, new GoTweenConfig().setEaseType(GoEaseType.Linear).scale(targetScale).vector2Prop("anchorMin", targetAnchorMin)
					.vector2Prop("anchorMax", targetAnchorMax));
				Go.addTween(tween);
				tween.play();
				opened = true;
			}
		}

		public void Close(bool instant)
		{
			if (opened)
			{
				opened = false;
				if (tween != null)
				{
					tween.destroy();
				}
				tween = null;
				if (instant)
				{
					DestroyMenu();
					return;
				}
				tween = new GoTween(myRectTransform, openSpeed, new GoTweenConfig().setEaseType(GoEaseType.Linear).scale(Vector3.zero).vector2Prop("anchorMin", endAnchorPoint)
					.vector2Prop("anchorMax", endAnchorPoint)
					.onComplete(CloseComplete));
				Go.addTween(tween);
				tween.play();
			}
		}

		private void CloseComplete(AbstractGoTween tween)
		{
			if (tween != null)
			{
				tween.destroy();
			}
			tween = null;
			DestroyMenu();
		}

		private void DestroyMenu()
		{
			if (onFinishClosing != null)
			{
				onFinishClosing();
			}
		}
	}
}
