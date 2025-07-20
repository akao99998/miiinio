using Elevation.Logging;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.ForcedUpgrade.View
{
	public class ForcedUpgradeMediator : Mediator
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("ForcedUpgradeMediator") as IKampaiLogger;

		[Inject]
		public ForcedUpgradeView view { get; set; }

		[Inject]
		public InitLocalizationServiceSignal initLocalizationServiceSignal { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		public override void OnRegister()
		{
			initLocalizationServiceSignal.Dispatch();
			view.titleText.text = localizationService.GetString("ClientUpgradeTitle");
			view.messageText.text = localizationService.GetString("ForcedClientUpgradeMessage");
			view.buttonText.text = localizationService.GetString("Update");
			view.storeButton.ClickedSignal.AddListener(OnStoreClick);
		}

		public override void OnRemove()
		{
			view.storeButton.ClickedSignal.RemoveListener(OnStoreClick);
		}

		private void OnStoreClick()
		{
			logger.Debug("Going to the store!");
			Application.OpenURL("market://details?id=com.ea.gp.minions");
		}
	}
}
