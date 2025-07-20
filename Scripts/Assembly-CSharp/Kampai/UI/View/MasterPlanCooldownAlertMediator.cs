using Kampai.Game;
using Kampai.Main;
using strange.extensions.context.api;

namespace Kampai.UI.View
{
	public class MasterPlanCooldownAlertMediator : UIStackMediator<MasterPlanCooldownAlertView>
	{
		private MasterPlan plan;

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IMasterPlanService masterPlanService { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public IFancyUIService fancyUIService { get; set; }

		[Inject]
		public HideSkrimSignal hideSignal { get; set; }

		[Inject]
		public SetPremiumCurrencySignal setPremiumCurrencySignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playGlobalSFX { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public VillainLairModel lairModel { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			gameContext.injectionBinder.GetInstance<MasterPlanCooldownCompleteSignal>().AddListener(CoolDownComplete);
			base.view.OnMenuClose.AddListener(CloseAnimationComplete);
			base.view.waitButton.ClickedSignal.AddListener(CloseButton);
			base.view.rushButton.ClickedSignal.AddListener(RushCooldown);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			gameContext.injectionBinder.GetInstance<MasterPlanCooldownCompleteSignal>().RemoveListener(CoolDownComplete);
			base.view.OnMenuClose.RemoveListener(CloseAnimationComplete);
			base.view.waitButton.ClickedSignal.RemoveListener(CloseButton);
			base.view.rushButton.ClickedSignal.RemoveListener(RushCooldown);
		}

		public override void Initialize(GUIArguments args)
		{
			plan = args.Get<MasterPlan>();
			bool hasReceivedFirstReward = masterPlanService.HasReceivedInitialRewardFromPlanDefinition(plan.Definition);
			base.view.Init(plan, hasReceivedFirstReward, timeEventService, definitionService, localService, fancyUIService, guiService);
			playGlobalSFX.Dispatch("Play_menu_popUp_01");
			lairModel.seenCooldownAlert = true;
		}

		private void CloseButton()
		{
			Close();
		}

		protected override void Close()
		{
			base.view.Close();
			playGlobalSFX.Dispatch("Play_menu_disappear_01");
		}

		private void CoolDownComplete(int masterplanID)
		{
			Close();
		}

		private void RushCooldown()
		{
			if (base.view.rushButton.isDoubleConfirmed())
			{
				playerService.ProcessRush(base.view.rushCost, true, "MasterPlanRush", RushTransactionCallback);
			}
		}

		private void RushTransactionCallback(PendingCurrencyTransaction pct)
		{
			if (pct.Success)
			{
				playGlobalSFX.Dispatch("Play_button_premium_01");
				setPremiumCurrencySignal.Dispatch();
				timeEventService.RushEvent(plan.ID);
				Close();
			}
		}

		private void CloseAnimationComplete()
		{
			if (base.view.timerRoutine != null)
			{
				StopCoroutine(base.view.timerRoutine);
			}
			base.view.Cleanup();
			plan.displayCooldownAlert = false;
			if (plan.cooldownUTCStartTime != 0)
			{
				gameContext.injectionBinder.GetInstance<CleanupMasterPlanSignal>().Dispatch(plan);
			}
			hideSignal.Dispatch("MasterPlanCooldownAlert");
			guiService.Execute(GUIOperation.Unload, "screen_MasterPlanCooldownAlert");
		}
	}
}
