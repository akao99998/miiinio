using Kampai.Common;
using Kampai.Game;
using strange.extensions.context.api;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class DCNConfirmationMediator : UIStackMediator<DCNConfirmationView>
	{
		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrim { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistanceService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public UIModel model { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			base.view.SetupSignals();
			base.view.OnMenuClose.AddListener(OnMenuClose);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.view.RemoveSignals();
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
		}

		public override void Initialize(GUIArguments args)
		{
			if (!model.StageUIOpen)
			{
				base.closeAllOtherMenuSignal.Dispatch(base.view.gameObject);
				base.view.Init(args.Get<Signal<bool>>(), localPersistanceService);
			}
		}

		private void OnMenuClose()
		{
			SendTelemetry();
			guiService.Execute(GUIOperation.Unload, "popup_DCNconfirmation");
			hideSkrim.Dispatch("ConfirmationSkrim");
		}

		private void SendTelemetry()
		{
			DCNService dCNService = gameContext.injectionBinder.GetInstance<IDCNService>() as DCNService;
			string launchURL = dCNService.GetLaunchURL();
			telemetryService.Send_Telemetry_EVT_DCN((!base.view.opened) ? "No" : "Yes", launchURL, dCNService.GetFeaturedContentId().ToString());
		}

		protected override void Close()
		{
			base.view.Close();
		}
	}
}
