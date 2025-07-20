using UnityEngine;
using UnityEngine.UI;
using strange.extensions.mediation.impl;

namespace Kampai.Splash
{
	public class LoadInTipView : strange.extensions.mediation.impl.View
	{
		internal void SetTip(string tip)
		{
			GameObject gameObject = base.gameObject.FindChild("txt_ToolTip");
			gameObject.GetComponent<Text>().text = tip;
		}
	}
}
