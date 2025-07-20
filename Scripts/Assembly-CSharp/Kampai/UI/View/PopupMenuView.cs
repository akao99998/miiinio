using Elevation.Logging;
using Kampai.Util;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class PopupMenuView : KampaiView
	{
		[Header("PopupMenu Property")]
		public float OpenSpeed = 0.3f;

		public Signal OnMenuClose = new Signal();

		public IKampaiLogger logger = LogManager.GetClassLogger("PopupMenuView") as IKampaiLogger;

		private IPopupController controller;

		protected Animator animator
		{
			get
			{
				AnimatorPopupController animatorPopupController = controller as AnimatorPopupController;
				return (animatorPopupController == null) ? null : animatorPopupController.currentAnimator;
			}
		}

		protected bool isOpened
		{
			get
			{
				return controller.isOpened;
			}
		}

		protected override void Awake()
		{
			if (Application.isPlaying)
			{
				base.Awake();
			}
		}

		public virtual void Init()
		{
			Animator component = GetComponent<Animator>();
			if (component == null)
			{
				logger.Log(KampaiLogLevel.Error, "PopupMenuView has a NULL animator on Init!!!");
			}
			else
			{
				controller = new AnimatorPopupController(component, this, logger, FinishClosing);
			}
		}

		public void InitProgrammatic(BuildingPopupPositionData buildingPopupPositionData)
		{
			controller = new ProgrammaticPopupController(base.transform as RectTransform, buildingPopupPositionData.StartPosition, buildingPopupPositionData.EndPosition, OpenSpeed, FinishClosing);
		}

		public void OpenInstantly(int defaultLayer, float lastFrame)
		{
			AnimatorPopupController animatorPopupController = controller as AnimatorPopupController;
			if (animatorPopupController != null)
			{
				animatorPopupController.OpenInstantly(defaultLayer, lastFrame);
			}
		}

		private void FinishClosing()
		{
			base.gameObject.SetActive(false);
			OnMenuClose.Dispatch();
			StopAllCoroutines();
		}

		internal virtual void Open()
		{
			if (controller != null)
			{
				base.gameObject.SetActive(true);
				controller.Open();
			}
			else
			{
				logger.Error("Popup Controller is null, make sure you init it properly!");
			}
		}

		public virtual void Close(bool instant = false)
		{
			if (controller != null)
			{
				controller.Close(instant);
			}
			else
			{
				logger.Error("Popup Controller is null, make sure you init it properly!");
			}
		}

		public bool IsAnimationPlaying(string animationState)
		{
			AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
			if (currentAnimatorStateInfo.IsName(animationState) && currentAnimatorStateInfo.normalizedTime < 1f)
			{
				return true;
			}
			if (currentAnimatorStateInfo.loop || currentAnimatorStateInfo.normalizedTime < 1f)
			{
				return true;
			}
			return false;
		}

		public virtual void FinishedOpening()
		{
		}
	}
}
