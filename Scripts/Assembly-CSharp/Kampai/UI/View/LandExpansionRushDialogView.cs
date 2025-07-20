using UnityEngine;

namespace Kampai.UI.View
{
	public class LandExpansionRushDialogView : RushDialogView
	{
		public RectTransform CurrencyScrollViewParent;

		internal override void SetupDialog(RushDialogType type, bool showPurchaseButton)
		{
			switch (type)
			{
			case RushDialogType.BRIDGE_QUEST:
				Title.text = localService.GetString("RepairBridge");
				break;
			case RushDialogType.LAND_EXPANSION:
				Title.text = localService.GetString("ExpandLandPrompt");
				break;
			}
			base.SetupDialog(type, showPurchaseButton);
		}
	}
}
