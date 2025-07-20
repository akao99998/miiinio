using Kampai.Main;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class NotificationsView : PopupMenuView
	{
		public Text MessageText;

		public Text buttonText;

		public ButtonView confirmButton;

		private ILocalizationService localService;

		internal void Init(ILocalizationService locService, string message)
		{
			base.Init();
			localService = locService;
			MessageText.text = message;
			buttonText.text = localService.GetString("socialpartycompletedbutton");
			base.Open();
		}
	}
}
