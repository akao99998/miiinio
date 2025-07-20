using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine.UI;
using strange.extensions.context.api;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class HelpPanelMediator : Mediator
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("HelpPanelMediator") as IKampaiLogger;

		[Inject]
		public HelpPanelView view { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject]
		public BuildingChangeStateSignal buildingChangeStateSignal { get; set; }

		[Inject]
		public CloseAllOtherMenuSignal closeSignal { get; set; }

		[Inject(MainElement.CONTEXT)]
		public ICrossContextCapable mainContext { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public IConfigurationsService configurationsService { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		public override void OnRegister()
		{
			view.onlineHelp.ClickedSignal.AddListener(OnlineHelpClicked);
			view.restorePurchases.ClickedSignal.AddListener(OnRestorePurchases);
			Init();
		}

		public override void OnRemove()
		{
			view.onlineHelp.ClickedSignal.RemoveListener(OnlineHelpClicked);
			view.restorePurchases.ClickedSignal.RemoveListener(OnRestorePurchases);
		}

		private void Init()
		{
			view.helpIdText.text = string.Format("{0}{1}", localizationService.GetString("playerid"), playerService.ID);
			view.onlineHelpText.text = localizationService.GetString("ContactUs");
			view.learnSystemsText.text = localizationService.GetString("LearnSystems");
			view.selectTopicText.text = localizationService.GetString("SelectTopic");
			view.restorePurchaseText.text = localizationService.GetString("RestorePurchases");
			view.restorePurchases.gameObject.SetActive(false);
		}

		private void OnBuildingStateChange(int buildingId, BuildingState state)
		{
			if (buildingId == 313 && state == BuildingState.Idle)
			{
				view.restorePurchases.GetComponent<Button>().interactable = true;
				buildingChangeStateSignal.RemoveListener(OnBuildingStateChange);
			}
		}

		private void OnlineHelpClicked()
		{
			telemetryService.Send_Telemetry_CONTACT_US_CLICKED();
			soundFXSignal.Dispatch("Play_button_click_01");
			mainContext.injectionBinder.GetInstance<OpenHelpSignal>().Dispatch(HelpType.ONLINE_HELP);
		}

		private void OnRestorePurchases()
		{
			mainContext.injectionBinder.GetInstance<RestoreMtxPurchaseSignal>().Dispatch();
			closeSignal.Dispatch(null);
		}

		private void OnEnable()
		{
			if (view != null)
			{
				Start();
			}
		}

		private void Start()
		{
			logger.Info("wwce killswitch :{0}", configurationsService.isKillSwitchOn(KillSwitch.WWCE));
			view.onlineHelp.gameObject.SetActive(!configurationsService.isKillSwitchOn(KillSwitch.WWCE));
		}
	}
}
