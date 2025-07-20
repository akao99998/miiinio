using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class MessagePopupView : KampaiView
	{
		public RectTransform PopupBox;

		private Text popupText;

		private KampaiImage popupImage;

		private float timer;

		private float disableTime = 3f;

		private float fadeTime = 0.1f;

		private Color popupTextColorOn;

		private Color popupTextColorOff;

		private Color popupImageColorOn;

		private Color popupImageColorOff;

		public bool AutoClose = true;

		public Signal DialogClosedSignal;

		internal void Init()
		{
			popupText = PopupBox.GetComponentInChildren<Text>();
			popupImage = PopupBox.GetComponent<KampaiImage>();
			popupTextColorOn = new Color(popupText.color.r, popupText.color.g, popupText.color.b, 1f);
			popupTextColorOff = new Color(popupText.color.r, popupText.color.g, popupText.color.b, 0f);
			popupImageColorOn = new Color(popupImage.color.r, popupImage.color.g, popupImage.color.b, 1f);
			popupImageColorOff = new Color(popupImage.color.r, popupImage.color.g, popupImage.color.b, 0f);
			popupText.color = popupTextColorOff;
			popupImage.color = popupImageColorOff;
			DialogClosedSignal = new Signal();
			base.gameObject.SetActive(false);
		}

		internal void SetCustomTiming(float fadeUITime, float openTime)
		{
			disableTime = openTime;
			fadeTime = fadeUITime;
		}

		internal void Display(string text, MessagePopUpAnchor anchor, Vector2 anchorPosition)
		{
			switch (anchor)
			{
			case MessagePopUpAnchor.TOP_LEFT:
				PopupBox.anchorMin = Vector2.up;
				PopupBox.anchorMax = Vector2.up;
				PopupBox.pivot = Vector2.up;
				break;
			case MessagePopUpAnchor.TOP_RIGHT:
				PopupBox.anchorMin = Vector2.one;
				PopupBox.anchorMax = Vector2.one;
				PopupBox.pivot = Vector2.one;
				break;
			case MessagePopUpAnchor.TOP_CENTER:
			{
				Vector2 vector2 = new Vector2(0.5f, 1f);
				PopupBox.anchorMin = vector2;
				PopupBox.anchorMax = vector2;
				PopupBox.pivot = vector2;
				break;
			}
			case MessagePopUpAnchor.BOTTOM_LEFT:
				PopupBox.anchorMin = Vector2.zero;
				PopupBox.anchorMax = Vector2.zero;
				PopupBox.pivot = Vector2.zero;
				break;
			case MessagePopUpAnchor.BOTTOM_RIGHT:
				PopupBox.anchorMin = Vector2.right;
				PopupBox.anchorMax = Vector2.right;
				PopupBox.pivot = Vector2.right;
				break;
			case MessagePopUpAnchor.CENTER:
			{
				Vector2 vector = new Vector2(0.5f, 0.5f);
				PopupBox.anchorMin = vector;
				PopupBox.anchorMax = vector;
				PopupBox.pivot = vector;
				break;
			}
			case MessagePopUpAnchor.CUSTOM:
				PopupBox.anchorMin = anchorPosition;
				PopupBox.anchorMax = anchorPosition;
				PopupBox.pivot = new Vector2(0.5f, 0.5f);
				break;
			}
			popupText.color = popupTextColorOff;
			popupImage.color = popupImageColorOff;
			popupText.text = text;
			base.gameObject.SetActive(true);
			base.transform.SetAsLastSibling();
			FadeIn();
		}

		private void Update()
		{
			if (AutoClose && timer > 0f)
			{
				timer -= Time.deltaTime;
				if (timer <= 0f)
				{
					FadeOut();
				}
			}
		}

		public void Show(bool enabled)
		{
			if (enabled)
			{
				FadeIn();
			}
			else
			{
				FadeOut();
			}
		}

		private void FadeIn()
		{
			Go.to(popupText, fadeTime, new GoTweenConfig().colorProp("color", popupTextColorOn).onComplete(delegate
			{
				timer = disableTime;
			}));
			Go.to(popupImage, fadeTime, new GoTweenConfig().colorProp("color", popupImageColorOn));
		}

		private void FadeOut()
		{
			Go.to(popupText, fadeTime, new GoTweenConfig().colorProp("color", popupTextColorOff).onComplete(delegate
			{
				popupText.text = string.Empty;
				base.gameObject.SetActive(false);
				DialogClosedSignal.Dispatch();
			}));
			Go.to(popupImage, fadeTime, new GoTweenConfig().colorProp("color", popupImageColorOff));
		}
	}
}
