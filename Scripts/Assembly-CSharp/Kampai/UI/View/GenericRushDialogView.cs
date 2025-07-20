using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class GenericRushDialogView : RushDialogView
	{
		public Text PurchaseDescriptionText;

		public Text HeadlineTitle;

		public ButtonView AdVideoButton;

		internal override void SetupDialog(RushDialogType type, bool showPurchaseButton)
		{
			switch (type)
			{
			case RushDialogType.DEFAULT:
				Title.text = localService.GetString("NotEnough");
				PurchaseDescriptionText.text = localService.GetString("NotEnoughResources");
				break;
			case RushDialogType.SOCIAL:
				Title.text = localService.GetString("SocialPartyNotEnough");
				PurchaseDescriptionText.text = localService.GetString("SocialPartyNotEnoughResources");
				break;
			default:
				Title.text = localService.GetString("YouNeed");
				PurchaseDescriptionText.text = localService.GetString("YouNeedResources");
				break;
			}
			base.SetupDialog(type, showPurchaseButton);
		}

		public void EnableRewardedAdRushButton(bool adButtonEnabled)
		{
			AdVideoButton.gameObject.SetActive(adButtonEnabled);
		}
	}
}
