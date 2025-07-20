using Kampai.Game;
using Kampai.Game.Mignette;
using Kampai.Main;

namespace Kampai.UI.View
{
	public class MignetteHUDMediator : UIStackMediator<MignetteHUDView>
	{
		private bool isShowingResults;

		[Inject]
		public RequestStopMignetteSignal requestStopMignetteSignal { get; set; }

		[Inject]
		public MignetteScoreUpdatedSignal mignetteScoreUpdatedSignal { get; set; }

		[Inject]
		public StartMignetteHUDCountdownSignal startMignetteHUDCountdownSignal { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public MignetteGameModel mignetteModel { get; set; }

		[Inject]
		public HideSkrimSignal hideSignal { get; set; }

		[Inject]
		public ShowAndIncreaseMignetteScoreSignal showResultsSignal { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			base.view.Init(localizationService);
			base.view.CloseButton.ClickedSignal.AddListener(OnCloseButtonClicked);
			mignetteScoreUpdatedSignal.AddListener(OnScoreUpdated);
			startMignetteHUDCountdownSignal.AddListener(OnStartCountdown);
			showResultsSignal.AddListener(OnMignetteResultShown);
			OnScoreUpdated(mignetteModel.CurrentGameScore);
			isShowingResults = false;
			base.view.SetCollectableImage(mignetteModel.CollectableImage, mignetteModel.CollectableImageMask);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.view.CloseButton.ClickedSignal.RemoveListener(OnCloseButtonClicked);
			mignetteScoreUpdatedSignal.RemoveListener(OnScoreUpdated);
			startMignetteHUDCountdownSignal.RemoveListener(OnStartCountdown);
			showResultsSignal.RemoveListener(OnMignetteResultShown);
			hideSignal.Dispatch("MignetteSkrim");
		}

		protected override void Update()
		{
			if (!isShowingResults)
			{
				base.view.ShowCounter(mignetteModel.UsesCounterHUD);
				base.view.SetCounter(mignetteModel.CounterValue);
				base.view.ShowTimeProgressBar(mignetteModel.UsesTimerHUD || mignetteModel.UsesProgressHUD);
				if (mignetteModel.UsesTimerHUD)
				{
					base.view.SetTime(mignetteModel.TimeRemaining, mignetteModel.TimeRemaining / mignetteModel.TotalEventTime);
				}
				else if (mignetteModel.UsesProgressHUD)
				{
					base.view.ShowTimeProgressBar(mignetteModel.UsesProgressHUD);
					base.view.SetProgressRemainingText(mignetteModel.PercentCompleted);
				}
			}
		}

		private void OnMignetteResultShown()
		{
			isShowingResults = true;
			base.view.ShowCounter(false);
			base.view.ShowTimeProgressBar(false);
		}

		private void OnStartCountdown()
		{
			base.view.StartCountdown();
		}

		protected override void Close()
		{
			requestStopMignetteSignal.Dispatch(false);
		}

		private void OnCloseButtonClicked()
		{
			Close();
		}

		private void OnScoreUpdated(int score)
		{
			base.view.SetScore(score);
		}

		public void StartScorePresentationSequence()
		{
			base.view.StartScorePresentationSequence();
		}
	}
}
