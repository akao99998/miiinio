using Kampai.Util;
using UnityEngine.EventSystems;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class BuffButtonView : KampaiView, IPointerDownHandler, IEventSystemHandler
	{
		public bool pulse;

		public float popupOffset;

		public Signal pointerDownSignal = new Signal();

		public virtual void OnPointerDown(PointerEventData eventData)
		{
			pointerDownSignal.Dispatch();
		}
	}
}
