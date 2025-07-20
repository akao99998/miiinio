using System.Collections;
using Kampai.Common;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class BuildMenuMediator : EventMediator
	{
		private InventoryBuildingMovementSignal toInventorySignal;

		private MutableBoxed<bool> currentPeekToken;

		[Inject]
		public BuildMenuView view { get; set; }

		[Inject]
		public MoveBuildMenuSignal moveSignal { get; set; }

		[Inject]
		public CloseAllOtherMenuSignal closeAllMenuSignal { get; set; }

		[Inject]
		public LoadDefinitionForUISignal loadDefinitionSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public BuildMenuButtonClickedSignal openButtonClickedSignal { get; set; }

		[Inject]
		public ShowStoreSignal showStoreSignal { get; set; }

		[Inject]
		public BuildMenuOpenedSignal buildMenuOpenedSignal { get; set; }

		[Inject]
		public SetNewUnlockForBuildMenuSignal setNewUnlockForBuildMenuSignal { get; set; }

		[Inject]
		public SetInventoryCountForBuildMenuSignal setInventoryCountForBuildMenuSignal { get; set; }

		[Inject]
		public IncreaseInventoryCountForBuildMenuSignal increaseInventoryCountForBuildMenuSignal { get; set; }

		[Inject]
		public HideStoreHighlightSignal hideStoreHighlightSignal { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public IBuildMenuService buildMenuService { get; set; }

		[Inject]
		public UIAddedSignal uiAddedSignal { get; set; }

		[Inject]
		public UIRemovedSignal uiRemovedSignal { get; set; }

		[Inject]
		public SetBuildMenuEnabledSignal setBuildMenuEnabledSignal { get; set; }

		[Inject]
		public StopAutopanSignal stopAutopanSignal { get; set; }

		[Inject]
		public RemoveUnlockForBuildMenuSignal removeUnlockForBuildMenuSignal { get; set; }

		[Inject]
		public CloseAllMessageDialogs closeAllDialogsSignal { get; set; }

		[Inject]
		public ShowMoveBuildingMenuSignal showMoveBuildingMenuSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public DisableBuildMenuButtonSignal disableBuildMenuSignal { get; set; }

		[Inject]
		public UIModel uiModel { get; set; }

		[Inject]
		public VillainLairModel lairModel { get; set; }

		[Inject]
		public EnableBuildMenuFromLairSignal enableBuildMenuFromLairSignal { get; set; }

		[Inject]
		public PeekStoreSignal peekStoreSignal { get; set; }

		[Inject]
		public HideDelayStoreSignal hideDelayStoreSignal { get; set; }

		public override void OnRegister()
		{
			TimeProfiler.StartSection("BuildMenuMediator");
			view.Init();
			loadDefinitionSignal.Dispatch();
			view.MenuButton.ClickedSignal.AddListener(OnMenuButtonClicked);
			closeAllMenuSignal.AddListener(CloseAllMenu);
			moveSignal.AddListener(MoveMenu);
			setBuildMenuEnabledSignal.AddListener(SetBuildMenuEnabled);
			enableBuildMenuFromLairSignal.AddListener(SetBuildMenuEnabledNoCheck);
			showMoveBuildingMenuSignal.AddListener(ShowMoveBuildingMenu);
			toInventorySignal = gameContext.injectionBinder.GetInstance<InventoryBuildingMovementSignal>();
			toInventorySignal.AddListener(ToInventory);
			showStoreSignal.AddListener(ToggleStore);
			setNewUnlockForBuildMenuSignal.AddListener(SetNewUnlock);
			removeUnlockForBuildMenuSignal.AddListener(RemoveUnlock);
			setInventoryCountForBuildMenuSignal.AddListener(SetBadgeCount);
			increaseInventoryCountForBuildMenuSignal.AddListener(IncreaseInventoryCount);
			disableBuildMenuSignal.AddListener(view.DisableBuildButton);
			peekStoreSignal.AddListener(PeekStore);
			hideDelayStoreSignal.AddListener(SetupHide);
			TimeProfiler.EndSection("BuildMenuMediator");
		}

		public override void OnRemove()
		{
			view.MenuButton.ClickedSignal.RemoveListener(OnMenuButtonClicked);
			moveSignal.RemoveListener(MoveMenu);
			toInventorySignal.RemoveListener(ToInventory);
			closeAllMenuSignal.RemoveListener(CloseAllMenu);
			setBuildMenuEnabledSignal.RemoveListener(SetBuildMenuEnabled);
			enableBuildMenuFromLairSignal.RemoveListener(SetBuildMenuEnabledNoCheck);
			showMoveBuildingMenuSignal.RemoveListener(ShowMoveBuildingMenu);
			showStoreSignal.RemoveListener(ToggleStore);
			setNewUnlockForBuildMenuSignal.RemoveListener(SetNewUnlock);
			removeUnlockForBuildMenuSignal.RemoveListener(RemoveUnlock);
			setInventoryCountForBuildMenuSignal.RemoveListener(SetBadgeCount);
			increaseInventoryCountForBuildMenuSignal.RemoveListener(IncreaseInventoryCount);
			disableBuildMenuSignal.RemoveListener(view.DisableBuildButton);
			peekStoreSignal.RemoveListener(PeekStore);
			hideDelayStoreSignal.RemoveListener(SetupHide);
		}

		internal void ToInventory()
		{
			view.IncreaseBadgeCounter();
		}

		internal void RemoveUnlock(int count)
		{
			view.RemoveUnlockBadge(count);
		}

		internal void SetNewUnlock(int count)
		{
			view.SetUnlockBadge(count);
		}

		internal void SetBadgeCount(int count)
		{
			view.SetBadgeCount(count);
		}

		internal void IncreaseInventoryCount()
		{
			view.IncreaseBadgeCounter();
		}

		internal void CloseAllMenu(GameObject exception)
		{
			if (base.gameObject != exception && view.isOpen)
			{
				moveSignal.Dispatch(false);
			}
		}

		internal void OnMenuButtonClicked()
		{
			if (Input.touchCount <= 1)
			{
				telemetryService.Send_Telemetry_EVT_IGE_STORE_VISIT("Menu Button", "Building Menu");
				moveSignal.Dispatch(!view.isOpen);
				if (view.isOpen)
				{
					buildMenuService.SetStoreUnlockChecked();
					closeAllMenuSignal.Dispatch(base.gameObject);
					Input.multiTouchEnabled = true;
				}
				openButtonClickedSignal.Dispatch();
				uiModel.GoToInEffect = false;
			}
		}

		internal void MoveMenu(bool show)
		{
			if (show == view.isOpen)
			{
				return;
			}
			if (show)
			{
				MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
				if (minionPartyInstance != null && minionPartyInstance.CharacterUnlocking)
				{
					return;
				}
				stopAutopanSignal.Dispatch();
			}
			view.MoveMenu(show);
			if (show)
			{
				uiAddedSignal.Dispatch(view.gameObject, OnMenuButtonClicked);
				playSFXSignal.Dispatch("Play_main_menu_open_01");
			}
			else
			{
				closeAllDialogsSignal.Dispatch();
				uiRemovedSignal.Dispatch(view.gameObject);
				hideStoreHighlightSignal.Dispatch();
				playSFXSignal.Dispatch("Play_main_menu_close_01");
			}
			buildMenuOpenedSignal.Dispatch();
		}

		private bool shouldEnableBuildMenu(bool enable)
		{
			return enable && (uiModel.UIState & UIModel.UIStateFlags.StoreButtonHiddenFromQuest) == 0 && lairModel.currentActiveLair == null;
		}

		internal void ToggleStore(bool enable)
		{
			CancelPeek();
			view.Toggle(enable);
			view.DisableBuildButton(!enable);
		}

		private void SetBuildMenuEnabled(bool isEnabled)
		{
			if (lairModel.currentActiveLair == null)
			{
				SetBuildMenuEnabledNoCheck(isEnabled);
			}
		}

		private void SetBuildMenuEnabledNoCheck(bool isEnabled)
		{
			bool buildMenuButtonEnabled = shouldEnableBuildMenu(isEnabled);
			view.SetBuildMenuButtonEnabled(buildMenuButtonEnabled);
		}

		private void ShowMoveBuildingMenu(bool show, MoveBuildingSetting setting)
		{
			bool buildMenuButtonEnabled = shouldEnableBuildMenu(!show);
			view.SetBuildMenuButtonEnabled(buildMenuButtonEnabled);
		}

		private void CancelPeek()
		{
			if (currentPeekToken != null)
			{
				currentPeekToken.Set(false);
				currentPeekToken = null;
			}
		}

		private void PeekStore(float seconds)
		{
			if (view.IsHiding())
			{
				CancelPeek();
				view.Toggle(true);
				SetupHide(seconds);
			}
		}

		private void SetupHide(float seconds)
		{
			if (!view.IsHiding())
			{
				currentPeekToken = new MutableBoxed<bool>(true);
				StartCoroutine(HideAfter(seconds, currentPeekToken));
			}
		}

		private IEnumerator HideAfter(float seconds, Boxed<bool> shouldStillHide)
		{
			yield return new WaitForSeconds(seconds);
			if (shouldStillHide.Value)
			{
				view.Toggle(false);
				currentPeekToken = null;
			}
		}
	}
}
