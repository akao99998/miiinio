using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Main;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class QuestPanelMediator : UIStackMediator<QuestPanelView>
	{
		private ModalSettings modalSettings = new ModalSettings();

		private int showQuestRewardID;

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public QuestDetailIDSignal idSignal { get; set; }

		[Inject]
		public CloseQuestBookSignal closeQuestSignal { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public ILocalPersistanceService localPersist { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public UpdateQuestPanelWithNewQuestSignal updateQuestPanelSignal { get; set; }

		[Inject]
		public UpdateQuestLineProgressSignal updateQuestLineProgressSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSoundFXSignal { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrimSignal { get; set; }

		[Inject]
		public ShowQuestRewardSignal questRewardSignal { get; set; }

		[Inject]
		public FTUEProgressSignal FTUEsignal { get; set; }

		[Inject]
		public FTUEQuestPanelCloseSignal FTUECloseMenuSignal { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public QuestUIModel questUIModel { get; set; }

		[Inject]
		public ShowHUDSignal showHUDSignal { get; set; }

		[Inject]
		public IZoomCameraModel zoomCameraModel { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		[Inject]
		public IFancyUIService fancyUIService { get; set; }

		[Inject]
		public MoveAudioListenerSignal toggleCharacterAudioSignal { get; set; }

		[Inject]
		public ShowSettingsButtonSignal showSettingsButtonSignal { get; set; }

		[Inject]
		public ShowPetsButtonSignal showPetsButtonSignal { get; set; }

		[Inject]
		public SetBuildMenuEnabledSignal setBuildMenuEnabledSignal { get; set; }

		[Inject]
		public ShowAllWayFindersSignal showAllWayFindersSignal { get; set; }

		[Inject]
		public HideAllWayFindersSignal hideAllWayFindersSignal { get; set; }

		[Inject]
		public DisplayPlayerTrainingSignal displayPlayerTrainingSignal { get; set; }

		[Inject]
		public FadeBackgroundAudioSignal fadeBackgroundAudioSignal { get; set; }

		[Inject]
		public QuestRewardPopupContentsSignal popupContentsSignal { get; set; }

		[Inject]
		public HideRewardDisplaySignal hideRewardDisplaySignal { get; set; }

		[Inject]
		public IMasterPlanQuestService masterPlanQuestService { get; set; }

		[Inject]
		public UIModel uiModel { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public IGoToService goToService { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			FTUEsignal.Dispatch();
			base.view.OnMenuClose.AddListener(OnMenuClose);
			base.view.RewardItemButton.ClickedSignal.AddListener(RewardItemsClicked);
			idSignal.AddListener(RegisterID);
			updateQuestPanelSignal.AddListener(UpdateQuestSteps);
			closeQuestSignal.AddListener(Close);
			showSettingsButtonSignal.Dispatch(false);
			setBuildMenuEnabledSignal.Dispatch(false);
			showPetsButtonSignal.Dispatch(false);
			hideAllWayFindersSignal.Dispatch();
			FadeBackgroundAudio(true);
			questService.UpdateMasterPlanQuestLine();
		}

		public override void OnRemove()
		{
			showSettingsButtonSignal.Dispatch(true);
			setBuildMenuEnabledSignal.Dispatch(true);
			showPetsButtonSignal.Dispatch(true);
			base.OnRemove();
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
			base.view.RewardItemButton.ClickedSignal.RemoveListener(RewardItemsClicked);
			idSignal.RemoveListener(RegisterID);
			updateQuestPanelSignal.RemoveListener(UpdateQuestSteps);
			closeQuestSignal.RemoveListener(Close);
			showAllWayFindersSignal.Dispatch();
			FadeBackgroundAudio(false);
		}

		public override void Initialize(GUIArguments args)
		{
			int id = args.Get<int>();
			modalSettings.enableGotoThrob = args.Contains<ThrobGotoButton>();
			modalSettings.enableDeliverThrob = args.Contains<ThrobDeliverButton>();
			modalSettings.enablePurchaseButtons = !args.Contains<DisablePurchaseButton>();
			base.view.Init();
			base.view.questService = questService;
			base.view.localizationService = localService;
			base.view.timeService = timeService;
			base.view.modalSettings = modalSettings;
			toggleCharacterAudioSignal.Dispatch(false, base.view.currentQuestView.MinionSlot.transform);
			RegisterID(id);
			base.closeAllOtherMenuSignal.Dispatch(base.view.gameObject);
		}

		private void RegisterID(int id)
		{
			playSoundFXSignal.Dispatch("Play_menu_popUp_01");
			Quest questByInstanceId = masterPlanQuestService.GetQuestByInstanceId(id);
			List<Quest> quests = masterPlanQuestService.GetQuests();
			InitQuestTabs(quests, questByInstanceId, questByInstanceId.GetActiveDefinition().SurfaceID, false);
			if (questByInstanceId.state == QuestState.Harvestable)
			{
				showQuestRewardID = questByInstanceId.ID;
			}
			TransactionDefinition reward = questByInstanceId.GetActiveDefinition().GetReward(definitionService);
			if (reward != null)
			{
				base.view.CreateQuestSteps(questByInstanceId, reward, definitionService);
				base.view.Open();
			}
			updateQuestLineProgressSignal.Dispatch(id);
			QuestDefinition definition = questByInstanceId.Definition;
			if (definition.ShowRewardsPopupByDefault && !localPersist.HasKey(definition.ID.ToString()))
			{
				popupContentsSignal.Dispatch(base.view.RewardItemButton.questRewards);
				localPersist.PutDataInt(definition.ID.ToString(), 0);
			}
		}

		private void UpdateQuestSteps(int questId)
		{
			hideRewardDisplaySignal.Dispatch();
			Quest questByInstanceId = masterPlanQuestService.GetQuestByInstanceId(questId);
			List<Quest> quests = masterPlanQuestService.GetQuests();
			InitQuestTabs(quests, questByInstanceId, questByInstanceId.GetActiveDefinition().SurfaceID);
			TransactionDefinition reward = questByInstanceId.GetActiveDefinition().GetReward(definitionService);
			if (reward != null)
			{
				base.view.CreateQuestSteps(questByInstanceId, reward, definitionService);
			}
		}

		private void InitQuestTabs(List<Quest> quests, Quest selectedQuest, int surfaceId, bool isUpdate = true)
		{
			int lastSelectedQuestID = questUIModel.lastSelectedQuestID;
			questUIModel.lastSelectedQuestID = selectedQuest.ID;
			Prestige prestige = prestigeService.GetPrestige(surfaceId);
			DummyCharacterType characterType = DummyCharacterType.Minion;
			if (prestige.Definition.Type == PrestigeType.Minion)
			{
				Definition definition = definitionService.Get<Definition>(prestige.Definition.TrackedDefinitionID);
				if (definition is NamedCharacterDefinition)
				{
					characterType = DummyCharacterType.NamedCharacter;
				}
			}
			else
			{
				characterType = DummyCharacterType.NamedCharacter;
			}
			if (isUpdate)
			{
				base.view.SetCurrentQuestImage(surfaceId, characterType);
				Quest questByInstanceId = masterPlanQuestService.GetQuestByInstanceId(lastSelectedQuestID);
				base.view.SwapQuest(questByInstanceId, selectedQuest.ID);
			}
			else
			{
				base.view.InitCurrentQuestImage(surfaceId, fancyUIService, characterType);
				base.view.InitQuestTabs(quests, selectedQuest.ID);
			}
		}

		private void RewardItemsClicked(List<DisplayableDefinition> itemDefs)
		{
			popupContentsSignal.Dispatch(itemDefs);
		}

		private void OnMenuClose()
		{
			if (!uiModel.GoToClicked && questService.ShouldPulseMoveButtonAccept())
			{
				goToService.OpenStoreFromAnywhere(3123);
			}
			uiModel.GoToClicked = false;
			base.view.RemoveCharacters();
			FTUECloseMenuSignal.Dispatch();
			hideSkrimSignal.Dispatch("QuestPanelSkrim");
			guiService.Execute(GUIOperation.Unload, "screen_QuestPanel");
			if (showQuestRewardID != 0)
			{
				questRewardSignal.Dispatch(showQuestRewardID);
				return;
			}
			Quest byInstanceId = playerService.GetByInstanceId<Quest>(questUIModel.lastSelectedQuestID);
			if (byInstanceId != null)
			{
				displayPlayerTrainingSignal.Dispatch(byInstanceId.GetActiveDefinition().QuestModalClosePlayerTrainingCategoryItemId, false, new Signal<bool>());
			}
		}

		protected override void Close()
		{
			toggleCharacterAudioSignal.Dispatch(true, null);
			playSoundFXSignal.Dispatch("Play_menu_disappear_01");
			if (zoomCameraModel.ZoomedIn && zoomCameraModel.LastZoomBuildingType == BuildingZoomType.TIKIBAR)
			{
				showHUDSignal.Dispatch(false);
			}
			base.view.CloseView();
		}

		private void FadeBackgroundAudio(bool fade)
		{
			if (zoomCameraModel.ZoomedIn && !zoomCameraModel.ZoomInProgress)
			{
				fadeBackgroundAudioSignal.Dispatch(fade, "Play_tikiBar_snapshotDuck_01");
			}
		}
	}
}
