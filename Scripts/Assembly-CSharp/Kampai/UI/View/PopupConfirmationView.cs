using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class PopupConfirmationView : PopupMenuView
	{
		public Text title;

		public Text description;

		public ButtonView Accept;

		public ButtonView Decline;

		public LocalizeView LeftButton;

		public LocalizeView RightButton;

		public override void Init()
		{
			base.Init();
			base.Open();
		}
	}
}
