using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class SocialPartyEventEndView : PopupMenuView
	{
		public Text TitleText;

		public Text MessageText;

		public Text YesButtonText;

		public ButtonView YesButton;

		public override void Init()
		{
			base.Init();
			base.Open();
		}
	}
}
