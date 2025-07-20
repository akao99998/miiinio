using Kampai.Util;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class MenuView : KampaiView
	{
		public Signal<bool> MenuMoved = new Signal<bool>();

		public Signal<bool> MenuMoveComplete = new Signal<bool>();

		internal UIAnimator animator;

		internal bool isOpen;

		public virtual float SlideSpeed
		{
			get
			{
				return 0.5f;
			}
		}

		internal virtual void Init()
		{
			animator = new UIAnimator();
			isOpen = false;
		}

		internal virtual void MoveMenu(bool show)
		{
			MoveMenu(show, Vector2.zero, Vector2.zero);
		}

		internal virtual void MoveAnchoredPosition(bool show, Vector2 offset, GoEaseType easeType = GoEaseType.ElasticOut)
		{
			if (show)
			{
				isOpen = true;
				MenuMoved.Dispatch(true);
				SetActive(true);
				animator.Play(new UIAnchorPositionSlideAnim(base.transform, SlideSpeed, offset, easeType, OnShowAnimationComplete), true);
			}
			else
			{
				isOpen = false;
				MenuMoved.Dispatch(false);
				animator.Play(new UIAnchorPositionSlideAnim(base.transform, SlideSpeed, offset, easeType, OnHideAnimationComplete), true);
			}
		}

		private void OnShowAnimationComplete()
		{
			MenuMoveComplete.Dispatch(true);
		}

		private void OnHideAnimationComplete()
		{
			MenuMoveComplete.Dispatch(false);
			SetActive(false);
		}

		protected void MoveMenu(bool show, Vector2 offset, GoEaseType easeType = GoEaseType.ElasticOut)
		{
			MoveMenu(show, offset, offset, easeType);
		}

		protected void MoveMenu(bool show, Vector2 offsetMin, Vector2 offsetMax, GoEaseType easeType = GoEaseType.ElasticOut)
		{
			if (show)
			{
				isOpen = true;
				MenuMoved.Dispatch(true);
				SetActive(true);
				animator.Play(new UIOffsetPositionSlideAnim(base.transform, SlideSpeed, offsetMin, offsetMax, easeType, OnShowAnimationComplete), true);
			}
			else if (!show)
			{
				isOpen = false;
				MenuMoved.Dispatch(false);
				animator.Play(new UIOffsetPositionSlideAnim(base.transform, SlideSpeed, offsetMin, offsetMax, easeType, OnHideAnimationComplete), true);
			}
		}

		internal virtual void SetActive(bool active)
		{
			base.gameObject.SetActive(active);
		}
	}
}
