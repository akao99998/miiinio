using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class SkrimView : KampaiView
	{
		public ButtonView ClickButton;

		public GameObject DarkSkrim;

		public Image DarkSkrimImage;

		public bool singleSkrimClose { get; set; }

		public bool genericPopupSkrim { get; set; }

		internal void Init(float alpha, bool fadeIn)
		{
			(base.transform as RectTransform).offsetMax = Vector2.zero;
			(base.transform as RectTransform).offsetMin = Vector2.zero;
			ClickButton.PlaySoundOnClick = false;
			Color color = DarkSkrimImage.color;
			color.a = alpha;
			DarkSkrimImage.color = color;
			if (fadeIn)
			{
				DarkSkrimImage.CrossFadeAlpha(0f, 0f, true);
			}
		}

		public void EnableSkrimButton(bool enable)
		{
			ClickButton.GetComponent<Button>().interactable = enable;
		}

		internal void SetDarkSkrimActive(bool enabled, float duration)
		{
			DarkSkrim.SetActive(enabled);
			if (duration != 0f)
			{
				FadeDarkSkrim(1f, duration);
			}
		}

		internal void FadeDarkSkrim(float alpha, float duration)
		{
			DarkSkrimImage.CrossFadeAlpha(alpha, duration, false);
		}
	}
}
