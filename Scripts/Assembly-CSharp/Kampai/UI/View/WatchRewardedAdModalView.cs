using Kampai.Main;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class WatchRewardedAdModalView : PopupMenuView
	{
		public Text Headline1;

		public KampaiImage RewardItemImage;

		public Text RewardAmountText;

		public Button YesButton;

		public Button NoButton;

		public Text YesButtonText;

		public Text NoButtonText;

		private ILocalizationService localService;

		internal void Init(ILocalizationService localService)
		{
			base.Init();
			this.localService = localService;
			Localize();
			base.Open();
		}

		private void Localize()
		{
			Headline1.text = localService.GetString("RewardedAdWatchVideoHeadline1");
			YesButtonText.text = localService.GetString("RewardedAdWatch");
			NoButtonText.text = localService.GetString("RewardedAdNoThanks");
		}
	}
}
