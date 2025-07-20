using UnityEngine;
using UnityEngine.UI;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class PartyMeterView : strange.extensions.mediation.impl.View
	{
		public GameObject PartyMeterPanel;

		public Text CountDownTimerText;

		public Text BuffText;

		public KampaiImage BuffIcon;

		internal void DisplayCooldownMeter(bool display)
		{
			PartyMeterPanel.SetActive(display);
		}

		internal void UpdateCountDownText(string text)
		{
			CountDownTimerText.text = text;
		}

		internal void UpdateBuffText(string text)
		{
			BuffText.text = text;
		}

		internal void UpdateBuffIcon(string path)
		{
			BuffIcon.maskSprite = UIUtils.LoadSpriteFromPath(path);
		}
	}
}
