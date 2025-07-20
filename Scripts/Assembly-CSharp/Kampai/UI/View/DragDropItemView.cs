using Kampai.Util;
using UnityEngine;
using UnityEngine.EventSystems;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class DragDropItemView : KampaiView, IPointerDownHandler, IDragHandler, IEndDragHandler, IEventSystemHandler, IBeginDragHandler
	{
		public Transform ObjectToMove;

		public GameObject DragPromptItem;

		internal Signal<PointerEventData> OnDragSignal = new Signal<PointerEventData>();

		internal Signal<PointerEventData> OnDropSignal = new Signal<PointerEventData>();

		internal Signal<PointerEventData> OnStartSignal = new Signal<PointerEventData>();

		private CanvasGroup canvasGroup;

		internal void Init()
		{
			canvasGroup = base.gameObject.GetComponent<CanvasGroup>();
			if (canvasGroup == null)
			{
				canvasGroup = base.gameObject.AddComponent<CanvasGroup>();
			}
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			DragPromptItem.SetActive(false);
			OnStartSignal.Dispatch(eventData);
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			canvasGroup.blocksRaycasts = false;
		}

		public void OnDrag(PointerEventData eventData)
		{
			OnDragSignal.Dispatch(eventData);
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			canvasGroup.blocksRaycasts = true;
			OnDropSignal.Dispatch(eventData);
		}
	}
}
