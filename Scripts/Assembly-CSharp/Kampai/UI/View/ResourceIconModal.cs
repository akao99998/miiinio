using UnityEngine.UI;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class ResourceIconModal : WorldToGlassUIModal
	{
		public KampaiImage TextBackground;

		public KampaiImage Image;

		public Text Text;

		public KampaiImage BackingGlow;

		public KampaiImage BackingPartyGlow;

		public KampaiImage Backing;

		public KampaiImage BackingStroke;

		public Signal ClickedSignal = new Signal();

		public void OnClickEvent()
		{
			ClickedSignal.Dispatch();
		}

		public void EnablePartyIcon()
		{
			if (BackingPartyGlow != null)
			{
				BackingPartyGlow.gameObject.SetActive(true);
			}
			if (BackingGlow != null)
			{
				BackingGlow.gameObject.SetActive(false);
			}
			if (Backing != null)
			{
				Backing.gameObject.SetActive(false);
			}
			if (BackingStroke != null)
			{
				BackingStroke.gameObject.SetActive(false);
			}
		}
	}
}
