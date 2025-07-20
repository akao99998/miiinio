using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.Util;
using strange.extensions.context.api;

namespace Kampai.UI.View
{
	public class ProceduralQuestMediator : UIStackMediator<ProceduralQuestView>
	{
		private Quest quest;

		private int itemCountOnPreviousUpdate = -1;

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public DeliverTaskItemSignal deliverTaskItemSignal { get; set; }

		[Inject]
		public UpdateProceduralQuestPanelSignal updateSignal { get; set; }

		[Inject]
		public CloseAllOtherMenuSignal closeSignal { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrimSignal { get; set; }

		[Inject]
		public CancelTSMQuestTaskSignal cancelQuestTaskSignal { get; set; }

		[Inject]
		public StartQuestTaskSignal startQuestTaskSignal { get; set; }

		[Inject]
		public CollectTSMQuestTaskRewardSignal collectRewardSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public IFancyUIService fancyUIService { get; set; }

		[Inject]
		public SetStorageCapacitySignal setStorageCapacitySignal { get; set; }

		[Inject]
		public MoveAudioListenerSignal moveAudioListenerSignal { get; set; }

		public override void OnRegister()
		{
			closeSignal.Dispatch(null);
			base.OnRegister();
			base.view.Init(localService, fancyUIService, moveAudioListenerSignal);
			updateSignal.AddListener(UpdateView);
			base.view.OnMenuClose.AddListener(OnMenuClose);
			base.view.YesSellButton.onClick.AddListener(YesSell);
			base.view.NoSellButton.onClick.AddListener(DeclineQuest);
			gameContext.injectionBinder.GetInstance<DisplayMinionSelectedIconSignal>().Dispatch(301, true);
			gameContext.injectionBinder.GetInstance<ToggleVignetteSignal>().Dispatch(true, 0f);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			updateSignal.RemoveListener(UpdateView);
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
			CleanupListeners();
			gameContext.injectionBinder.GetInstance<DisplayMinionSelectedIconSignal>().Dispatch(301, false);
			gameContext.injectionBinder.GetInstance<ToggleVignetteSignal>().Dispatch(false, null);
		}

		private void CleanupListeners()
		{
			base.view.YesSellButton.onClick.RemoveListener(YesSell);
			base.view.NoSellButton.onClick.RemoveListener(DeclineQuest);
		}

		public override void Initialize(GUIArguments args)
		{
			int questInstanceId = args.Get<int>();
			UpdateView(questInstanceId);
		}

		private void UpdateView(int questInstanceId)
		{
			quest = playerService.GetByInstanceId<Quest>(questInstanceId);
			if (quest == null || quest.GetActiveDefinition() == null || quest.GetActiveDefinition().SurfaceType != QuestSurfaceType.ProcedurallyGenerated)
			{
				return;
			}
			IList<QuestStep> steps = quest.Steps;
			if (steps == null || steps.Count == 0 || steps[0] == null || quest.GetActiveDefinition().QuestSteps[0] == null)
			{
				return;
			}
			int rewardCount = 0;
			ItemDefinition inputItem = definitionService.Get<ItemDefinition>(quest.GetActiveDefinition().QuestSteps[0].ItemDefinitionID);
			ItemDefinition outputItem = null;
			TransactionDefinition reward = quest.GetActiveDefinition().GetReward(definitionService);
			if (reward != null)
			{
				IList<QuantityItem> outputs = reward.Outputs;
				if (outputs.Count > 0)
				{
					rewardCount = (int)outputs[0].Quantity;
					outputItem = definitionService.Get<ItemDefinition>(outputs[0].ID);
				}
			}
			if (quest.state == QuestState.Harvestable)
			{
				Collect();
			}
			else
			{
				HandleNonHarvestable(rewardCount, inputItem, outputItem);
			}
		}

		private void HandleNonHarvestable(int rewardCount, ItemDefinition inputItem, ItemDefinition outputItem)
		{
			QuestStepDefinition questStepDefinition = quest.GetActiveDefinition().QuestSteps[0];
			int itemAmount = questStepDefinition.ItemAmount;
			int quantityByDefinitionId = (int)playerService.GetQuantityByDefinitionId(questStepDefinition.ItemDefinitionID);
			base.view.InitSellView(itemAmount, quantityByDefinitionId, rewardCount, inputItem, outputItem);
			if (itemCountOnPreviousUpdate < 0)
			{
				itemCountOnPreviousUpdate = quantityByDefinitionId;
			}
			else if (itemCountOnPreviousUpdate != quantityByDefinitionId)
			{
				itemCountOnPreviousUpdate = quantityByDefinitionId;
				setStorageCapacitySignal.Dispatch();
			}
		}

		protected override void Close()
		{
			moveAudioListenerSignal.Dispatch(true, null);
			playSFXSignal.Dispatch("Play_menu_disappear_01");
			base.view.Close();
		}

		private void OnMenuClose()
		{
			hideSkrimSignal.Dispatch("ProceduralTaskSkrim");
			guiService.Execute(GUIOperation.Unload, "popup_TSM_SellItems");
		}

		private void Collect()
		{
			CleanupListeners();
			collectRewardSignal.Dispatch(quest);
			closeSignal.Dispatch(null);
		}

		private void DeclineQuest()
		{
			playSFXSignal.Dispatch("Play_button_click_01");
			cancelQuestTaskSignal.Dispatch(quest);
			closeSignal.Dispatch(null);
		}

		private void YesSell()
		{
			if (!base.view.IsAnimationPlaying("Close"))
			{
				playSFXSignal.Dispatch("Play_button_click_01");
				startQuestTaskSignal.Dispatch(quest);
				deliverTaskItemSignal.Dispatch(new Tuple<int, int>(quest.ID, 0));
			}
		}
	}
}
