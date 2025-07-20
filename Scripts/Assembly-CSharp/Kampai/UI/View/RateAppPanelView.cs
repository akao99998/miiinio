using Kampai.Util;
using UnityEngine;

namespace Kampai.UI.View
{
	public class RateAppPanelView : KampaiView
	{
		public ButtonView closeButton;

		public ButtonView rateButton;

		public ButtonView notNowButton;

		public ButtonView neverButton;

		protected override void Start()
		{
			RectTransform rectTransform = base.transform as RectTransform;
			rectTransform.anchoredPosition = Vector2.zero;
			base.Start();
		}
	}
}
