using Kampai.Util;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class RewardSliderView : KampaiView, IPointerDownHandler, IDragHandler, IPointerUpHandler, IEventSystemHandler
	{
		public Text description;

		public Text itemQuantity;

		public KampaiImage icon;

		public Signal pointerDownSignal = new Signal();

		public Signal pointerUpSignal = new Signal();

		public int ID { get; set; }

		public ScrollRect scrollRect { get; set; }

		public void OnPointerDown(PointerEventData eventData)
		{
			scrollRect.OnBeginDrag(eventData);
			pointerDownSignal.Dispatch();
		}

		public void OnDrag(PointerEventData eventData)
		{
			scrollRect.OnDrag(eventData);
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			scrollRect.OnEndDrag(eventData);
			pointerUpSignal.Dispatch();
		}
	}
}
