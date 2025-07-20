using System.Collections;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Common.Service.HealthMetrics;
using Kampai.Game.Transaction;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class FinishPremiumPurchaseCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("FinishPremiumPurchaseCommand") as IKampaiLogger;

		[Inject]
		public string ExternalIdentifier { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ICurrencyService currencyService { get; set; }

		[Inject]
		public IClientHealthService clientHealthService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public IPlayerDurationService durationService { get; set; }

		[Inject]
		public SetGrindCurrencySignal setGrindCurrencySignal { get; set; }

		[Inject]
		public SetPremiumCurrencySignal setPremiumCurrencySignal { get; set; }

		[Inject]
		public SetStorageCapacitySignal setStorageSignal { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public UnlockMinionsSignal unlockMinionsSignal { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public PurchaseLandExpansionSignal purchaseSignal { get; set; }

		[Inject]
		public UpdatePlayerDLCTierSignal playerDLCTierSignal { get; set; }

		[Inject]
		public UpdateUIButtonsSignal updateStoreButtonsSignal { get; set; }

		[Inject]
		public OpenStoreHighlightItemSignal openStoreHighlightItemSignal { get; set; }

		[Inject]
		public FinishPurchasingSalePackSignal finishPurchasingSalePackSignal { get; set; }

		[Inject]
		public VillainLairModel villainLairModel { get; set; }

		public override void Execute()
		{
			logger.Debug("[NCS] FinishPremiumPurchaseCommand.Execute()");
			TransactionDefinition reward = GetReward();
			if (reward == null)
			{
				logger.Log(KampaiLogLevel.Error, "{0} unknown SKU", ExternalIdentifier);
				return;
			}
			MarkSalePurchased(reward);
			RecordPurchase(reward);
			if (reward.Inputs != null && reward.Inputs.Count > 0)
			{
				logger.Log(KampaiLogLevel.Error, "Reward contains inputs {0}", reward.ID);
			}
			playerService.TrackMTXPurchase(ExternalIdentifier.Trim().ToLower());
			telemetryService.SendInAppPurchaseEventOnProductDelivery(ExternalIdentifier, reward);
			localPersistService.PutDataPlayer("IsSpender", "true");
			DispatchSignals();
			unlockMinionsSignal.Dispatch();
		}

		private IEnumerator UpdateDLCTier()
		{
			yield return new WaitForSeconds(1f);
			playerDLCTierSignal.Dispatch();
		}

		private void ExpansionTransactionCallback(PendingCurrencyTransaction pct)
		{
			if (!pct.Success)
			{
				return;
			}
			bool flag = false;
			int num = -1;
			foreach (QuantityItem output in pct.GetPendingTransaction().Outputs)
			{
				Definition definition = definitionService.Get<Definition>(output.ID);
				LandExpansionConfig landExpansionConfig = definition as LandExpansionConfig;
				if (landExpansionConfig != null)
				{
					flag = true;
					purchaseSignal.Dispatch(landExpansionConfig.expansionId, true);
					continue;
				}
				BuildingDefinition buildingDefinition = definition as BuildingDefinition;
				if (buildingDefinition != null)
				{
					updateStoreButtonsSignal.Dispatch(false);
					num = buildingDefinition.ID;
				}
			}
			if (flag)
			{
				routineRunner.StartCoroutine(UpdateDLCTier());
			}
			if (num > -1)
			{
				openStoreHighlightItemSignal.Dispatch(num, villainLairModel.currentActiveLair == null);
			}
		}

		private SalePackDefinition getReedemableSalePackDefinition()
		{
			foreach (SalePackDefinition item in definitionService.GetAll<SalePackDefinition>())
			{
				if (CompareSKU(item.SKU) && item.Type == SalePackType.Redeemable)
				{
					return item;
				}
			}
			return null;
		}

		private TransactionDefinition GetReward()
		{
			TransactionDefinition transactionDefinition = null;
			SalePackDefinition reedemableSalePackDefinition = getReedemableSalePackDefinition();
			if (reedemableSalePackDefinition != null)
			{
				logger.Log(KampaiLogLevel.Info, "[NCS] Rewarding Redeemable SKU {0}", reedemableSalePackDefinition.SKU);
				TransactionDefinition transactionDefinition2 = reedemableSalePackDefinition.TransactionDefinition.ToDefinition();
				TransactionArg transactionArg = new TransactionArg();
				transactionArg.IsFromPremiumSource = false;
				playerService.RunEntireTransaction(transactionDefinition2.ID, TransactionTarget.CURRENCY, null, transactionArg);
				transactionDefinition = transactionDefinition2;
			}
			else if (!string.IsNullOrEmpty(ExternalIdentifier))
			{
				KampaiPendingTransaction kampaiPendingTransaction = playerService.ProcessPendingTransaction(ExternalIdentifier, true, ExpansionTransactionCallback);
				if (kampaiPendingTransaction != null)
				{
					transactionDefinition = kampaiPendingTransaction.TransactionInstance.ToDefinition();
				}
			}
			if (transactionDefinition == null)
			{
				logger.Log(KampaiLogLevel.Info, "[NCS] {0} not found in pending transactions, trying to run transaction again", ExternalIdentifier);
				foreach (StoreItemDefinition item in definitionService.GetAll<StoreItemDefinition>())
				{
					PremiumCurrencyItemDefinition definition = null;
					if (definitionService.TryGet<PremiumCurrencyItemDefinition>(item.ReferencedDefID, out definition) && CompareSKU(definition.SKU))
					{
						SalePackDefinition salePackDefinition = definition as SalePackDefinition;
						TransactionDefinition transactionDefinition3 = ((salePackDefinition == null) ? definitionService.Get<TransactionDefinition>(item.TransactionID) : salePackDefinition.TransactionDefinition.ToDefinition());
						TransactionArg transactionArg2 = new TransactionArg();
						transactionArg2.IsFromPremiumSource = true;
						playerService.RunEntireTransaction(transactionDefinition3.ID, TransactionTarget.CURRENCY, null, transactionArg2);
						transactionDefinition = transactionDefinition3;
					}
				}
			}
			return transactionDefinition;
		}

		private void RecordPurchase(TransactionDefinition reward)
		{
			IPurchaseRecorder purchaseRecorder = playerService as IPurchaseRecorder;
			if (purchaseRecorder != null)
			{
				int premiumOutputForTransaction = TransactionUtil.GetPremiumOutputForTransaction(reward);
				int grindOutputForTransaction = TransactionUtil.GetGrindOutputForTransaction(reward);
				if (premiumOutputForTransaction > 0)
				{
					purchaseRecorder.AddPurchasedCurrency(true, (uint)premiumOutputForTransaction);
				}
				if (grindOutputForTransaction > 0)
				{
					purchaseRecorder.AddPurchasedCurrency(false, (uint)grindOutputForTransaction);
				}
				playerService.AlterQuantity(StaticItem.TRANSACTIONS_LIFETIME_COUNT_ID, 1);
				int totalGamePlaySeconds = durationService.TotalGamePlaySeconds;
				playerService.SetQuantity(StaticItem.LAST_GAME_TIME_PURCHASE, (totalGamePlaySeconds > 0) ? totalGamePlaySeconds : 0);
				int num = timeService.CurrentTime();
				playerService.SetQuantity(StaticItem.LAST_CAL_TIME_PURCHASE, (num > 0) ? num : 0);
			}
			else
			{
				logger.Error("Premium purchase occured without a purchase recorder");
			}
		}

		private void DispatchSignals()
		{
			setGrindCurrencySignal.Dispatch();
			setPremiumCurrencySignal.Dispatch();
			setStorageSignal.Dispatch();
			currencyService.CurrencyDialogClosed(true);
			clientHealthService.MarkMeterEvent("AppFlow.Purchase");
		}

		private void MarkSalePurchased(TransactionDefinition reward)
		{
			if (string.IsNullOrEmpty(ExternalIdentifier))
			{
				return;
			}
			IList<PackDefinition> all = definitionService.GetAll<PackDefinition>();
			foreach (PackDefinition item in all)
			{
				if (CompareSKU(item.SKU) && reward.ID == item.TransactionDefinition.ID)
				{
					finishPurchasingSalePackSignal.Dispatch(item.ID);
					break;
				}
			}
		}

		private bool CompareSKU(string SKU)
		{
			return SKU.Trim().ToLower().Equals(ExternalIdentifier.Trim().ToLower());
		}
	}
}
