using UnityEngine;
using UnityEngine.EventSystems;

namespace Kampai.UI.View
{
	public class LevelUpButtonView : ButtonView, IPointerDownHandler, IPointerUpHandler, IEventSystemHandler
	{
		private bool isDragging;

		private float currentOffset;

		private Vector2 startPos;

		private float MaxClickOffset
		{
			get
			{
				return 0.15f * Screen.dpi;
			}
		}

		public override void OnClickEvent()
		{
			if (PlaySoundOnClick)
			{
				base.playSFXSignal.Dispatch("Play_button_click_01");
			}
			if (!isDragging)
			{
				ClickedSignal.Dispatch();
			}
		}

		public virtual void OnPointerDown(PointerEventData eventData)
		{
			startPos = eventData.position;
		}

		public virtual void OnPointerUp(PointerEventData eventData)
		{
			currentOffset = (eventData.position - startPos).magnitude;
			if (currentOffset < MaxClickOffset)
			{
				isDragging = false;
			}
			else
			{
				isDragging = true;
			}
		}
	}
}
