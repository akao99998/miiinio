using Kampai.Common;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;

namespace Kampai.UI.View
{
	public class PetsPopupMediator : UIStackMediator<PetsPopupView>
	{
		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrim { get; set; }

		[Inject]
		public OnClickSkrimSignal onClickSkrimSignal { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		public override void Initialize(GUIArguments args)
		{
			base.view.Init();
		}

		public override void OnRegister()
		{
			base.view.PlayNowButton.ClickedSignal.AddListener(PlayNowClicked);
			base.view.OnMenuClose.AddListener(OnMenuClose);
			onClickSkrimSignal.AddListener(SkrimClose);
			base.OnRegister();
		}

		public override void OnRemove()
		{
			base.view.PlayNowButton.ClickedSignal.RemoveListener(PlayNowClicked);
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
			onClickSkrimSignal.RemoveListener(SkrimClose);
			base.OnRemove();
		}

		protected override void Close()
		{
			playSFXSignal.Dispatch("Play_menu_disappear_01");
			base.view.Close();
		}

		private void SkrimClose()
		{
			telemetryService.Send_Telemetry_EVT_GAME_BUTTON_PRESSED_GENERIC(GameConstants.TrackedGameButton.PetsXPromo_Dismiss, string.Empty);
		}

		private void PlayNowClicked()
		{
			PetsXPromoDefinition petsXPromoDefinition = definitionService.Get<PetsXPromoDefinition>(95000);
			if (petsXPromoDefinition != null)
			{
				if (Native.IsAppInstalled(petsXPromoDefinition.AndroidInstallURL))
				{
					telemetryService.Send_Telemetry_EVT_GAME_XPROMO_BUTTON_PRESSED(GameConstants.TrackedGameButton.PetsXPromo_PlayNow, true);
					Native.LaunchApp(petsXPromoDefinition.AndroidInstallURL);
				}
				else
				{
					telemetryService.Send_Telemetry_EVT_GAME_XPROMO_BUTTON_PRESSED(GameConstants.TrackedGameButton.PetsXPromo_PlayNow, false);
					Native.OpenURL(petsXPromoDefinition.AndroidPetsSmartURL);
				}
				Close();
			}
		}

		private void OnMenuClose()
		{
			guiService.Execute(GUIOperation.Unload, "popup_Pets");
			hideSkrim.Dispatch("PetsXPromo");
		}
	}
}
