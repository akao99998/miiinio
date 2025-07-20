using System;
using System.Collections;
using Kampai.Common;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class HUDMediator : EventMediator
	{
		private MutableBoxed<bool> currentPeekToken;

		private bool characterLoopIsOn;

		[Inject]
		public HUDView view { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public CloseHUDSignal closeSignal { get; set; }

		[Inject]
		public CloseAllOtherMenuSignal closeAllOtherMenusSignal { get; set; }

		[Inject]
		public SetGrindCurrencySignal setGrindCurrencySignal { get; set; }

		[Inject]
		public SetPremiumCurrencySignal setPremiumCurrencySignal { get; set; }

		[Inject]
		public SetStorageCapacitySignal setStorageSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		[Inject]
		public ShowMTXStoreSignal showMTXStoreSignal { get; set; }

		[Inject]
		public ShowHUDSignal showHUDSignal { get; set; }

		[Inject]
		public ShowSettingsButtonSignal showSettingsButtonSignal { get; set; }

		[Inject]
		public ShowPetsButtonSignal showPetsButtonSignal { get; set; }

		[Inject]
		public ShowPetsXPromoSignal showPetsXPromoSignal { get; set; }

		[Inject]
		public PeekHUDSignal peekHUDSignal { get; set; }

		[Inject]
		public TogglePopupForHUDSignal togglePopupSignal { get; set; }

		[Inject]
		public SetHUDButtonsVisibleSignal setHUDButtonsVisibleSignal { get; set; }

		[Inject]
		public ICurrencyService currencyService { get; set; }

		[Inject]
		public ICurrencyStoreService currencyStoreService { get; set; }

		[Inject]
		public UIAddedSignal uiAddedSignal { get; set; }

		[Inject]
		public UIRemovedSignal uiRemovedSignal { get; set; }

		[Inject]
		public XPFTUEHighlightSignal ftueHighlightSignal { get; set; }

		[Inject]
		public ShowAllWayFindersSignal showAllWayFindersSignal { get; set; }

		[Inject]
		public HideAllWayFindersSignal hideAllWayFindersSignal { get; set; }

		[Inject]
		public ShowAllResourceIconsSignal showAllResourceIconsSignal { get; set; }

		[Inject]
		public HideAllResourceIconsSignal hideAllResourceIconsSignal { get; set; }

		[Inject]
		public ToggleAllFloatingTextSignal toggleAllFloatingTextSignal { get; set; }

		[Inject]
		public HUDChangedSiblingIndexSignal hudChangingSiblingIndexSignal { get; set; }

		[Inject]
		public FirePremiumVFXSignal firePremiumSignal { get; set; }

		[Inject]
		public FireGrindVFXSignal fireGrindSignal { get; set; }

		[Inject]
		public HideDelayHUDSignal hideDelayHUDSignal { get; set; }

		[Inject]
		public IQuestScriptService questScriptService { get; set; }

		[Inject]
		public CurrencyDialogClosedSignal currencyDialogClosedSignal { get; set; }

		[Inject]
		public OpenStorageBuildingSignal OpenStorageBuildingSignal { get; set; }

		[Inject]
		public RushDialogPurchaseHelper rushDialogPurchaseHelper { get; set; }

		[Inject]
		public SetStorageMenuEnabledSignal setStorageMenuEnabledSignal { get; set; }

		[Inject]
		public UpdateSaleBadgeSignal updateSaleBadgeSignal { get; set; }

		[Inject]
		public QuestDialogDismissedSignal questDialogDismissed { get; set; }

		[Inject]
		public ShowOfflinePopupSignal showOfflinePopupSignal { get; set; }

		[Inject]
		public PauseSoundSignal pauseSoundSignal { get; set; }

		[Inject]
		public BeginCharacterLoopAnimationSignal beginCharacterLoop { get; set; }

		[Inject]
		public CharacterIntroCompleteSignal endCharacterLoop { get; set; }

		[Inject]
		public EnableVillainLairHudSignal enableVillainHudSignal { get; set; }

		[Inject]
		public ExitVillainLairSignal exitVillainLairSignal { get; set; }

		[Inject]
		public DisplayCurrencyStoreSignal displayCurrencyStoreSignal { get; set; }

		[Inject]
		public SetBuildMenuEnabledSignal setBuildMenuEnabledSignal { get; set; }

		[Inject]
		public VillainLairModel villainLairModel { get; set; }

		[Inject]
		public DisplayDisco3DElements displayDisco3DElements { get; set; }

		[Inject]
		public UIModel uiModel { get; set; }

		[Inject]
		public PickControllerModel pickControllerModel { get; set; }

		[Inject]
		public AwardLevelSignal awardLevelSignal { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		public override void OnRegister()
		{
			view.Init(hudChangingSiblingIndexSignal);
			setStorageSignal.AddListener(SetStorage);
			closeAllOtherMenusSignal.AddListener(CloseAllMenu);
			closeSignal.AddListener(CloseMenu);
			view.MenuMoved.AddListener(MenuMoved);
			view.StorageButton.ClickedSignal.AddListener(OpenStorage);
			view.StorageExpandButton.ClickedSignal.AddListener(ExpandStorage);
			view.PetsButton.ClickedSignal.AddListener(ShowPetsXPromo);
			showHUDSignal.AddListener(ToggleHUD);
			showSettingsButtonSignal.AddListener(ToggleSettings);
			peekHUDSignal.AddListener(PeekHUD);
			hideDelayHUDSignal.AddListener(SetupHide);
			setHUDButtonsVisibleSignal.AddListener(SetButtonsVisible);
			togglePopupSignal.AddListener(TogglePopup);
			ftueHighlightSignal.AddListener(ShowFTUEXP);
			firePremiumSignal.AddListener(PlayPremiumVFX);
			fireGrindSignal.AddListener(PlayGrindVFX);
			rushDialogPurchaseHelper.actionSuccessfulSignal.AddListener(OnExpandStorageSuccess);
			currencyDialogClosedSignal.AddListener(OnCurrencyDone);
			setStorageMenuEnabledSignal.AddListener(SetStorageButtonVisible);
			updateSaleBadgeSignal.AddListener(UpdateSaleBadgeCount);
			questDialogDismissed.AddListener(QuestDialogDismissed);
			showOfflinePopupSignal.AddListener(OnPauseSound);
			beginCharacterLoop.AddListener(OnCharacterLoopBegin);
			endCharacterLoop.AddListener(OnCharacterLoopEnd);
			awardLevelSignal.AddListener(OnLevelUp);
			enableVillainHudSignal.AddListener(OnEnableVillainHud);
			view.ExitLairButton.ClickedSignal.AddListener(OnExitLairButtonClicked);
			showPetsButtonSignal.AddListener(HandlePetsButtonRequest);
			RegisterCurrencySignals();
			CheckStorageExpansionLimitReached();
			UpdateSaleBadgeCount();
			CheckShowPetsXPromo();
		}

		public override void OnRemove()
		{
			setStorageSignal.RemoveListener(SetStorage);
			closeAllOtherMenusSignal.RemoveListener(CloseAllMenu);
			closeSignal.RemoveListener(CloseMenu);
			view.MenuMoved.RemoveListener(MenuMoved);
			view.StorageButton.ClickedSignal.RemoveListener(OpenStorage);
			view.StorageExpandButton.ClickedSignal.RemoveListener(ExpandStorage);
			rushDialogPurchaseHelper.actionSuccessfulSignal.RemoveListener(OnExpandStorageSuccess);
			showHUDSignal.RemoveListener(ToggleHUD);
			showSettingsButtonSignal.RemoveListener(ToggleSettings);
			peekHUDSignal.RemoveListener(PeekHUD);
			hideDelayHUDSignal.RemoveListener(SetupHide);
			setHUDButtonsVisibleSignal.RemoveListener(SetButtonsVisible);
			ftueHighlightSignal.RemoveListener(ShowFTUEXP);
			togglePopupSignal.RemoveListener(TogglePopup);
			firePremiumSignal.RemoveListener(PlayPremiumVFX);
			fireGrindSignal.RemoveListener(PlayGrindVFX);
			currencyDialogClosedSignal.RemoveListener(OnCurrencyDone);
			updateSaleBadgeSignal.RemoveListener(UpdateSaleBadgeCount);
			questDialogDismissed.RemoveListener(QuestDialogDismissed);
			showOfflinePopupSignal.RemoveListener(OnPauseSound);
			beginCharacterLoop.RemoveListener(OnCharacterLoopBegin);
			endCharacterLoop.RemoveListener(OnCharacterLoopEnd);
			RemoveVillainLairSignals();
			RemoveCurrencySignals();
			rushDialogPurchaseHelper.Cleanup();
		}

		private void OnPauseSound(bool isPause)
		{
			pauseSoundSignal.Dispatch(isPause);
		}

		private void RegisterCurrencySignals()
		{
			setGrindCurrencySignal.AddListener(SetGrindCurrency);
			setPremiumCurrencySignal.AddListener(SetPremiumCurrency);
			showMTXStoreSignal.AddListener(ShowStore);
			view.PremiumMenuButton.ClickedSignal.AddListener(OnPremiumButtonClicked);
			view.PremiumIconButton.ClickedSignal.AddListener(OnPremiumButtonClicked);
			view.PremiumTextButton.ClickedSignal.AddListener(OnPremiumButtonClicked);
			view.GrindMenuButton.ClickedSignal.AddListener(OnGrindButtonClicked);
			view.GrindIconButton.ClickedSignal.AddListener(OnGrindButtonClicked);
			view.GrindTextButton.ClickedSignal.AddListener(OnGrindButtonClicked);
			view.StoreMenuButton.ClickedSignal.AddListener(OnStoreButtonClicked);
			view.BackgroundButton.ClickedSignal.AddListener(CloseMenuAndCurrency);
		}

		private void RemoveCurrencySignals()
		{
			setGrindCurrencySignal.RemoveListener(SetGrindCurrency);
			setPremiumCurrencySignal.RemoveListener(SetPremiumCurrency);
			showMTXStoreSignal.RemoveListener(ShowStore);
			view.PremiumMenuButton.ClickedSignal.RemoveListener(OnPremiumButtonClicked);
			view.PremiumIconButton.ClickedSignal.RemoveListener(OnPremiumButtonClicked);
			view.PremiumTextButton.ClickedSignal.RemoveListener(OnPremiumButtonClicked);
			view.GrindMenuButton.ClickedSignal.RemoveListener(OnGrindButtonClicked);
			view.GrindIconButton.ClickedSignal.RemoveListener(OnGrindButtonClicked);
			view.GrindTextButton.ClickedSignal.RemoveListener(OnGrindButtonClicked);
			view.StoreMenuButton.ClickedSignal.RemoveListener(OnStoreButtonClicked);
			view.BackgroundButton.ClickedSignal.RemoveListener(CloseMenuAndCurrency);
		}

		private void RemoveVillainLairSignals()
		{
			enableVillainHudSignal.RemoveListener(OnEnableVillainHud);
			view.ExitLairButton.ClickedSignal.RemoveListener(OnExitLairButtonClicked);
		}

		private void DisplayAllWorldToGlassUI(bool display)
		{
			toggleAllFloatingTextSignal.Dispatch(display);
			if (display)
			{
				if (!characterLoopIsOn && villainLairModel.currentActiveLair == null)
				{
					showAllWayFindersSignal.Dispatch();
				}
				showAllResourceIconsSignal.Dispatch();
			}
			else
			{
				if (!characterLoopIsOn && villainLairModel.currentActiveLair == null)
				{
					hideAllWayFindersSignal.Dispatch();
				}
				hideAllResourceIconsSignal.Dispatch();
			}
		}

		private void CheckStorageExpansionLimitReached()
		{
			StorageBuilding byInstanceId = playerService.GetByInstanceId<StorageBuilding>(314);
			if (byInstanceId.CurrentStorageBuildingLevel == byInstanceId.Definition.StorageUpgrades.Count - 1)
			{
				view.StorageExpandButton.gameObject.SetActive(false);
			}
		}

		private void OnExpandStorageSuccess()
		{
			playSFXSignal.Dispatch("Play_expand_storage_01");
			SetStorage();
			CheckStorageExpansionLimitReached();
		}

		private void ExpandStorage()
		{
			int num = 314;
			StorageBuilding byInstanceId = playerService.GetByInstanceId<StorageBuilding>(num);
			if (byInstanceId.State != BuildingState.Broken && byInstanceId.State != BuildingState.Inaccessible)
			{
				int transactionId = byInstanceId.Definition.StorageUpgrades[byInstanceId.CurrentStorageBuildingLevel].TransactionId;
				rushDialogPurchaseHelper.Init(transactionId, TransactionTarget.STORAGEBUILDING, new TransactionArg(num));
				rushDialogPurchaseHelper.TryAction(true);
			}
		}

		private void HandlePetsButtonRequest(bool enable)
		{
			if (enable)
			{
				CheckShowPetsXPromo();
			}
			else
			{
				TogglePetsButton(false);
			}
		}

		private void ShowPetsXPromo()
		{
			playSFXSignal.Dispatch("Play_menu_popUp_01");
			telemetryService.Send_Telemetry_EVT_GAME_BUTTON_PRESSED_GENERIC(GameConstants.TrackedGameButton.PetsXPromo_HUD, string.Empty);
			showPetsXPromoSignal.Dispatch();
		}

		private void OpenStorage()
		{
			if (!ButtonClicked())
			{
				return;
			}
			MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
			if (minionPartyInstance == null || (!minionPartyInstance.CharacterUnlocking && !minionPartyInstance.IsPartyHappening))
			{
				StorageBuilding byInstanceId = playerService.GetByInstanceId<StorageBuilding>(314);
				if (byInstanceId.State != BuildingState.Broken && byInstanceId.State != BuildingState.Inaccessible)
				{
					OpenStorageBuildingSignal.Dispatch(byInstanceId, true);
				}
			}
		}

		private void ShowStore(Tuple<int, int> categorySettings)
		{
			closeAllOtherMenusSignal.Dispatch(base.gameObject);
			view.MoveMenu(true);
			displayCurrencyStoreSignal.Dispatch(categorySettings);
			view.ActivateBackgroundButton();
			if (playerService.GetHighestFtueCompleted() < 9)
			{
				questScriptService.PauseQuestScripts();
			}
			DisplayAllWorldToGlassUI(false);
			UpdateSaleBadgeCount();
		}

		private void ShowStoreInternal(int storeCategoryDefintiionID, int amountNeeded)
		{
			ShowStore(new Tuple<int, int>(storeCategoryDefintiionID, amountNeeded));
		}

		internal void CloseAllMenu(GameObject exception)
		{
			if (base.gameObject != exception)
			{
				CloseMenu(false);
			}
		}

		private void SetStorage()
		{
			uint currentStorageCapacity = playerService.GetCurrentStorageCapacity();
			uint storageCount = playerService.GetStorageCount();
			view.SetStorage(storageCount, currentStorageCapacity);
			view.PlayStorageVFX();
		}

		internal void SetGrindCurrency()
		{
			view.SetGrindCurrency(playerService.GetQuantity(StaticItem.GRIND_CURRENCY_ID));
		}

		internal void SetPremiumCurrency()
		{
			view.SetPremiumCurrency(playerService.GetQuantity(StaticItem.PREMIUM_CURRENCY_ID));
		}

		internal void CloseMenu(bool closeCurrency)
		{
			if (closeCurrency)
			{
				CloseMenuAndCurrency();
			}
			view.MoveMenu(false);
			UpdateSaleBadgeCount();
		}

		internal void CloseMenuAndCurrency()
		{
			currencyService.CurrencyDialogClosed(false);
			OnCurrencyDone();
		}

		internal void OnPremiumButtonClicked()
		{
			if (ButtonClicked())
			{
				ShowStoreInternal(800002, 0);
			}
		}

		internal void OnGrindButtonClicked()
		{
			if (ButtonClicked())
			{
				ShowStoreInternal(800001, 0);
			}
		}

		internal void OnStoreButtonClicked()
		{
			if (ButtonClicked())
			{
				ShowStoreInternal(800003, 0);
			}
		}

		internal void OnEnableVillainHud(bool isEnabled)
		{
			view.EnableVillainHud(isEnabled);
			setBuildMenuEnabledSignal.Dispatch(!isEnabled);
		}

		internal void OnExitLairButtonClicked()
		{
			if (!pickControllerModel.PanningCameraBlocked)
			{
				exitVillainLairSignal.Dispatch(new Boxed<Action>(OnExitLairComplete));
			}
		}

		private void OnExitLairComplete()
		{
			displayDisco3DElements.Dispatch(true);
		}

		internal void QuestDialogDismissed()
		{
			StopCoroutine(RenableStoreButton());
			view.EnableStoreMenuButton(false);
			StartCoroutine(RenableStoreButton());
		}

		internal IEnumerator RenableStoreButton()
		{
			yield return new WaitForSeconds(0.25f);
			view.EnableStoreMenuButton(true);
		}

		internal void OnCurrencyDone()
		{
			if (playerService.GetHighestFtueCompleted() < 9)
			{
				questScriptService.ResumeQuestScripts();
			}
			DisplayAllWorldToGlassUI(true);
			if (view.isInForeground)
			{
				view.MoveMenu(false);
			}
		}

		internal void UpdateSaleBadgeCount()
		{
			int num = 0;
			foreach (CurrencyStoreCategoryDefinition currencyStoreCategoryDefinition in definitionService.GetCurrencyStoreCategoryDefinitions())
			{
				num += currencyStoreService.GetBadgeCount(currencyStoreCategoryDefinition);
			}
			bool flag = num != 0;
			if (flag)
			{
				view.SaleCount.text = num.ToString();
			}
			view.SaleBadge.SetActive(flag);
		}

		internal void CheckShowPetsXPromo()
		{
			PetsXPromoDefinition petsXPromoDefinition = definitionService.Get<PetsXPromoDefinition>(95000);
			bool flag = petsXPromoDefinition != null && petsXPromoDefinition.PetsXPromoEnabled;
			if (flag)
			{
				int quantity = (int)playerService.GetQuantity(StaticItem.LEVEL_ID);
				if (quantity < petsXPromoDefinition.PetsXPromoSurfaceLevel)
				{
					flag = false;
				}
			}
			TogglePetsButton(flag);
		}

		internal bool ButtonClicked()
		{
			MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
			if (villainLairModel.currentActiveLair != null)
			{
				return true;
			}
			if (view.IsHiding() || minionPartyInstance.IsPartyHappening || uiModel.LevelUpUIOpen)
			{
				return false;
			}
			return true;
		}

		internal void MenuMoved(bool show)
		{
			if (show)
			{
				uiAddedSignal.Dispatch(base.gameObject, delegate
				{
					CloseAllMenu(null);
				});
				playSFXSignal.Dispatch("Play_main_menu_open_01");
			}
			else
			{
				uiRemovedSignal.Dispatch(base.gameObject);
				playSFXSignal.Dispatch("Play_main_menu_close_01");
			}
		}

		internal void TogglePopup(bool enable)
		{
			DisplayAllWorldToGlassUI(!enable);
			view.TogglePopup(enable);
		}

		internal void ToggleHUD(bool enable)
		{
			CancelPeek();
			view.Toggle(enable);
			if (enable)
			{
				currencyService.ResumeTransactionsHandling();
			}
		}

		internal void ToggleSettings(bool enable)
		{
			view.ToggleSettings(enable);
		}

		internal void TogglePetsButton(bool enable)
		{
			view.TogglePetsButton(enable);
		}

		private void OnLevelUp(TransactionDefinition td)
		{
			CheckShowPetsXPromo();
		}

		private void CancelPeek()
		{
			if (currentPeekToken != null)
			{
				currentPeekToken.Set(false);
				currentPeekToken = null;
			}
		}

		internal void PeekHUD(float seconds)
		{
			if (view.IsHiding())
			{
				CancelPeek();
				view.Toggle(true);
				SetupHide(seconds);
			}
		}

		internal void SetupHide(float seconds)
		{
			if (!view.IsHiding())
			{
				currentPeekToken = new MutableBoxed<bool>(true);
				StartCoroutine(HideAfterSeconds(seconds, currentPeekToken));
			}
		}

		private IEnumerator HideAfterSeconds(float seconds, Boxed<bool> shouldStillHide)
		{
			yield return new WaitForSeconds(seconds);
			if (shouldStillHide.Value)
			{
				view.Toggle(false);
				currentPeekToken = null;
			}
		}

		private void SetStorageButtonVisible(bool visible)
		{
			view.SetStorageButtonVisible(visible);
		}

		private void SetButtonsVisible(bool visible)
		{
			view.SetButtonsVisible(visible);
		}

		internal void ShowFTUEXP(bool show)
		{
			view.ToggleDarkSkrim(false);
		}

		private void PlayPremiumVFX()
		{
			view.PlayPremiumVFX();
		}

		private void PlayGrindVFX()
		{
			view.PlayGrindVFX();
		}

		private void OnCharacterLoopBegin(CharacterObject co)
		{
			characterLoopIsOn = true;
		}

		private void OnCharacterLoopEnd(CharacterObject co, int routeIndex)
		{
			characterLoopIsOn = false;
		}
	}
}
