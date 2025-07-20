using Kampai.Common;
using Kampai.Game;
using Kampai.Main;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class PlayerTrainingMediator : UIStackMediator<PlayerTrainingView>
	{
		private bool openedFromSettingsMenu;

		private float startTime;

		private int triggeredID;

		private Signal<bool> callback;

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public HideSkrimSignal hideSignal { get; set; }

		[Inject]
		public DisplaySettingsMenuSignal displaySettingsMenuSignal { get; set; }

		[Inject]
		public TempHideSettingsMenuSignal tempHideMenuSignal { get; set; }

		[Inject]
		public IFancyUIService fancyUIService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject]
		public EnableCameraBehaviourSignal enableCameraSignal { get; set; }

		[Inject]
		public DisableCameraBehaviourSignal disableCameraSignal { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public TrainingClosedSignal trainingClosedSignal { get; set; }

		[Inject]
		public UnlockMinionsSignal unlockMinionsSignal { get; set; }

		[Inject]
		public UIModel uiModel { get; set; }

		[Inject]
		public VillainLairModel lairModel { get; set; }

		[Inject]
		public MoveAudioListenerSignal toggleCharacterAudioSignal { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			toggleCharacterAudioSignal.Dispatch(false, base.view.minionSlots[1].transform);
			if (lairModel.currentActiveLair == null)
			{
				disableCameraSignal.Dispatch(2);
			}
			base.view.Init(definitionService, fancyUIService);
			base.view.confirmButton.ClickedSignal.AddListener(ButtonClose);
			base.view.completeSignal.AddListener(StartTimer);
			base.view.audioSignal.AddListener(PlayAudio);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			if (lairModel.currentActiveLair == null)
			{
				enableCameraSignal.Dispatch(2);
			}
			base.view.confirmButton.ClickedSignal.RemoveListener(ButtonClose);
			base.view.completeSignal.RemoveListener(StartTimer);
			base.view.audioSignal.RemoveListener(PlayAudio);
		}

		public override void Initialize(GUIArguments args)
		{
			int defID = args.Get<int>();
			openedFromSettingsMenu = args.Get<bool>();
			callback = args.Get<Signal<bool>>();
			uiModel.DisableBack = true;
			uiModel.PopupAnimationIsPlaying = true;
			if (openedFromSettingsMenu)
			{
				tempHideMenuSignal.Dispatch();
				displaySettingsMenuSignal.Dispatch(false);
			}
			base.closeAllOtherMenuSignal.Dispatch(base.view.gameObject);
			ExtractData(defID);
		}

		private void ExtractData(int defID)
		{
			triggeredID = defID;
			PlayerTrainingDefinition playerTrainingDefinition = definitionService.Get<PlayerTrainingDefinition>(defID);
			base.view.SetTitle(localizationService.GetString(playerTrainingDefinition.trainingTitleLocalizedKey));
			PlayerTrainingCardDefinition playerTrainingCardDefinition = definitionService.Get<PlayerTrainingCardDefinition>(playerTrainingDefinition.cardOneDefinitionID);
			PlayerTrainingCardDefinition playerTrainingCardDefinition2 = definitionService.Get<PlayerTrainingCardDefinition>(playerTrainingDefinition.cardTwoDefinitionID);
			PlayerTrainingCardDefinition playerTrainingCardDefinition3 = definitionService.Get<PlayerTrainingCardDefinition>(playerTrainingDefinition.cardThreeDefinitionID);
			base.view.SetCardTitles(localizationService.GetString(playerTrainingCardDefinition.cardTitleLocalizedKey), localizationService.GetString(playerTrainingCardDefinition2.cardTitleLocalizedKey), localizationService.GetString(playerTrainingCardDefinition3.cardTitleLocalizedKey));
			base.view.SetCardDescriptions(localizationService.GetString(playerTrainingCardDefinition.cardDescriptionLocalizedKey), localizationService.GetString(playerTrainingCardDefinition2.cardDescriptionLocalizedKey), localizationService.GetString(playerTrainingCardDefinition3.cardDescriptionLocalizedKey));
			base.view.SetTransitionOne(DetermineTransitionMask((int)playerTrainingDefinition.transitionOne));
			base.view.SetTransitionTwo(DetermineTransitionMask((int)playerTrainingDefinition.transitionTwo));
			SetupCardImages(playerTrainingCardDefinition, playerTrainingCardDefinition2, playerTrainingCardDefinition3);
			base.view.animator.Play("Open");
		}

		private void SetupCardImages(PlayerTrainingCardDefinition cardOne, PlayerTrainingCardDefinition cardTwo, PlayerTrainingCardDefinition cardThree)
		{
			if (cardOne.prestigeDefinitionID == 0 && cardOne.buildingDefinitionID == 0)
			{
				base.view.SetCardOneImages(cardOne.cardImages);
			}
			if (cardTwo.prestigeDefinitionID == 0 && cardTwo.buildingDefinitionID == 0)
			{
				base.view.SetCardTwoImages(cardTwo.cardImages);
			}
			if (cardThree.prestigeDefinitionID == 0 && cardThree.buildingDefinitionID == 0)
			{
				base.view.SetCardThreeImages(cardThree.cardImages);
			}
			base.view.prestigeDefinitionIDs.Add(cardOne.prestigeDefinitionID);
			base.view.prestigeDefinitionIDs.Add(cardTwo.prestigeDefinitionID);
			base.view.prestigeDefinitionIDs.Add(cardThree.prestigeDefinitionID);
			base.view.buildingDefinitionIDs.Add(cardOne.buildingDefinitionID);
			base.view.buildingDefinitionIDs.Add(cardTwo.buildingDefinitionID);
			base.view.buildingDefinitionIDs.Add(cardThree.buildingDefinitionID);
		}

		private string DetermineTransitionMask(int type)
		{
			switch (type)
			{
			default:
				return string.Empty;
			case 1:
				return "icn_nav_arrow_transistion_mask";
			case 2:
				return "icn_nav_plus_mask";
			case 3:
				return "icn_nav_equals_mask";
			case 4:
				return "icn_nav_ampersand_mask";
			}
		}

		private void PlayAudio()
		{
			soundFXSignal.Dispatch("Play_training_popUp_01");
		}

		private void StartTimer()
		{
			uiModel.PopupAnimationIsPlaying = false;
			uiModel.DisableBack = false;
			startTime = timeService.CurrentTime();
		}

		private void ButtonClose()
		{
			Close();
		}

		protected override void Close()
		{
			base.view.animator.Play("Close");
			unlockMinionsSignal.Dispatch();
			soundFXSignal.Dispatch("Play_menu_disappear_01");
			if (callback != null)
			{
				callback.Dispatch(true);
			}
		}

		private void FinishClose()
		{
			toggleCharacterAudioSignal.Dispatch(true, null);
			base.view.RemoveCoroutine();
			uiModel.DisableBack = false;
			guiService.Execute(GUIOperation.Unload, "popup_PlayerTraining");
			hideSignal.Dispatch("PlayerTrainingSkrim");
			float num = (float)timeService.CurrentTime() - startTime;
			int fromSettings = (openedFromSettingsMenu ? 1 : 0);
			telemetryService.Send_Telemetry_EVT_PLAYER_TRAINING(triggeredID, fromSettings, (int)num);
			if (openedFromSettingsMenu)
			{
				openedFromSettingsMenu = false;
				displaySettingsMenuSignal.Dispatch(true);
			}
			trainingClosedSignal.Dispatch();
		}
	}
}
