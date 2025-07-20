using Kampai.Util;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class SellQuantityButtonView : ButtonView, IPointerDownHandler, IPointerUpHandler, IEventSystemHandler
	{
		public class HeldDownSignal : Signal<int>
		{
		}

		internal readonly float COUNT_WAIT_TIME = 0.25f;

		internal readonly float PRICE_INIT_WAIT_TIME = 0.1f;

		internal readonly float PRICE_MAX_WAIT_TIME = 1.5f;

		public HeldDownSignal heldDownSignal = new HeldDownSignal();

		public Signal<PointerEventData> OnPointerUpSignal = new Signal<PointerEventData>();

		public Signal<PointerEventData> OnPointerDownSignal = new Signal<PointerEventData>();

		private bool m_isEnabled;

		internal bool IsHeldDown { get; set; }

		public int MaxValue { get; set; }

		public int MinValue { get; set; }

		public bool IsPriceButton { get; set; }

		protected override void Awake()
		{
			KampaiView.BubbleToContextOnAwake(this, ref currentContext, true);
		}

		public override void OnClickEvent()
		{
			ClickedSignal.Dispatch();
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			IsHeldDown = true;
			Button component = GetComponent<Button>();
			m_isEnabled = component != null && component.interactable;
			OnPointerDownSignal.Dispatch(eventData);
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			IsHeldDown = false;
			OnPointerUpSignal.Dispatch(eventData);
			if (m_isEnabled)
			{
				m_isEnabled = false;
			}
		}

		public void SetSize(float height)
		{
			RectTransform rectTransform = base.transform as RectTransform;
			if (!(rectTransform == null))
			{
				rectTransform.sizeDelta = new Vector2(height, height);
			}
		}
	}
}
