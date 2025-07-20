using Kampai.Main;

namespace Kampai.UI.View
{
	public class TipsPopupMediator : UIStackMediator<TipsPopupView>
	{
		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrim { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			base.view.closeButton.ClickedSignal.AddListener(Close);
			base.view.gameObject.SetActive(false);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.view.closeButton.ClickedSignal.RemoveListener(Close);
		}

		protected override void Close()
		{
			hideSkrim.Dispatch("DidYouKnowSkrim");
			guiService.Execute(GUIOperation.Unload, "popup_Tip");
		}

		public override void Initialize(GUIArguments args)
		{
			string arg = args.Get<string>();
			string text = string.Format("<b>{0}</b> <color=black>{1}</color>", localizationService.GetString("DidYouKnow"), arg);
			base.view.Display(text);
			soundFXSignal.Dispatch("Play_menu_popUp_02");
		}
	}
}
