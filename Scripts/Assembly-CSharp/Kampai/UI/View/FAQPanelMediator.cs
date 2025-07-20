using Kampai.Main;

namespace Kampai.UI.View
{
	public class FAQPanelMediator : UIStackMediator<FAQPanelView>
	{
		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		public override void OnRegister()
		{
			base.view.closeButton.ClickedSignal.AddListener(CloseButton);
			base.OnRegister();
		}

		public override void OnRemove()
		{
			base.view.closeButton.ClickedSignal.RemoveListener(CloseButton);
			base.OnRemove();
		}

		private void CloseButton()
		{
			soundFXSignal.Dispatch("Play_button_click_01");
			Close();
		}

		protected override void Close()
		{
			base.gameObject.SetActive(false);
		}
	}
}
