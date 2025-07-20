using UnityEngine;
using UnityEngine.UI;
using strange.extensions.signal.impl;

namespace Kampai.BuildingsSizeToolbox
{
	public class UpsellScreenSelectorListItemView : MonoBehaviour
	{
		public Text ScreenName;

		public Signal<string> ClickedSignal = new Signal<string>();

		public void OnClick()
		{
			ClickedSignal.Dispatch(ScreenName.text);
		}
	}
}
