using System;
using System.Collections;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class COPPAAgeGatePanelMediator : Mediator
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("COPPAAgeGatePanelMediator") as IKampaiLogger;

		[Inject]
		public COPPAAgeGatePanelView view { get; set; }

		[Inject]
		public SaveDevicePrefsSignal saveSignal { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public ICoppaService coppaService { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public UserAgeForCOPPAReceivedSignal userAgeForCOPPAReceivedSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject]
		public ShowHUDSignal showHUDSignal { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public SetupPushNotificationsSignal pushNotificationSignal { get; set; }

		[Inject]
		public IDevicePrefsService prefs { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistService { get; set; }

		[Inject]
		public IDefinitionService defService { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrimSignal { get; set; }

		public override void OnRegister()
		{
			view.Init(soundFXSignal, timeService);
			view.AcceptButton.ClickedSignal.AddListener(OnAccept);
			view.TOSButton.ClickedSignal.AddListener(TermsOfServiceClicked);
			view.EULAButton.ClickedSignal.AddListener(EulaClicked);
			view.PrivacyButton.ClickedSignal.AddListener(PrivacyPolicyClicked);
			view.DeclineButton.ClickedSignal.AddListener(OnDecline);
			StartCoroutine(WaitAFrame());
		}

		public override void OnRemove()
		{
			view.AcceptButton.ClickedSignal.RemoveListener(OnAccept);
			view.TOSButton.ClickedSignal.RemoveListener(TermsOfServiceClicked);
			view.EULAButton.ClickedSignal.RemoveListener(EulaClicked);
			view.PrivacyButton.ClickedSignal.RemoveListener(PrivacyPolicyClicked);
			view.DeclineButton.ClickedSignal.RemoveListener(OnDecline);
			saveSignal.Dispatch();
			DateTime birthdate;
			if (!coppaService.GetBirthdate(out birthdate))
			{
				birthdate = DateTime.Now;
			}
			telemetryService.Send_Telemetry_EVT_AGE_GATE_SET(birthdate.Year, birthdate.Month);
			telemetryService.COPPACompliance();
			showHUDSignal.Dispatch(true);
			SetupNotifications();
		}

		private void SetupNotifications()
		{
			pushNotificationSignal.Dispatch();
			if (!localPersistService.HasKeyPlayer("InitialSettings"))
			{
				localPersistService.PutDataPlayer("InitialSettings", "true");
				prefs.GetDevicePrefs().ConstructionNotif = true;
				prefs.GetDevicePrefs().BlackMarketNotif = true;
				prefs.GetDevicePrefs().MinionsParadiseNotif = true;
				prefs.GetDevicePrefs().BaseResourceNotif = true;
				prefs.GetDevicePrefs().CraftingNotif = true;
				prefs.GetDevicePrefs().EventNotif = true;
				prefs.GetDevicePrefs().MarketPlaceNotif = true;
				prefs.GetDevicePrefs().SocialEventNotif = true;
			}
		}

		private void OnAccept()
		{
			DateTime now = DateTime.Now;
			int num = now.Year - (int)view.AgeSlider.value;
			DateTime userBirthdate = new DateTime(num, now.Month, 1);
			coppaService.SetUserBirthdate(userBirthdate);
			UnloadUI();
			userAgeForCOPPAReceivedSignal.Dispatch(new Tuple<int, int>(num, now.Month));
		}

		private void OnDecline()
		{
			DateTime now = DateTime.Now;
			DateTime userBirthdate = new DateTime(now.Year, now.Month, 1);
			coppaService.SetUserBirthdate(userBirthdate);
			userAgeForCOPPAReceivedSignal.Dispatch(new Tuple<int, int>(now.Year, now.Month));
			UnloadUI();
		}

		private void UnloadUI()
		{
			hideSkrimSignal.Dispatch("CoppaAgeGate");
			guiService.Execute(GUIOperation.Unload, "COPPA_Age_Gate_Panel");
		}

		private IEnumerator WaitAFrame()
		{
			yield return null;
			showHUDSignal.Dispatch(false);
		}

		private void TermsOfServiceClicked()
		{
			LegalDocuments.TermsOfServiceClicked(localService, soundFXSignal, logger, defService);
		}

		private void PrivacyPolicyClicked()
		{
			LegalDocuments.PrivacyPolicyClicked(localService, soundFXSignal, logger, defService);
		}

		private void EulaClicked()
		{
			LegalDocuments.EulaClicked(localService, soundFXSignal, logger, defService);
		}
	}
}
