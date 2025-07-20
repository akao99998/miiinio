using Kampai.Common;
using Kampai.Util;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class BuffButtonMediator : Mediator
	{
		private Vector3 originalScale;

		[Inject]
		public BuffButtonView view { get; set; }

		[Inject(UIElement.CAMERA)]
		public Camera uiCamera { get; set; }

		[Inject]
		public ShowBuffInfoPopupSignal showBuffInfoPopupSignal { get; set; }

		[Inject]
		public PickControllerModel pickModel { get; set; }

		public override void OnRegister()
		{
			view.pointerDownSignal.AddListener(ShowBuffPopup);
			originalScale = view.transform.localScale;
			if (view.pulse)
			{
				TweenUtil.Throb(view.transform, 0.85f, 0.5f, out originalScale);
			}
		}

		public override void OnRemove()
		{
			view.transform.localScale = originalScale;
			view.pointerDownSignal.RemoveListener(ShowBuffPopup);
			Go.killAllTweensWithTarget(view.transform);
		}

		private void ShowBuffPopup()
		{
			if (!pickModel.PanningCameraBlocked)
			{
				Vector3[] array = new Vector3[4];
				(view.gameObject.transform as RectTransform).GetWorldCorners(array);
				Vector3 position = default(Vector3);
				Vector3[] array2 = array;
				foreach (Vector3 vector in array2)
				{
					position += vector;
				}
				position /= 4f;
				showBuffInfoPopupSignal.Dispatch(uiCamera.WorldToViewportPoint(position), view.popupOffset);
			}
		}
	}
}
