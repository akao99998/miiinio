using Kampai.Main;

namespace Kampai.UI.View
{
	public class BillingNotAvailablePanelMediator : UIStackMediator<BillingNotAvailablePanelView>
	{
		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrimSignal { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			base.view.okButton.ClickedSignal.AddListener(Close);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.view.okButton.ClickedSignal.RemoveListener(Close);
		}

		protected override void Close()
		{
			soundFXSignal.Dispatch("Play_button_click_01");
			hideSkrimSignal.Dispatch("BillingSkrim");
			guiService.Execute(GUIOperation.Unload, "BillingNotAvailablePanel");
		}
	}
}
