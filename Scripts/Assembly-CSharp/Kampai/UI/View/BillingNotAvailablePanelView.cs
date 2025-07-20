using Kampai.Util;
using UnityEngine;

namespace Kampai.UI.View
{
	public class BillingNotAvailablePanelView : KampaiView
	{
		public ButtonView okButton;

		protected override void Start()
		{
			RectTransform rectTransform = base.transform as RectTransform;
			rectTransform.anchoredPosition = Vector2.zero;
			base.Start();
		}
	}
}
