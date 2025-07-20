using Kampai.Main;

namespace Kampai.UI.View
{
	public class TSMHelpModalMediator : UIStackMediator<TSMHelpModalView>
	{
		[Inject]
		public IFancyUIService fancyUIService { get; set; }

		[Inject]
		public CloseAllOtherMenuSignal closeSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrimSignal { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public MoveAudioListenerSignal moveAudioListenerSignal { get; set; }

		public override void Initialize(GUIArguments args)
		{
			base.Initialize(args);
			base.view.InitializeView(fancyUIService, args.Get<TSMHelpModalArguments>(), moveAudioListenerSignal);
		}

		protected override void Close()
		{
			moveAudioListenerSignal.Dispatch(true, null);
			playSFXSignal.Dispatch("Play_menu_disappear_01");
			base.view.Close();
		}

		public override void OnRegister()
		{
			closeSignal.Dispatch(null);
			base.OnRegister();
			base.view.OnMenuClose.AddListener(OnMenuClose);
			base.view.Button.ClickedSignal.AddListener(OnOkClicked);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			CleanupListeners();
		}

		private void CleanupListeners()
		{
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
			base.view.Button.ClickedSignal.RemoveListener(OnOkClicked);
		}

		private void OnMenuClose()
		{
			hideSkrimSignal.Dispatch("ProceduralTaskSkrim");
			guiService.Execute(GUIOperation.Unload, "popup_TSM_Help");
		}

		private void OnOkClicked()
		{
			CleanupListeners();
			closeSignal.Dispatch(null);
		}
	}
}
