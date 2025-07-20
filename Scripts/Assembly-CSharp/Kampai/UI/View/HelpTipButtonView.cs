using System.Collections;
using Kampai.Game;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kampai.UI.View
{
	public class HelpTipButtonView : ButtonView, IPointerDownHandler, IDragHandler, IPointerUpHandler, IEventSystemHandler
	{
		public int tipDefinitionId;

		private IEnumerator autoCloseCoroutine;

		public RectTransform rectTransform;

		[Inject]
		public DisplayItemPopupSignal displayItemPopupSignal { get; set; }

		[Inject]
		public HideItemPopupSignal hideItemPopupSignal { get; set; }

		[Inject]
		public IHelpTipTrackingService helpTipTrackingService { get; set; }

		protected override void Awake()
		{
			base.Awake();
			rectTransform = base.gameObject.GetComponent<RectTransform>();
		}

		public override void OnClickEvent()
		{
			ClickedSignal.Dispatch();
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (autoCloseCoroutine != null)
			{
				StopCoroutine(autoCloseCoroutine);
				autoCloseCoroutine = null;
			}
			displayItemPopupSignal.Dispatch(tipDefinitionId, rectTransform, UIPopupType.HELPTIP);
			helpTipTrackingService.TrackHelpTipShown(tipDefinitionId);
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			autoCloseCoroutine = CloseTipPopup();
			StartCoroutine(autoCloseCoroutine);
		}

		private IEnumerator CloseTipPopup()
		{
			yield return new WaitForSeconds(3f);
			hideItemPopupSignal.Dispatch();
			autoCloseCoroutine = null;
		}

		public void OnDrag(PointerEventData eventData)
		{
		}
	}
}
