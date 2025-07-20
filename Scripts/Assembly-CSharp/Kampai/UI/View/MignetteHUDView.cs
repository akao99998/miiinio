using Kampai.Main;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class MignetteHUDView : strange.extensions.mediation.impl.View
	{
		public ButtonView CloseButton;

		public Text LabelScore;

		public Transform DooberTarget;

		public KampaiImage CurrencyImage;

		public KampaiProgressBar ProgressBar;

		public Text TimeRemainingLabel;

		public RectTransform CounterRect;

		public Text CounterLabel;

		public Animation CountdownIntroAnimation;

		public GameObject CurrencyPanel;

		private ILocalizationService localizationService;

		private int previousProgressValue = -1;

		private int previousTimerValue = -1;

		[Inject]
		public PlayGlobalSoundFXSignal globalAudioSignal { get; set; }

		internal void Init(ILocalizationService localizationService)
		{
			this.localizationService = localizationService;
		}

		public void SetCollectableImage(Sprite image, Sprite mask)
		{
			if (image != null)
			{
				CurrencyImage.sprite = image;
			}
			if (mask != null)
			{
				CurrencyImage.maskSprite = mask;
			}
		}

		public void ShowTimeProgressBar(bool enable)
		{
			if (ProgressBar.gameObject.activeSelf != enable)
			{
				ProgressBar.gameObject.SetActive(enable);
			}
		}

		public void ShowCounter(bool enable)
		{
			if (CounterRect.gameObject.activeSelf != enable)
			{
				CounterRect.gameObject.SetActive(enable);
			}
		}

		public void SetScore(int score)
		{
			LabelScore.text = score.ToString();
		}

		public void SetCounter(int counterValue)
		{
			CounterLabel.text = counterValue.ToString();
		}

		public void SetProgressRemainingText(float progressBarPct)
		{
			int num = Mathf.FloorToInt(progressBarPct * 100f);
			if (num != previousProgressValue)
			{
				previousProgressValue = num;
				TimeRemainingLabel.text = string.Format("{0}%", num);
				ProgressBar.SetProgress(progressBarPct);
			}
		}

		public void SetTime(float timeRemainingInSeconds, float progressBarPct)
		{
			int num = (int)timeRemainingInSeconds;
			if (previousTimerValue != num)
			{
				previousTimerValue = num;
				TimeRemainingLabel.text = UIUtils.FormatTime(num, localizationService);
				if (num < 1)
				{
					progressBarPct = 0f;
				}
				ProgressBar.SetProgress(progressBarPct);
			}
		}

		public void StartCountdown()
		{
			CountdownIntroAnimation.Rewind();
			CountdownIntroAnimation.Play();
			globalAudioSignal.Dispatch("Play_mignette_countDown_01");
		}

		public void StartScorePresentationSequence()
		{
			CloseButton.gameObject.SetActive(false);
			CurrencyPanel.SetActive(false);
		}
	}
}
