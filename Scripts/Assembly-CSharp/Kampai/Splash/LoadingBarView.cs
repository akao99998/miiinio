using UnityEngine;
using UnityEngine.UI;
using strange.extensions.mediation.impl;

namespace Kampai.Splash
{
	public class LoadingBarView : strange.extensions.mediation.impl.View
	{
		internal GameObject meter_fill;

		internal Text txt_progress;

		public void Init()
		{
			meter_fill = base.gameObject.FindChild("meter_fill");
			txt_progress = base.gameObject.FindChild("txt_progressCounter").GetComponent<Text>();
		}

		public void SetText(string text)
		{
			txt_progress.text = text;
		}

		public void SetMeterFill(float fill)
		{
			meter_fill.GetComponent<RectTransform>().anchorMax = new Vector2(fill / 100f, 1f);
		}
	}
}
