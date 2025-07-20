using Kampai.Game;
using Kampai.Main;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.UI.View
{
	public class GuestOfHonorSelectionMediator : UIStackMediator<GuestOfHonorSelectionView>
	{
		private float scrollPosition;

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrim { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IGuestOfHonorService guestOfHonorService { get; set; }

		[Inject]
		public SetPremiumCurrencySignal setPremiumCurrencySignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal globalSFXSignal { get; set; }

		[Inject]
		public GOHCardClickedSignal gohCardClickedSignal { get; set; }

		[Inject]
		public ShowMinionPartySkipButtonSignal showSkipButtonSignal { get; set; }

		[Inject]
		public MoveAudioListenerSignal toggleCharacterAudioSignal { get; set; }

		[Inject]
		public CancelPurchaseSignal cancelPurchaseSignal { get; set; }

		public override void OnRegister()
		{
			Input.multiTouchEnabled = false;
			base.view.OnMenuClose.AddListener(OnMenuClose);
			base.view.rushGOHCooldown_Callback.AddListener(RushGoHCooldown);
			gohCardClickedSignal.AddListener(CharacterClicked);
			cancelPurchaseSignal.AddListener(CurrencyClosed);
		}

		public override void OnRemove()
		{
			Input.multiTouchEnabled = true;
			toggleCharacterAudioSignal.Dispatch(true, null);
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
			base.view.rushGOHCooldown_Callback.RemoveListener(RushGoHCooldown);
			gohCardClickedSignal.RemoveListener(CharacterClicked);
			cancelPurchaseSignal.RemoveListener(CurrencyClosed);
		}

		private void CharacterClicked(int cardIndex, bool avail)
		{
			soundFXSignal.Dispatch("Play_button_click_01");
			if (base.view.currentCharacterIndex != cardIndex)
			{
				base.view.currentCharacterIndex = cardIndex;
				base.view.SetStartButtonUnlocked(avail);
			}
		}

		public override void Initialize(GUIArguments args)
		{
			Init();
			soundFXSignal.Dispatch("Play_main_menu_open_01");
		}

		private void Init()
		{
			base.closeAllOtherMenuSignal.Dispatch(base.gameObject);
			base.view.Init(prestigeService, definitionService, playerService, guestOfHonorService);
			base.view.startButton.onClick.AddListener(Proceed);
		}

		private void Proceed()
		{
			soundFXSignal.Dispatch("Play_main_menu_close_01");
			Input.multiTouchEnabled = true;
			base.view.Close();
			StartMinionParty();
		}

		private void StartMinionParty()
		{
			guestOfHonorService.SelectGuestOfHonor(base.view.GetCharacterPrestigeDefID(base.view.currentCharacterIndex));
			gameContext.injectionBinder.GetInstance<StartMinionPartyIntroSignal>().Dispatch();
		}

		private void OnMenuClose()
		{
			guiService.Execute(GUIOperation.Unload, "screen_GuestOfHonorSelection");
			hideSkrim.Dispatch("StartPartySkirm");
			showSkipButtonSignal.Dispatch(true);
		}

		protected override void OnCloseAllMenu(GameObject exception)
		{
		}

		protected override void Close()
		{
		}

		private void RushGoHCooldown()
		{
			Prestige prestige = prestigeService.GetPrestige(base.view.GetCharacterPrestigeDefID(base.view.currentCharacterIndex), false);
			if (playerService.GetQuantity(StaticItem.PREMIUM_CURRENCY_ID) < guestOfHonorService.GetRushCostForPartyCoolDown(prestige.ID))
			{
				scrollPosition = base.view.GetHorizontalScrollPosition();
				base.gameObject.SetActive(false);
				base.view.Hide();
			}
			playerService.ProcessRush(guestOfHonorService.GetRushCostForPartyCoolDown(prestige.ID), true, RushTransactionCallback, prestige.ID);
		}

		private void RushTransactionCallback(PendingCurrencyTransaction pct)
		{
			if (pct.Success)
			{
				globalSFXSignal.Dispatch("Play_button_premium_01");
				Prestige prestige = prestigeService.GetPrestige(base.view.GetCharacterPrestigeDefID(base.view.currentCharacterIndex), false);
				guestOfHonorService.RushPartyCooldownForPrestige(prestige.ID);
				base.view.RushCurrentCharacterCooldown();
				setPremiumCurrencySignal.Dispatch();
			}
			CurrencyClosed(false);
		}

		private void CurrencyClosed(bool value)
		{
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(true);
				Init();
				base.view.SetHorizontalScrollPosition(scrollPosition);
			}
		}
	}
}
