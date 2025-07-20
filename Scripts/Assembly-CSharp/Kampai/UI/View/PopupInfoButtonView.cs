using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class PopupInfoButtonView : ButtonView, IPointerDownHandler, IDragHandler, IPointerUpHandler, IEndDragHandler, IEventSystemHandler, IBeginDragHandler
	{
		public Signal pointerDownSignal = new Signal();

		public Signal pointerUpSignal = new Signal();

		private int pointerId;

		private bool dragFinished;

		private Vector2 currentDragDelta;

		private ScrollRect scrollRect;

		public virtual void OnPointerDown(PointerEventData eventData)
		{
			pointerId = eventData.pointerId;
			pointerDownSignal.Dispatch();
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			pointerUpSignal.Dispatch();
		}

		public override void OnClickEvent()
		{
			ClickedSignal.Dispatch();
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (scrollRect != null)
			{
				scrollRect.OnDrag(eventData);
			}
			RectTransform rectTransform = base.transform as RectTransform;
			currentDragDelta += eventData.delta;
			if (!dragFinished && (Mathf.Abs(currentDragDelta.y) > rectTransform.rect.height / 2f || Mathf.Abs(currentDragDelta.x) > rectTransform.rect.width))
			{
				pointerUpSignal.Dispatch();
				dragFinished = true;
			}
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			dragFinished = false;
			currentDragDelta = Vector2.zero;
			if (scrollRect != null)
			{
				scrollRect.OnBeginDrag(eventData);
			}
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if (!dragFinished && pointerId == eventData.pointerId)
			{
				pointerUpSignal.Dispatch();
			}
			if (scrollRect != null)
			{
				scrollRect.OnEndDrag(eventData);
			}
		}

		protected virtual void OnEnable()
		{
			scrollRect = base.transform.GetComponentTypeInParent<ScrollRect>();
		}

		protected override void Start()
		{
			base.Start();
			scrollRect = base.transform.GetComponentTypeInParent<ScrollRect>();
		}
	}
}
