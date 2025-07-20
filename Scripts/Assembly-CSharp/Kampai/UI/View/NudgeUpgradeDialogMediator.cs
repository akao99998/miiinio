using Elevation.Logging;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;

namespace Kampai.UI.View
{
	public class NudgeUpgradeDialogMediator : UIStackMediator<NudgeUpgradeDialogView>
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("NudgeUpgradeDialogMediator") as IKampaiLogger;

		private string storeUrl;

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrimSignal { get; set; }

		public override void Initialize(GUIArguments args)
		{
			storeUrl = args.Get<string>();
		}

		public override void OnRegister()
		{
			base.OnRegister();
			base.view.titleText.text = "Update Available";
			base.view.messageText.text = localizationService.GetString("NudgeClientUpgradeMessage");
			base.view.upgradeText.text = "Update";
			base.view.cancelText.text = localizationService.GetString("CancelText");
			base.view.upgradeButton.ClickedSignal.AddListener(OnUpgradeClicked);
			base.view.cancelButton.ClickedSignal.AddListener(OnCancelClicked);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.view.upgradeButton.ClickedSignal.RemoveListener(OnUpgradeClicked);
			base.view.cancelButton.ClickedSignal.RemoveListener(OnCancelClicked);
		}

		private void OnUpgradeClicked()
		{
			GoToAppStore();
			Close();
		}

		private void OnCancelClicked()
		{
			Close();
		}

		private void GoToAppStore()
		{
			logger.Log(KampaiLogLevel.Info, "Going to store to upgrade client!");
			if (!string.IsNullOrEmpty(storeUrl))
			{
				Application.OpenURL(storeUrl);
			}
		}

		protected override void Close()
		{
			guiService.Execute(GUIOperation.Unload, "popup_NudgeUpgrade");
			hideSkrimSignal.Dispatch("ClientUpgradeSkrim");
		}
	}
}
