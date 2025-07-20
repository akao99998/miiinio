using Kampai.Game;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class StoreTabView : ScrollableButtonView
	{
		public Text TabName;

		public StoreBadgeView TabBadgeCount;

		public KampaiImage TabIcon;

		public float PaddingInPixel;

		public new Signal<StoreItemType, string> ClickedSignal = new Signal<StoreItemType, string>();

		public StoreItemType Type { get; set; }

		public override void ButtonClicked()
		{
			ClickedSignal.Dispatch(Type, TabName.text);
		}

		internal void SetBadgeCount(int badgeCount)
		{
			TabBadgeCount.SetBadgeCount(Mathf.Abs(TabBadgeCount.CurrentBadgeCount - badgeCount));
		}

		internal void SetNewUnlockState(int badgeCount)
		{
			TabBadgeCount.SetNewUnlockCounter(badgeCount);
		}
	}
}
