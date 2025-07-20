using System;
using System.Collections;
using Kampai.Util;
using UnityEngine;

namespace Kampai.UI
{
	public class AnimatorPopupController : IPopupController
	{
		private IKampaiLogger logger;

		private MonoBehaviour behavior;

		private Animator animator;

		private bool opened;

		private Action onFinishClosing;

		private IEnumerator disableOnFinishEnumerator;

		public Animator currentAnimator
		{
			get
			{
				return animator;
			}
		}

		public bool isOpened
		{
			get
			{
				return opened;
			}
		}

		public AnimatorPopupController(Animator animator, MonoBehaviour behavior, IKampaiLogger logger, Action onFinishClosing)
		{
			this.animator = animator;
			this.behavior = behavior;
			this.logger = logger;
			this.onFinishClosing = onFinishClosing;
		}

		public void Open()
		{
			if (!opened && animator != null)
			{
				if (disableOnFinishEnumerator != null)
				{
					behavior.StopCoroutine(disableOnFinishEnumerator);
					disableOnFinishEnumerator = null;
				}
				animator.Play("Open");
				opened = true;
			}
		}

		public void OpenInstantly(int defaultLayer, float lastFrame)
		{
			animator.Play("Open", defaultLayer, lastFrame);
			opened = true;
		}

		public void Close(bool instant)
		{
			if (opened)
			{
				opened = false;
				if (disableOnFinishEnumerator != null)
				{
					behavior.StopCoroutine(disableOnFinishEnumerator);
					disableOnFinishEnumerator = null;
				}
				if (instant)
				{
					DestroyMenu();
				}
				else if (animator != null)
				{
					animator.Play("Close");
					disableOnFinishEnumerator = DisableOnFinish();
					behavior.StartCoroutine(disableOnFinishEnumerator);
				}
				else
				{
					logger.Log(KampaiLogLevel.Error, "PopupMenuView has a NULL animator on Close!!!");
					DestroyMenu();
				}
			}
		}

		private IEnumerator DisableOnFinish()
		{
			yield return null;
			float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
			yield return new WaitForSeconds(animationLength);
			yield return null;
			disableOnFinishEnumerator = null;
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
