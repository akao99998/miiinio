using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class QuestRewardMediator : UIStackMediator<QuestRewardView>
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("QuestRewardMediator") as IKampaiLogger;

		private Quest q;

		private bool collected;

		private ModalSettings modalSettings = new ModalSettings();

		private readonly AdPlacementName adPlacementName = AdPlacementName.QUEST;

		private AdPlacementInstance adPlacementInstance;

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IBuildMenuService buildMenuService { get; set; }

		[Inject]
		public CloseAllOtherMenuSignal closeSignal { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject(UIElement.CAMERA)]
		public Camera uiCamera { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject]
		public SpawnDooberSignal tweenSignal { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public FTUERewardClosed ftueRewardClosed { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrim { get; set; }

		[Inject]
		public HideDelayHUDSignal hideDelayHUDSignal { get; set; }

		[Inject]
		public HideDelayStoreSignal hideDelayStoreSignal { get; set; }

		[Inject]
		public IZoomCameraModel zoomCameraModel { get; set; }

		[Inject]
		public IFancyUIService fancyUIService { get; set; }

		[Inject]
		public MoveAudioListenerSignal toggleCharacterAudioSignal { get; set; }

		[Inject]
		public ShowAllWayFindersSignal showAllWayFindersSignal { get; set; }

		[Inject]
		public HideAllWayFindersSignal hideAllWayFindersSignal { get; set; }

		[Inject]
		public IncreaseInventoryCountForBuildMenuSignal increaseInventoryCountSignal { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public IMasterPlanQuestService masterPlanQuestService { get; set; }

		[Inject]
		public IMasterPlanService masterPlanService { get; set; }

		[Inject]
		public ResetLairWayfinderIconSignal resetIconSignal { get; set; }

		[Inject]
		public DisplayPlayerTrainingSignal playerTrainingSignal { get; set; }

		[Inject]
		public IRewardedAdService rewardedAdService { get; set; }

		[Inject]
		public RewardedAdRewardSignal rewardedAdRewardSignal { get; set; }

		[Inject]
		public AdPlacementActivityStateChangedSignal adPlacementActivityStateChangedSignal { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			soundFXSignal.Dispatch("Play_completeQuest_01");
			toggleCharacterAudioSignal.Dispatch(false, base.view.MinionSlot.transform);
			collected = false;
			base.view.collectButton.ClickedSignal.AddListener(Collect);
			base.view.adVideoButton.ClickedSignal.AddListener(AdVideo);
			base.view.OnMenuClose.AddListener(OnMenuClose);
			rewardedAdRewardSignal.AddListener(OnRewardedAdReward);
			adPlacementActivityStateChangedSignal.AddListener(OnAdPlacementActivityStateChanged);
			hideAllWayFindersSignal.Dispatch();
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.view.collectButton.ClickedSignal.RemoveListener(Collect);
			base.view.adVideoButton.ClickedSignal.RemoveListener(AdVideo);
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
			rewardedAdRewardSignal.RemoveListener(OnRewardedAdReward);
			adPlacementActivityStateChangedSignal.RemoveListener(OnAdPlacementActivityStateChanged);
			showAllWayFindersSignal.Dispatch();
		}

		public override void Initialize(GUIArguments args)
		{
			int id = args.Get<int>();
			modalSettings.enableCollectThrob = args.Contains<ThrobCollectButton>();
			RegisterRewards(id);
		}

		private void RegisterRewards(int id)
		{
			q = masterPlanQuestService.GetQuestByInstanceId(id);
			if (q == null)
			{
				toggleCharacterAudioSignal.Dispatch(true, null);
				logger.Error("You are trying to show a quest reward with an empty quest: {0}. Something is wrong.", id);
				OnMenuClose();
				return;
			}
			base.view.Init(q, localService, definitionService, playerService, modalSettings, fancyUIService, questService);
			if (q.GetActiveDefinition().type == QuestType.MasterPlan)
			{
				base.view.SetupVillainAtlas();
			}
			UpdateAdButton();
		}

		protected override void Close()
		{
			toggleCharacterAudioSignal.Dispatch(true, null);
			soundFXSignal.Dispatch("Play_menu_disappear_01");
			if (q != null)
			{
				Collect();
			}
		}

		private void Collect()
		{
			ExecuteRewardTransaction(1u);
		}

		private void ExecuteRewardTransaction(uint multiplier = 1)
		{
			if (q == null || collected)
			{
				return;
			}
			collected = true;
			QuestDefinition activeDefinition = q.GetActiveDefinition();
			TransactionDefinition transactionDefinition = GetQuestReward();
			if (transactionDefinition != null)
			{
				if (multiplier > 1)
				{
					transactionDefinition = GetMultiReward(transactionDefinition, multiplier);
				}
				TransactionArg arg = new TransactionArg((activeDefinition.type != QuestType.MasterPlan) ? "Quest" : "MasterPlan");
				playerService.RunEntireTransaction(transactionDefinition, TransactionTarget.NO_VISUAL, CollectTransactionCallback, arg);
			}
		}

		private TransactionDefinition GetQuestReward()
		{
			if (q == null)
			{
				return null;
			}
			QuestDefinition activeDefinition = q.GetActiveDefinition();
			return activeDefinition.GetReward(definitionService);
		}

		private TransactionDefinition GetMultiReward(TransactionDefinition transaction, uint multipier)
		{
			TransactionDefinition transactionDefinition = transaction.CopyTransaction();
			IList<QuantityItem> outputs = transactionDefinition.Outputs;
			if (outputs != null)
			{
				for (int i = 0; i < outputs.Count; i++)
				{
					outputs[i].Quantity *= multipier;
				}
			}
			return transactionDefinition;
		}

		private void AdVideo()
		{
			if (adPlacementInstance != null)
			{
				rewardedAdService.ShowRewardedVideo(adPlacementInstance);
			}
		}

		private void UpdateAdButton()
		{
			bool flag = rewardedAdService.IsPlacementActive(adPlacementName);
			AdPlacementInstance placementInstance = rewardedAdService.GetPlacementInstance(adPlacementName);
			bool flag2 = IsQuestRewardTypeAllowed(placementInstance);
			bool adEnabled = flag && flag2 && placementInstance != null;
			base.view.EnableAdButton(adEnabled);
			adPlacementInstance = placementInstance;
		}

		private bool IsQuestRewardTypeAllowed(AdPlacementInstance placement)
		{
			if (placement == null)
			{
				return false;
			}
			if (q == null)
			{
				return false;
			}
			QuestDefinition activeDefinition = q.GetActiveDefinition();
			if (activeDefinition != null)
			{
				if (activeDefinition.ForceDisableRewardedAd2xReward)
				{
					return false;
				}
				if (activeDefinition.ForceEnableRewardedAd2xReward)
				{
					return true;
				}
			}
			Quest2xRewardDefinition quest2xRewardDefinition = placement.Definition as Quest2xRewardDefinition;
			if (quest2xRewardDefinition != null)
			{
				List<int> allowedRewardItemTypes = quest2xRewardDefinition.AllowedRewardItemTypes;
				if (allowedRewardItemTypes != null)
				{
					TransactionDefinition questReward = GetQuestReward();
					if (questReward != null)
					{
						IList<QuantityItem> outputs = questReward.Outputs;
						if (outputs != null && outputs.Count > 0)
						{
							foreach (QuantityItem item in outputs)
							{
								if (!allowedRewardItemTypes.Contains(item.ID))
								{
									return false;
								}
							}
							return true;
						}
					}
				}
			}
			return false;
		}

		private void OnRewardedAdReward(AdPlacementInstance placement)
		{
			if (placement.Equals(adPlacementInstance))
			{
				ExecuteRewardTransaction(2u);
				rewardedAdService.RewardPlayer(null, placement);
				TransactionDefinition questReward = GetQuestReward();
				if (questReward != null)
				{
					telemetryService.Send_Telemetry_EVT_AD_INTERACTION(placement.Definition.Name, questReward.Outputs, placement.RewardPerPeriodCount);
				}
				adPlacementInstance = null;
			}
		}

		private void OnAdPlacementActivityStateChanged(AdPlacementInstance placement, bool enabled)
		{
			UpdateAdButton();
		}

		private void CollectTransactionCallback(PendingCurrencyTransaction pct)
		{
			if (pct.Success)
			{
				if (q.GetActiveDefinition().type == QuestType.MasterPlan)
				{
					MasterPlanQuestType.Component component = q as MasterPlanQuestType.Component;
					if (component != null)
					{
						if (component.component != null)
						{
							component.component.State = ((!component.isBuildQuest) ? MasterPlanComponentState.TasksCollected : MasterPlanComponentState.Complete);
							if (component.component.State == MasterPlanComponentState.Complete)
							{
								MasterPlanDefinition definition = masterPlanService.CurrentMasterPlan.Definition;
								VillainDefinition villainDefinition = definitionService.Get<VillainDefinition>(definition.VillainCharacterDefID);
								int componentCompleteCount = masterPlanService.GetComponentCompleteCount(definition);
								MasterPlanComponentBuildingDefinition masterPlanComponentBuildingDefinition = definitionService.Get<MasterPlanComponentBuildingDefinition>(definition.BuildingDefID);
								MasterPlanComponentBuildingDefinition masterPlanComponentBuildingDefinition2 = definitionService.Get<MasterPlanComponentBuildingDefinition>(component.component.buildingDefID);
								telemetryService.Send_Telemetry_EVT_MASTER_PLAN_COMPONENT_COMPLETE(masterPlanComponentBuildingDefinition.TaxonomySpecific, villainDefinition.LocalizedKey, masterPlanComponentBuildingDefinition2.TaxonomySpecific, componentCompleteCount);
							}
						}
						else
						{
							gameContext.injectionBinder.GetInstance<DisplayMasterPlanRewardDialogSignal>().Dispatch(component.masterPlan);
							playerTrainingSignal.Dispatch(19000030, false, new Signal<bool>());
						}
						resetIconSignal.Dispatch();
					}
				}
				CheckForBuildingAward();
				soundFXSignal.Dispatch("Play_button_click_01");
			}
			DooberUtil.CheckForTween(base.view.transactionDefinition, base.view.viewList, true, uiCamera, tweenSignal, definitionService);
			base.view.CloseView();
		}

		private void CheckForBuildingAward()
		{
			foreach (QuantityItem output in base.view.transactionDefinition.Outputs)
			{
				int iD = output.ID;
				BuildingDefinition definition;
				if (definitionService.TryGet<BuildingDefinition>(iD, out definition))
				{
					switch (definition.Type)
					{
					case BuildingType.BuildingTypeIdentifier.CRAFTING:
					case BuildingType.BuildingTypeIdentifier.DECORATION:
					case BuildingType.BuildingTypeIdentifier.LEISURE:
					case BuildingType.BuildingTypeIdentifier.RESOURCE:
					{
						int storeItemDefinitionIDFromBuildingID = buildMenuService.GetStoreItemDefinitionIDFromBuildingID(iD);
						StoreItemDefinition storeItemDefinition = definitionService.Get<StoreItemDefinition>(storeItemDefinitionIDFromBuildingID);
						buildMenuService.AddUncheckedInventoryItem(storeItemDefinition.Type, iD);
						increaseInventoryCountSignal.Dispatch();
						break;
					}
					}
				}
			}
		}

		private void OnMenuClose()
		{
			if (base.view.rewardDisplay != null)
			{
				StopCoroutine(base.view.rewardDisplay);
			}
			ftueRewardClosed.Dispatch();
			if (q != null)
			{
				gameContext.injectionBinder.GetInstance<GoToNextQuestStateSignal>().Dispatch(q.GetActiveDefinition().ID);
			}
			if (zoomCameraModel.ZoomedIn && zoomCameraModel.LastZoomBuildingType == BuildingZoomType.TIKIBAR)
			{
				hideDelayHUDSignal.Dispatch(3f);
				hideDelayStoreSignal.Dispatch(3f);
			}
			hideSkrim.Dispatch("QuestRewardSkrim");
			guiService.Execute(GUIOperation.Unload, "popup_QuestReward");
			toggleCharacterAudioSignal.Dispatch(true, null);
			closeSignal.Dispatch(base.view.gameObject);
		}
	}
}
