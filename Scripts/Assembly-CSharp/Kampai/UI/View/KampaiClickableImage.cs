using UnityEngine.EventSystems;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class KampaiClickableImage : KampaiImage, IPointerClickHandler, IEventSystemHandler
	{
		public Signal ClickedSignal = new Signal();

		private bool enableClick;

		public void OnPointerClick(PointerEventData eventData)
		{
			if (enableClick)
			{
				ClickedSignal.Dispatch();
			}
		}

		public void EnableClick(bool enable)
		{
			enableClick = enable;
		}
	}
}
