using Kampai.Common;
using Kampai.Common.Service.Telemetry;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class OrderBoardFillOrderCommand : Command
	{
		[Inject]
		public OrderBoard building { get; set; }

		[Inject]
		public int TicketIndex { get; set; }

		[Inject]
		public TransactionDefinition def { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public OrderBoardSetNewTicketSignal setNewTicketSignal { get; set; }

		[Inject]
		public OrderBoardTransactionFailedSignal transactionFailedSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public SetStorageCapacitySignal setStorageSignal { get; set; }

		[Inject]
		public DisplayPlayerTrainingSignal displayPlayerTrainingSignal { get; set; }

		[Inject]
		public GetBuffStateSignal getBuffStateSignal { get; set; }

		[Inject]
		public ValidateCurrentTriggerSignal validateCurrentTriggerSignal { get; set; }

		[Inject]
		public IPartyFavorAnimationService partyFavorAnimationService { get; set; }

		[Inject]
		public IDefinitionService defService { get; set; }

		[Inject]
		public PickControllerModel pickControllerModel { get; set; }

		public override void Execute()
		{
			int startTime = timeService.CurrentTime();
			getBuffStateSignal.Dispatch(BuffType.CURRENCY, UpdateTransactionCallback);
			playerService.StartTransaction(def, TransactionTarget.BLACKMARKETBOARD, TransactionCallback, new TransactionArg(building.ID), startTime, TicketIndex);
		}

		private void UpdateTransactionCallback(float multiplier)
		{
			foreach (QuantityItem output in def.Outputs)
			{
				if (output.ID == 0)
				{
					output.Quantity = (uint)Mathf.CeilToInt((float)output.Quantity * multiplier);
					break;
				}
			}
		}

		private void TransactionCallback(PendingCurrencyTransaction pct)
		{
			if (pct.Success)
			{
				UpdatePartyFavors();
				soundFXSignal.Dispatch("Play_fill_order_01");
				OrderBoardTicket orderBoardTicket = building.tickets[TicketIndex];
				int characterDefinitionId = orderBoardTicket.CharacterDefinitionId;
				int partyPointsEarned = 0;
				if (orderBoardTicket.TransactionInst != null && orderBoardTicket.TransactionInst.Outputs != null)
				{
					partyPointsEarned = TransactionUtil.GetXPOutputForTransaction(orderBoardTicket.TransactionInst.ToDefinition());
				}
				telemetryService.Send_Telemetry_EVT_GP_ACHIEVEMENTS_CHECKPOINTS_EAL(TicketIndex.ToString(), TelemetryAchievementType.Order, partyPointsEarned, string.Empty);
				telemetryService.Send_TelemetryOrderBoard(true, def, characterDefinitionId);
				GetReward(characterDefinitionId);
				playerService.IncreaseCompletedOrders();
				setNewTicketSignal.Dispatch(-TicketIndex, true);
				validateCurrentTriggerSignal.Dispatch();
			}
			else
			{
				building.HarvestableCharacterDefinitionId = 0;
				transactionFailedSignal.Dispatch(def);
			}
		}

		private void UpdatePartyFavors()
		{
			foreach (QuantityItem input in def.Inputs)
			{
				int partyFavorDefinitionIDByItemID = defService.GetPartyFavorDefinitionIDByItemID(input.ID);
				if (partyFavorDefinitionIDByItemID != -1)
				{
					partyFavorAnimationService.AddAvailablePartyFavorItem(partyFavorDefinitionIDByItemID);
				}
			}
		}

		private void GetReward(int prestigeDefID)
		{
			TransactionArg transactionArg = new TransactionArg(building.ID);
			if (prestigeDefID != 0)
			{
				building.HarvestableCharacterDefinitionId = prestigeDefID;
				Prestige prestige = prestigeService.GetPrestige(prestigeDefID);
				if (prestige != null)
				{
					prestige.CurrentOrdersCompleted++;
					transactionArg.AddAccumulator(prestige);
				}
			}
			if (playerService.FinishTransaction(def, TransactionTarget.BLACKMARKETBOARD, transactionArg))
			{
				questService.UpdateAllQuestsWithQuestStepType(QuestStepType.OrderBoard);
				if (prestigeDefID == 0)
				{
					displayPlayerTrainingSignal.Dispatch(19000006, false, new Signal<bool>());
				}
				else
				{
					Prestige prestige2 = prestigeService.GetPrestige(prestigeDefID);
					if (prestigeService.IsPrestigeFulfilled(prestige2))
					{
						pickControllerModel.SetIgnoreInstance(313, true);
					}
				}
			}
			setStorageSignal.Dispatch();
		}
	}
}
