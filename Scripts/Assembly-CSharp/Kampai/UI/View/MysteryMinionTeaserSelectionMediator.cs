using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Game.Trigger;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.UI.View
{
	public class MysteryMinionTeaserSelectionMediator : KampaiMediator
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("MysteryMinionTeaserSelectionMediator") as IKampaiLogger;

		private int rewardIndex;

		private TransactionDefinition trans1;

		private TransactionDefinition trans2;

		private TriggerInstance trigger;

		[Inject]
		public MysteryMinionTeaserSelectionView view { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public CloseAllOtherMenuSignal closeSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrimSignal { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public IFancyUIService fancyUIService { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject(UIElement.CAMERA)]
		public Camera uiCamera { get; set; }

		[Inject]
		public SpawnDooberSignal tweenSignal { get; set; }

		[Inject]
		public IBuildMenuService buildMenuService { get; set; }

		[Inject]
		public UpdateUIButtonsSignal updateStoreButtonsSignal { get; set; }

		[Inject]
		public MoveAudioListenerSignal moveAudioListenerSignal { get; set; }

		public override void Initialize(GUIArguments args)
		{
			closeSignal.Dispatch(view.gameObject);
			view.Initialize(fancyUIService, playSFXSignal);
			trigger = args.Get<TriggerInstance>();
			PendingRewardDefinition pendingRewardDefinition = GetPendingRewardDefinition(trigger);
			if (pendingRewardDefinition == null)
			{
				logger.Error("Failed to get the pending reward definition");
				Close();
				return;
			}
			if (pendingRewardDefinition.transactions.Count < 2)
			{
				logger.Error("Unable to set up Mystery Minion Coming Modal: definition incomplete");
				Close();
				return;
			}
			SetUpFirstTransaction(pendingRewardDefinition);
			SetUpSecondTransaction(pendingRewardDefinition);
			playSFXSignal.Dispatch("Play_menu_popUp_01");
			moveAudioListenerSignal.Dispatch(false, view.MinionSlot.transform);
		}

		private PendingRewardDefinition GetPendingRewardDefinition(TriggerInstance triggerInstance)
		{
			if (triggerInstance == null)
			{
				return null;
			}
			IList<TriggerRewardDefinition> rewards = triggerInstance.Definition.rewards;
			if (rewards == null || rewards.Count == 0)
			{
				return null;
			}
			TriggerRewardDefinition triggerRewardDefinition = rewards[0];
			CaptainTeaseTriggerRewardDefinition captainTeaseTriggerRewardDefinition = triggerRewardDefinition as CaptainTeaseTriggerRewardDefinition;
			if (captainTeaseTriggerRewardDefinition == null)
			{
				return null;
			}
			return definitionService.Get<PendingRewardDefinition>(captainTeaseTriggerRewardDefinition.PendingRewardDefinitionID);
		}

		private void SetUpFirstTransaction(PendingRewardDefinition prDef)
		{
			trans1 = definitionService.Get<TransactionDefinition>(prDef.transactions[0]);
			view.SetUpRewardIconDisplayable(view.choice1_icon1, definitionService.Get<DisplayableDefinition>(trans1.Outputs[0].ID), view.choice1_icon1_amt, trans1.Outputs[0].Quantity);
			view.SetUpRewardIconDisplayable(view.choice1_icon2, definitionService.Get<DisplayableDefinition>(trans1.Outputs[1].ID), view.choice1_icon2_amt, trans1.Outputs[1].Quantity);
		}

		private void SetUpSecondTransaction(PendingRewardDefinition prDef)
		{
			trans2 = definitionService.Get<TransactionDefinition>(prDef.transactions[1]);
			view.SetUpRewardIconDisplayable(view.choice2_icon1, definitionService.Get<DisplayableDefinition>(trans2.Outputs[0].ID), view.choice2_icon1_amt, trans2.Outputs[0].Quantity);
			view.SetUpRewardIconDisplayable(view.choice2_icon2, definitionService.Get<DisplayableDefinition>(trans2.Outputs[1].ID), view.choice2_icon2_amt, trans2.Outputs[1].Quantity);
		}

		private void Clicked_1()
		{
			view.PlayerSelectedFirstReward(true);
			rewardIndex = 0;
		}

		private void Clicked_2()
		{
			view.PlayerSelectedFirstReward(false);
			rewardIndex = 1;
		}

		private void Clicked_Confirm()
		{
			TransactionDefinition transactionDefinition = ((rewardIndex != 0) ? trans2 : trans1);
			playerService.RunEntireTransaction(transactionDefinition, TransactionTarget.NO_VISUAL, null);
			updateStoreButtonsSignal.Dispatch(false);
			List<KampaiImage> list = new List<KampaiImage>();
			list.Add((rewardIndex != 0) ? view.choice2_icon1 : view.choice1_icon1);
			list.Add((rewardIndex != 0) ? view.choice2_icon2 : view.choice1_icon2);
			DooberUtil.CheckForTween(transactionDefinition, list, true, uiCamera, tweenSignal, definitionService);
			SendBuildingTelemetry(transactionDefinition);
			logger.Debug(string.Format("Player selected reward choice {0}", rewardIndex + 1));
			Close();
		}

		private void SendBuildingTelemetry(TransactionDefinition transactionDefinition)
		{
			for (int i = 0; i < transactionDefinition.Outputs.Count; i++)
			{
				QuantityItem quantityItem = transactionDefinition.Outputs[i];
				BuildingDefinition definition;
				definitionService.TryGet<BuildingDefinition>(quantityItem.ID, out definition);
				if (definition != null)
				{
					for (int j = 0; j < quantityItem.Quantity; j++)
					{
						buildMenuService.CompleteBuildMenuUpdate(definition.Type, definition.ID);
						telemetryService.Send_Telemetry_EVT_USER_ACQUIRES_BUILDING(definition.TaxonomyType, quantityItem.ID, 0);
					}
				}
			}
		}

		protected void Close()
		{
			moveAudioListenerSignal.Dispatch(true, null);
			playSFXSignal.Dispatch("Play_menu_disappear_01");
			view.Close();
		}

		public override void OnRegister()
		{
			base.OnRegister();
			view.OnMenuClose.AddListener(OnMenuClose);
			view.choice1Button.ClickedSignal.AddListener(Clicked_1);
			view.choice2Button.ClickedSignal.AddListener(Clicked_2);
			view.confirmButton.ClickedSignal.AddListener(Clicked_Confirm);
			view.PulseSelectButtons();
		}

		public override void OnRemove()
		{
			base.OnRemove();
			CleanupListeners();
		}

		private void CleanupListeners()
		{
			view.OnMenuClose.RemoveListener(OnMenuClose);
			view.choice1Button.ClickedSignal.RemoveListener(Clicked_1);
			view.choice2Button.ClickedSignal.RemoveListener(Clicked_2);
			view.confirmButton.ClickedSignal.RemoveListener(Clicked_Confirm);
		}

		private void OnMenuClose()
		{
			if (trigger != null && trigger.Definition.rewards.Count > 0)
			{
				gameContext.injectionBinder.GetInstance<RewardTriggerSignal>().Dispatch(trigger, trigger.Definition.rewards[0]);
			}
			view.Release();
			hideSkrimSignal.Dispatch("TSMTeaseSkrim");
			guiService.Execute(GUIOperation.Unload, "screen_MysteryMinionTeaserSelectionModal");
		}
	}
}
