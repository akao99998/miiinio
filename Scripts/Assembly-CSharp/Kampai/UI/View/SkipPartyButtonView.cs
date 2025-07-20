using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class SkipPartyButtonView : strange.extensions.mediation.impl.View
	{
		public ButtonView SkipButton;

		public GameObject SkipButtonCooldownMeter;

		public RectTransform SkipButtonCooldownFillImage;

		private Vector2 skipButtonCooldownFillAmount = new Vector2(1f, 1f);

		internal void ShowSkipPartyButtonView(bool display)
		{
			SkipButton.gameObject.SetActive(display);
		}

		internal void UpdateSkipMeterTime(float timeRemaining, float totalTime)
		{
			float x = timeRemaining / totalTime;
			skipButtonCooldownFillAmount.x = x;
			SkipButtonCooldownFillImage.anchorMax = skipButtonCooldownFillAmount;
		}
	}
}
