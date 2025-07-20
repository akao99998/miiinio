using System.Collections;
using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.View;
using Kampai.Main;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.UI.View
{
	public class LevelUpRewardMediator : UIStackMediator<LevelUpRewardView>
	{
		private StartMinionPartySignal startMinionPartySignal;

		private List<RewardQuantity> quantityChange;

		private bool isInspiration;

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public IFancyUIService fancyUIService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSoundFX { get; set; }

		[Inject]
		public FTUELevelUpOpenSignal FTUEOpenSignal { get; set; }

		[Inject]
		public FTUELevelUpCloseSignal FTUECloseSignal { get; set; }

		[Inject]
		public UnlockMinionsSignal unlockMinionsSignal { get; set; }

		[Inject]
		public UpdatePlayerDLCTierSignal playerDLCTierSignal { get; set; }

		[Inject]
		public HideAllWayFindersSignal hideAllWayFindersSignal { get; set; }

		[Inject]
		public DisplayCameraControlsSignal displayCameraControlsSignal { get; set; }

		[Inject]
		public UIModel uiModel { get; set; }

		[Inject]
		public LevelUpBackButtonSignal levelUpBackButtonSignal { get; set; }

		[Inject]
		public RevealLevelUpUISignal revealLevelUpSignal { get; set; }

		[Inject]
		public IGuestOfHonorService guestService { get; set; }

		[Inject]
		public ShowAllWayFindersSignal showAllWayFindersSignal { get; set; }

		[Inject]
		public HideItemPopupSignal hideItemPopupSignal { get; set; }

		[Inject]
		public UpdateAdHUDSignal updateAdHUDSignal { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			startMinionPartySignal = gameContext.injectionBinder.GetInstance<StartMinionPartySignal>();
			startMinionPartySignal.AddListener(DisplayView);
			base.view.closeSignal.AddListener(Close);
			base.view.beginUnlockSignal.AddListener(BeginUnlock);
			base.view.closeBuffInfoSignal.AddListener(CloseBuffInfo);
			base.view.skrimButton.ClickedSignal.AddListener(SkrimClicked);
			base.view.skipButton.ClickedSignal.AddListener(SkrimClicked);
			levelUpBackButtonSignal.AddListener(SkrimClicked);
			revealLevelUpSignal.AddListener(RevealUI);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.view.closeSignal.RemoveListener(Close);
			base.view.beginUnlockSignal.RemoveListener(BeginUnlock);
			base.view.closeBuffInfoSignal.RemoveListener(CloseBuffInfo);
			base.view.skrimButton.ClickedSignal.RemoveListener(SkrimClicked);
			base.view.skipButton.ClickedSignal.RemoveListener(SkrimClicked);
			levelUpBackButtonSignal.RemoveListener(SkrimClicked);
			startMinionPartySignal.RemoveListener(DisplayView);
			revealLevelUpSignal.RemoveListener(RevealUI);
			uiModel.LevelUpUIOpen = false;
		}

		public override void Initialize(GUIArguments args)
		{
			quantityChange = args.Get<List<RewardQuantity>>();
			isInspiration = args.Get<bool>();
			base.view.Init(playerService, definitionService, localService, fancyUIService, playSoundFX, quantityChange, guestService);
		}

		private void DisplayView()
		{
			gameContext.injectionBinder.GetInstance<BuildingZoomSignal>().Dispatch(new BuildingZoomSettings(ZoomType.OUT, BuildingZoomType.TIKIBAR));
			hideAllWayFindersSignal.Dispatch();
			RevealUI();
		}

		private void RevealUI()
		{
			base.closeAllOtherMenuSignal.Dispatch(base.view.gameObject);
			FTUEOpenSignal.Dispatch();
			uiModel.LevelUpUIOpen = true;
			if (isInspiration)
			{
				base.view.StartAnimation();
			}
			else
			{
				StartCoroutine(WaitThenReleasePicker());
			}
		}

		private IEnumerator WaitThenReleasePicker()
		{
			yield return new WaitForSeconds(base.view.waitForDooberTimer + base.view.openForTimer);
			Close();
		}

		protected override void OnCloseAllMenu(GameObject exception)
		{
		}

		private void SkrimClicked()
		{
			if (base.view.coroutine != null)
			{
				StopCoroutine(base.view.coroutine);
				base.view.StartCoroutine(base.view.CloseDown());
			}
		}

		private void BeginUnlock()
		{
			unlockMinionsSignal.Dispatch();
		}

		private void CloseBuffInfo()
		{
			hideItemPopupSignal.Dispatch();
		}

		protected override void Close()
		{
			base.view.CleanupListeners();
			FTUECloseSignal.Dispatch();
			playerDLCTierSignal.Dispatch();
			if (!playerService.GetMinionPartyInstance().PartyPreSkip)
			{
				displayCameraControlsSignal.Dispatch(true);
				gameContext.injectionBinder.GetInstance<StartPartyFavorAnimationSignal>().Dispatch();
			}
			guiService.Execute(GUIOperation.Unload, "screen_PhilsInspiration");
			MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
			if (!minionPartyInstance.IsPartyHappening)
			{
				showAllWayFindersSignal.Dispatch();
			}
			updateAdHUDSignal.Dispatch();
			TSMCharacter firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<TSMCharacter>(70008);
			if (firstInstanceByDefinitionId != null && !timeEventService.HasEventID(firstInstanceByDefinitionId.ID))
			{
				gameContext.injectionBinder.GetInstance<CheckTriggersSignal>().Dispatch(firstInstanceByDefinitionId.ID);
			}
		}
	}
}
