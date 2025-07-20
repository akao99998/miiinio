using System;
using System.Collections;
using System.Collections.Generic;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class MignetteScoreSummaryView : PopupMenuView
	{
		public GameObject MainContainer;

		public ButtonView ConfirmButton;

		public Text ConfirmButtonText;

		public AnimatedProgressBarViewObject ProgressBar;

		public GameObject CollectionRewardPanel;

		public GameObject ScorePanel;

		public KampaiImage ProgressBarCurrencyIcon;

		public KampaiImage GroupScoreCurrencyIcon;

		public Text TitleLabel;

		public Text ScoreTitleLabel;

		public Text ScoreAmountLabel;

		public Text TotalScoreLabel;

		public Text FunPointsAmount;

		public KampaiImage MignetteImage;

		public List<MignetteRuleViewObject> RuleDisplays;

		public GameObject LockedOverlay;

		public Image LockedImage;

		public Signal<GameObject> CollectCurrencySignal = new Signal<GameObject>();

		private int newProgressValue;

		private CollectionRewardIndicator rewardIndicatorReadyToCollect;

		private bool hasUnlockedBuildingReward;

		[Inject]
		public PlayGlobalSoundFXSignal globalAudioSignal { get; set; }

		protected override void Start()
		{
			base.Start();
			MainContainer.SetActive(false);
		}

		public void Init(ILocalizationService localizationService, int score, int collectionProgress, int collectionProgessBeforeReset, int maxScore, int xpReward, MignetteBuildingDefinition mignetteDefinition, bool showMignetteScoreIncrease, bool isMignetteUnlocked, bool hasUnlockedBuildingReward)
		{
			MainContainer.SetActive(true);
			base.Init();
			int num = 0;
			if (collectionProgress > 0)
			{
				num = collectionProgress - score;
				newProgressValue = collectionProgress;
			}
			else
			{
				num = collectionProgessBeforeReset - score;
				newProgressValue = collectionProgessBeforeReset;
			}
			this.hasUnlockedBuildingReward = hasUnlockedBuildingReward;
			ProgressBar.Init(num, maxScore);
			KampaiImage progressBarCurrencyIcon = ProgressBarCurrencyIcon;
			Sprite sprite = UIUtils.LoadSpriteFromPath(mignetteDefinition.CollectableImage);
			GroupScoreCurrencyIcon.sprite = sprite;
			progressBarCurrencyIcon.sprite = sprite;
			KampaiImage progressBarCurrencyIcon2 = ProgressBarCurrencyIcon;
			sprite = UIUtils.LoadSpriteFromPath(mignetteDefinition.CollectableImageMask);
			GroupScoreCurrencyIcon.maskSprite = sprite;
			progressBarCurrencyIcon2.maskSprite = sprite;
			MignetteImage.sprite = UIUtils.LoadSpriteFromPath(mignetteDefinition.Image);
			MignetteImage.maskSprite = UIUtils.LoadSpriteFromPath(mignetteDefinition.Mask);
			TitleLabel.text = localizationService.GetString(mignetteDefinition.LocalizedKey);
			ScoreTitleLabel.text = localizationService.GetString("MignetteScoreSummary_Score");
			ScoreAmountLabel.text = score.ToString();
			TotalScoreLabel.text = localizationService.GetString("MignetteTotalScore");
			FunPointsAmount.text = xpReward.ToString();
			if (showMignetteScoreIncrease)
			{
				if (hasUnlockedBuildingReward)
				{
					ConfirmButtonText.text = localizationService.GetString("MignetteScoreSummary_CollectButton");
				}
				else
				{
					ConfirmButtonText.text = localizationService.GetString("MignetteScoreSummary_ConfirmButton");
				}
			}
			else
			{
				ConfirmButtonText.text = localizationService.GetString("MignetteScoreSummary_GoToButton");
			}
			LockedOverlay.SetActive(!isMignetteUnlocked);
			ConfirmButton.GetComponent<Button>().interactable = isMignetteUnlocked;
			ConfirmButton.gameObject.SetActive(!showMignetteScoreIncrease);
			ScorePanel.SetActive(showMignetteScoreIncrease);
			base.Open();
		}

		public void CreateRewardIndicator(int requiredPoints, uint rewardQuantity, int maxScore, string rewardImagePath, string rewardMaskImagePath, bool isReadyForCollection)
		{
			GameObject gameObject = (GameObject)KampaiResources.Load("cmp_MignetteReward");
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
			CollectionRewardIndicator component = gameObject2.GetComponent<CollectionRewardIndicator>();
			component.RewardLocLabel.text = UIUtils.FormatLargeNumber(requiredPoints);
			if (rewardQuantity > 1)
			{
				component.RewardCountLabel.text = UIUtils.FormatLargeNumber((int)rewardQuantity);
			}
			else
			{
				component.RewardCountLabel.enabled = false;
			}
			component.RewardImage.sprite = UIUtils.LoadSpriteFromPath(rewardImagePath);
			component.RewardImage.maskSprite = UIUtils.LoadSpriteFromPath(rewardMaskImagePath);
			RectTransform component2 = CollectionRewardPanel.GetComponent<RectTransform>();
			float width = component2.rect.width;
			RectTransform component3 = gameObject2.GetComponent<RectTransform>();
			component3.SetParent(component2, false);
			component3.localScale = gameObject.transform.localScale;
			component3.localPosition = Vector3.zero;
			component3.anchoredPosition = new Vector2(width * (float)requiredPoints / (float)maxScore, 0f);
			if (isReadyForCollection)
			{
				rewardIndicatorReadyToCollect = component;
			}
		}

		public void RefreshProgressBar(Action onComplete)
		{
			ProgressBar.AnimateToValue(newProgressValue);
			if (rewardIndicatorReadyToCollect != null)
			{
				StartCoroutine(WaitThenShowRewardIndicatorAndConfirmButton(onComplete));
			}
			else
			{
				StartCoroutine(WaitThenShowConfirmButton(onComplete));
			}
		}

		private IEnumerator WaitThenShowRewardIndicatorAndConfirmButton(Action onComplete)
		{
			yield return new WaitForSeconds(ProgressBar.FillMainWaitTime + 1.2f);
			globalAudioSignal.Dispatch("Play_Mign_receivedAward_01");
			rewardIndicatorReadyToCollect.PlayCollectAnimation();
			yield return new WaitForSeconds(1.6f);
			if (!hasUnlockedBuildingReward)
			{
				CollectCurrencySignal.Dispatch(rewardIndicatorReadyToCollect.RewardImage.gameObject);
				yield return new WaitForSeconds(2f);
			}
			onComplete();
		}

		private IEnumerator WaitThenShowConfirmButton(Action onComplete)
		{
			yield return new WaitForSeconds(ProgressBar.FillMainWaitTime + 0.3f);
			onComplete();
		}
	}
}
