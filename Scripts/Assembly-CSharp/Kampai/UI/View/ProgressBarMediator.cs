using Elevation.Logging;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using strange.extensions.context.api;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class ProgressBarMediator : Mediator
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("ProgressBarMediator") as IKampaiLogger;

		private int rushCost;

		private bool started;

		[Inject]
		public ProgressBarView view { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IPositionService positionService { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public RushRevealBuildingSignal rushRevealBuildingSignal { get; set; }

		[Inject(UIElement.CONTEXT)]
		public ICrossContextCapable uiContext { get; set; }

		[Inject]
		public RemoveWorldProgressSignal removeWorldProgressSignal { get; set; }

		public override void OnRegister()
		{
			view.Init(positionService, gameContext, logger, playerService, localizationService);
			view.OnTimerCompleteSignal.AddListener(OnComplete);
			view.rushButton.ClickedSignal.AddListener(Rush);
			view.OnShowSignal.AddListener(OnShow);
			view.OnRemoveSignal.AddListener(OnRemoveProgressBar);
		}

		public override void OnRemove()
		{
			view.OnTimerCompleteSignal.RemoveListener(OnComplete);
			view.rushButton.ClickedSignal.RemoveListener(Rush);
			view.OnShowSignal.RemoveListener(OnShow);
			view.OnRemoveSignal.RemoveListener(OnRemoveProgressBar);
		}

		private void OnShow()
		{
			started = true;
			view.StartTime(view.startTime, view.endTime);
			InvokeRepeating("UpdateTime", 0.001f, 1f);
		}

		private void OnRemoveProgressBar()
		{
			removeWorldProgressSignal.Dispatch(view.TrackedId);
		}

		public void StopTime()
		{
			if (started)
			{
				view.OnTimerCompleteSignal.RemoveListener(OnComplete);
				view.StopTime();
				CancelInvoke("UpdateTime");
			}
		}

		public void UpdateTime()
		{
			int timeRemaining = timeEventService.GetTimeRemaining(view.TrackedId);
			view.UpdateTime(timeRemaining);
			rushCost = timeEventService.CalculateRushCostForTimer(timeRemaining, RushActionType.CONSTRUCTION);
			view.SetRushCost(rushCost);
		}

		private void OnComplete(int instanceId)
		{
			if (view.TrackedId == instanceId)
			{
				removeWorldProgressSignal.Dispatch(instanceId);
				StopTime();
			}
		}

		private void Rush()
		{
			if (view.rushButton.isDoubleConfirmed())
			{
				playerService.ProcessRush(rushCost, true, RushTransactionCallback, view.TrackedId);
			}
		}

		private void RushTransactionCallback(PendingCurrencyTransaction pct)
		{
			if (pct.Success)
			{
				int trackedId = view.TrackedId;
				uiContext.injectionBinder.GetInstance<SetBuildingRushedSignal>().Dispatch(trackedId);
				timeEventService.RushEvent(trackedId);
				rushRevealBuildingSignal.Dispatch(trackedId);
				playSFXSignal.Dispatch("Play_button_premium_01");
			}
		}
	}
}
