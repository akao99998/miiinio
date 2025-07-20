using Kampai.Common;
using Kampai.Main;

namespace Kampai.UI.View
{
	public class UsageSharingMediator : UIStackMediator<UsageSharingView>
	{
		[Inject]
		public UsageSharingAcceptedSignal usageSharingAccepted { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public ILocalizationService loc { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			base.view.yesButton.ClickedSignal.AddListener(YesClicked);
			base.view.noButton.ClickedSignal.AddListener(NoClicked);
			Init();
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.view.yesButton.ClickedSignal.RemoveListener(YesClicked);
			base.view.noButton.ClickedSignal.RemoveListener(NoClicked);
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			if (base.view != null)
			{
				Init();
			}
		}

		private void Init()
		{
			if (!telemetryService.SharingUsageEnabled())
			{
				base.view.usageSharingTitle.text = loc.GetString("UsageSharingEnableTitle");
				base.view.usageSharingDescription.text = loc.GetString("UsageSharingEnableDesc");
				base.view.usageSharingPrompt.text = loc.GetString("UsageSharingEnablePrompt");
			}
			else
			{
				base.view.usageSharingTitle.text = loc.GetString("UsageSharingDisableTitle");
				base.view.usageSharingDescription.text = loc.GetString("UsageSharingDisableDesc");
				base.view.usageSharingPrompt.text = loc.GetString("UsageSharingDisablePrompt");
			}
			base.view.yesButtonText.text = loc.GetString("UsageSharingYes");
			base.view.noButtonText.text = loc.GetString("UsageSharingNo");
		}

		private void YesClicked()
		{
			usageSharingAccepted.Dispatch(true);
		}

		private void NoClicked()
		{
			usageSharingAccepted.Dispatch(false);
		}

		protected override void Close()
		{
			usageSharingAccepted.Dispatch(false);
		}
	}
}
