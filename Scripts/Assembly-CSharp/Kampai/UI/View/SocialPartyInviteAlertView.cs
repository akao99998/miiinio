using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class SocialPartyInviteAlertView : PopupMenuView
	{
		public ButtonView acceptButton;

		public ButtonView declineButton;

		public ButtonView nextButton;

		public ButtonView previousButton;

		public ButtonView closeButton;

		public Text acceptButtonText;

		public Text declineButtonText;

		public Text socialPartyDescription;

		public Text recipesFilledDescription;

		public Text title;

		public Text playerName;

		public Image playerImage;

		public override void Init()
		{
			base.Init();
			base.Open();
		}
	}
}
