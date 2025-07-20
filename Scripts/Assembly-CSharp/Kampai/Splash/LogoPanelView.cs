using Kampai.Util;
using UnityEngine;

namespace Kampai.Splash
{
	public class LogoPanelView : KampaiView
	{
		private GameObject progressBar;

		private GameObject wiFiPopup;

		public void SetupRefs()
		{
			progressBar = base.gameObject.FindChild("meter_bar");
		}

		public void ShowNoWiFi(bool show)
		{
			progressBar.SetActive(!show);
			wiFiPopup.SetActive(show);
		}

		public void SetNoWifiPanel(GameObject wiFiPopup)
		{
			this.wiFiPopup = wiFiPopup;
		}
	}
}
