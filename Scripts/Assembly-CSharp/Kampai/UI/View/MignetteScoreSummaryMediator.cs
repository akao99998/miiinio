using System;
using System.Collections;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Game.Mignette;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;

namespace Kampai.UI.View
{
	public class MignetteScoreSummaryMediator : UIStackMediator<MignetteScoreSummaryView>
	{
		private const float initialDooberDelayTime = 1.2f;

		private const float dooberFlyTime = 1f;

		private const float dooberFlyScale = 1.75f;

		private const float waitBetweenDoobers = 0.1f;

		private const int maxDoobersToSend = 5;

		public IKampaiLogger logger = LogManager.GetClassLogger("MignetteScoreSummaryMediator") as IKampaiLogger;

		private int mignetteBuildingID;

		private MignetteBuilding mignetteBuilding;

		private bool showMignetteScoreIncrease;

		private int xpReward;

		private bool hasUnlockedCompositeBuildingReward;

		private DisplayableDefinition rewardReadyItemDef;

		private TransactionDefinition rewardReadyTransactionDef;

		private bool isClosed;

		private RewardCollection collection;

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public MignetteGameModel mignetteGameModel { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public StopMignetteSignal stopMignettSignal { get; set; }

		[Inject]
		public MignetteCollectionService collectionService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public SetHUDButtonsVisibleSignal setHUDButtonsVisibleSignal { get; set; }

		[Inject]
		public CreditCollectionRewardSignal creditCollectionRewardSignal { get; set; }

		[Inject]
		public SpawnDooberSignal spawnDooberSignal { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject(UIElement.CAMERA)]
		public Camera uiCamera { get; set; }

		[Inject]
		public PlayGlobalMusicSignal musicSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal globalAudioSignal { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrim { get; set; }

		[Inject]
		public EnableSkrimButtonSignal enableSkrimSignal { get; set; }

		[Inject]
		public CameraAutoMoveToMignetteSignal moveToMignetteSignal { get; set; }

		[Inject]
		public ShowAllWayFindersSignal showAllWayFindersSignal { get; set; }

		[Inject]
		public HideAllWayFindersSignal hideAllWayFindersSignal { get; set; }

		[Inject]
		public ReconcileSalesSignal reconcileSalesSignal { get; set; }

		[Inject]
		public RefreshMTXStoreSignal refreshMTXStoreSignal { get; set; }

		[Inject]
		public MignetteCallMinionsModel model { get; set; }

		public override void Initialize(GUIArguments args)
		{
			hideAllWayFindersSignal.Dispatch();
			showMignetteScoreIncrease = args.Get<bool>();
			mignetteBuildingID = args.Get<int>();
			mignetteBuilding = playerService.GetByInstanceId<MignetteBuilding>(mignetteBuildingID);
			bool isMignetteUnlocked = true;
			if (mignetteBuilding == null)
			{
				mignetteBuilding = args.Get<MignetteBuilding>();
				isMignetteUnlocked = false;
			}
			collection = collectionService.GetActiveCollectionForMignette(mignetteBuilding, false);
			collection.NumTimesPlayed++;
			for (int i = 0; i < collection.Definition.Rewards.Count; i++)
			{
				CreateRewardIndicator(collection, collection.Definition.Rewards[i]);
			}
			int score = (showMignetteScoreIncrease ? mignetteGameModel.CurrentGameScore : 0);
			if (showMignetteScoreIncrease && rewardReadyTransactionDef != null)
			{
				collectionService.pendingRewardTransaction = collectionService.CreditRewardForActiveMignette();
				refreshMTXStoreSignal.Dispatch();
			}
			xpReward = Mathf.CeilToInt(mignetteBuilding.Definition.XPRewardFactor);
			base.view.Init(localizationService, score, collection.CollectionScoreProgress, collection.CollectionScorePreReset, collection.GetMaxScore(), xpReward, mignetteBuilding.Definition, showMignetteScoreIncrease, isMignetteUnlocked, hasUnlockedCompositeBuildingReward);
			base.view.ConfirmButton.ClickedSignal.AddListener(OnConfirmButtonClicked);
			base.view.CollectCurrencySignal.AddListener(OnCollectCurrency);
			if (showMignetteScoreIncrease)
			{
				HandleScoreIncrease();
			}
		}

		private void HandleScoreIncrease()
		{
			IGUICommand command = guiService.BuildCommand(GUIOperation.Load, "MignetteHUD");
			GameObject mignetteHUD = guiService.Execute(command);
			StartCoroutine(ensureHUDAvailable(mignetteHUD));
		}

		private IEnumerator ensureHUDAvailable(GameObject mignetteHUD)
		{
			yield return new WaitForEndOfFrame();
			MignetteHUDMediator hudMediator = mignetteHUD.GetComponent<MignetteHUDMediator>();
			if (hudMediator != null)
			{
				hudMediator.StartScorePresentationSequence();
			}
			StartCoroutine(spawnDoobersThenCall(base.view.ProgressBarCurrencyIcon.rectTransform, onHUDPresentationFinished));
		}

		private IEnumerator spawnDoobersThenCall(RectTransform flyToTarget, Action completeCallback)
		{
			yield return new WaitForSeconds(1.2f);
			int numDoobers = Mathf.Min(mignetteGameModel.CurrentGameScore, 5);
			globalAudioSignal.Dispatch("Play_Mign_smallScoreEvent_01");
			for (int i = 0; i < numDoobers; i++)
			{
				spawnMignetteCurrencyDoober(flyToTarget);
				globalAudioSignal.Dispatch("Play_mignette_scoreFlyDown_01");
				yield return new WaitForSeconds(0.1f);
			}
			float firstArrivalTimeFromNow = 1f - 0.1f * (float)numDoobers;
			yield return new WaitForSeconds(firstArrivalTimeFromNow);
			completeCallback();
		}

		private void spawnMignetteCurrencyDoober(RectTransform flyToTarget)
		{
			RectTransform currencyImageClone = UnityEngine.Object.Instantiate(base.view.GroupScoreCurrencyIcon.rectTransform);
			currencyImageClone.SetParent(base.view.GroupScoreCurrencyIcon.rectTransform, false);
			currencyImageClone.anchoredPosition = Vector2.zero;
			currencyImageClone.transform.position = new Vector3(currencyImageClone.transform.position.x, currencyImageClone.transform.position.y, -10f);
			currencyImageClone.localScale = Vector3.one;
			currencyImageClone.SetParent(flyToTarget, true);
			Go.to(currencyImageClone.transform, 1f, new GoTweenConfig().setEaseType(GoEaseType.CubicInOut).localPosition(Vector3.zero).onComplete(delegate(AbstractGoTween thisTween)
			{
				thisTween.destroy();
				UnityEngine.Object.Destroy(currencyImageClone.gameObject);
			}));
			Go.to(currencyImageClone.transform, 0.5f, new GoTweenConfig().setEaseType(GoEaseType.Linear).scale(1.75f));
			Go.to(currencyImageClone.transform, 0.5f, new GoTweenConfig().setDelay(0.5f).setEaseType(GoEaseType.Linear).scale(1f));
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.view.ConfirmButton.ClickedSignal.RemoveListener(OnConfirmButtonClicked);
			base.view.CollectCurrencySignal.RemoveListener(OnCollectCurrency);
			setHUDButtonsVisibleSignal.Dispatch(true);
			showAllWayFindersSignal.Dispatch();
		}

		private void CreateRewardIndicator(RewardCollection collection, CollectionReward reward)
		{
			bool flag = collection.IsRewardReadyForCollection(reward);
			TransactionDefinition transactionDefinition = definitionService.Get<TransactionDefinition>(reward.TransactionID);
			DisplayableDefinition displayableDefinition = definitionService.Get(transactionDefinition.Outputs[0].ID) as DisplayableDefinition;
			if (flag && rewardReadyItemDef == null)
			{
				rewardReadyItemDef = displayableDefinition;
				rewardReadyTransactionDef = transactionDefinition;
				int tier = collection.Definition.Rewards.IndexOf(reward) + 1;
				telemetryService.Send_Telemtry_EVT_MINI_TIER_REACHED(mignetteBuilding.Definition.LocalizedKey, tier, collection.NumTimesPlayed);
				if (displayableDefinition is CompositeBuildingPieceDefinition)
				{
					hasUnlockedCompositeBuildingReward = true;
				}
			}
			base.view.CreateRewardIndicator(reward.RequiredPoints, transactionDefinition.Outputs[0].Quantity, collection.GetMaxScore(), displayableDefinition.Image, displayableDefinition.Mask, flag);
		}

		private void onHUDPresentationFinished()
		{
			base.view.RefreshProgressBar(OnCompleteRefreshProgressBar);
		}

		private void OnCompleteRefreshProgressBar()
		{
			StartCoroutine(AwardXPAndShowButton());
		}

		private IEnumerator AwardXPAndShowButton()
		{
			yield return new WaitForSeconds(2f);
			playerService.AddXP(xpReward);
			logger.Log(KampaiLogLevel.Info, "Awarded " + xpReward + " Fun Points For completing the mignette");
			Vector3 buildingPosition = new Vector3(mignetteBuilding.Location.x, 0f, mignetteBuilding.Location.y);
			spawnDooberSignal.Dispatch(buildingPosition, DestinationType.XP, 2, true);
			globalAudioSignal.Dispatch("Play_drop_harvest_01");
			yield return new WaitForSeconds(2.5f);
			enableSkrimSignal.Dispatch(true);
			base.view.ConfirmButton.gameObject.SetActive(true);
		}

		private void OnCollectCurrency(GameObject spawnDooberTransform)
		{
			StartCoroutine(SpawnDoobersThenCreditReward(spawnDooberTransform));
		}

		private IEnumerator SpawnDoobersThenCreditReward(GameObject spawnDooberTransform)
		{
			for (int i = 0; i < 7; i++)
			{
				globalAudioSignal.Dispatch("Play_mignette_rewardFlyUp_01");
				DooberUtil.CheckForTween(rewardReadyTransactionDef, new List<GameObject> { spawnDooberTransform }, true, uiCamera, spawnDooberSignal, definitionService);
				yield return new WaitForSeconds(0.2f);
			}
			yield return new WaitForSeconds(0.8f);
		}

		private void CloseMignetteScoreView()
		{
			if (showMignetteScoreIncrease && !isClosed)
			{
				isClosed = true;
				if (hasUnlockedCompositeBuildingReward)
				{
					creditCollectionRewardSignal.Dispatch();
				}
				hideSkrim.Dispatch("MignetteSkrim");
				guiService.Execute(GUIOperation.Unload, "MignetteScoreSummary");
				stopMignettSignal.Dispatch(false);
				Dictionary<string, float> type = new Dictionary<string, float>();
				musicSignal.Dispatch("Play_backGroundMusic_01", type);
				if (mignetteBuilding.Definition.ID == 3502)
				{
					reconcileSalesSignal.Dispatch(0);
				}
			}
		}

		private void CloseMiniGamesView()
		{
			if (!showMignetteScoreIncrease)
			{
				model.NumberOfMinionsToCall = mignetteBuilding.MinionSlotsOwned;
				model.Building = mignetteBuilding;
				model.SignalSender = base.view.gameObject;
				bool type = ((mignetteBuilding.State != BuildingState.Cooldown) ? true : false);
				moveToMignetteSignal.Dispatch(mignetteBuilding.Definition.ID, type, new PanInstructions(null));
			}
		}

		protected override void Close()
		{
			CloseMignetteScoreView();
		}

		private void OnConfirmButtonClicked()
		{
			CloseMignetteScoreView();
			CloseMiniGamesView();
		}
	}
}
