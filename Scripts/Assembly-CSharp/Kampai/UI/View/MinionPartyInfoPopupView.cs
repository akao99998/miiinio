using Kampai.Main;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class MinionPartyInfoPopupView : PopupMenuView, IPointerDownHandler, IPointerUpHandler, IEventSystemHandler, IGenericPopupView
	{
		public Text partyPointsText;

		public float offsetValue = -7f;

		public RectTransform FillImage;

		public Signal<PointerEventData> pointerDownSignal = new Signal<PointerEventData>();

		public Signal<PointerEventData> pointerUpSignal = new Signal<PointerEventData>();

		public int partyPointsTweenCount { get; set; }

		public void Init(ILocalizationService localizationService)
		{
			base.Init();
		}

		public void Display(Vector3 itemCenter)
		{
			base.Init();
			RectTransform rectTransform = base.transform as RectTransform;
			Vector3 anchoredPosition3D = rectTransform.sizeDelta / offsetValue;
			anchoredPosition3D.x = 0f;
			rectTransform.anchoredPosition3D = anchoredPosition3D;
			rectTransform.anchorMin = itemCenter;
			rectTransform.anchorMax = itemCenter;
			base.Open();
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			pointerDownSignal.Dispatch(eventData);
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			pointerUpSignal.Dispatch(eventData);
		}

		public void SetPartyPoints(uint partyPoints, uint maxPartyPoints, bool animate = true)
		{
			if (!animate)
			{
				SetPartyPointsText(partyPoints, maxPartyPoints);
				FillImage.anchorMax = new Vector2((float)partyPoints / (float)maxPartyPoints, 1f);
				return;
			}
			SetPartyPointsText(partyPoints, maxPartyPoints);
			Go.to(FillImage, 1f, new GoTweenConfig().vector2Prop("anchorMax", new Vector2((float)partyPoints / (float)maxPartyPoints, 1f)).onComplete(delegate
			{
				if (partyPoints > maxPartyPoints)
				{
					partyPoints = maxPartyPoints;
				}
			}));
		}

		public void SetPartyPointsText(uint partyPoints, uint maxPartyPoints)
		{
			partyPointsText.text = string.Format("{0}/{1}", partyPoints, maxPartyPoints);
		}
	}
}
