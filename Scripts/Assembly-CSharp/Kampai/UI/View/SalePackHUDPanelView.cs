using Kampai.Util;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class SalePackHUDPanelView : KampaiView
	{
		public ScrollRect scrollList;

		public void Init()
		{
			scrollList.enabled = true;
		}
	}
}
