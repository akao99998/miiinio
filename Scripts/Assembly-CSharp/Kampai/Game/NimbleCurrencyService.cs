using System;
using System.Collections.Generic;
using Kampai.Common;
using Kampai.Game.Mtx;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;

namespace Kampai.Game
{
	public class NimbleCurrencyService : CurrencyService, IDisposable
	{
		private enum State
		{
			Idle = 0,
			CatalogRefresh = 1,
			TransactionPending = 2
		}

		private enum TransactionPendingState
		{
			Purchasing = 0,
			Recovering = 1,
			WaitingForDefferedTransactionApprovement = 2,
			ReceiptValidation = 3,
			PlayerDataSaving = 4,
			TransactionConsuming = 5,
			Finalizing = 6
		}

		private interface TransactionPendingStateData
		{
			TransactionPendingState State { get; }
		}

		private sealed class PlayerDataSavingStateData : TransactionPendingStateData
		{
			public const SaveLocation REMOTE_SAVE_LOCATION = SaveLocation.REMOTE;

			public TransactionPendingState State
			{
				get
				{
					return TransactionPendingState.PlayerDataSaving;
				}
			}

			public string NimbleTransactionId { get; private set; }

			public DateTime SaveTimestamp { get; private set; }

			public bool SaveImmediatelly { get; private set; }

			public PlayerDataSavingStateData(string savingId)
			{
				NimbleTransactionId = savingId;
				SaveImmediatelly = false;
			}
		}

		private sealed class DeferredCallbackEntry
		{
			public DeferredCallback callback { get; set; }

			public string tag { get; set; }
		}

		private delegate void DeferredCallback(NimbleCurrencyService ncs);

		private const string MTX_INFO_KEY_CURRENCY = "localCurrency";

		private const string ORDER_ID_KEY = "orderId";

		private const string PURCHASE_DATA_KEY = "purchaseData";

		private const string PURCHASE_STATE_KEY = "purchaseState";

		private const string PURCHASE_TIME_KEY = "purchaseTime";

		private const string TOKEN_KEY = "token";

		private const string AMAZON_UID_KEY = "amazonUid";

		private static string TAG = "[NCS] ";

		private static NimbleCurrencyService sInstance;

		private static State s_CurrentState;

		private Action OnIdleState;

		private int PurchaseStartTimeStamp;

		private static TransactionPendingState s_CurrentTransactionPendingState;

		private TransactionPendingStateData transactionPendingStateData;

		private static bool transactionHandlingPaused;

		private static Queue<DeferredCallbackEntry> s_deferredCallbacks = new Queue<DeferredCallbackEntry>();

		private NimbleBridge_NotificationListener m_mtxCatalogRefreshListener;

		private NimbleBridge_NotificationListener m_mtxTransactionsRecoveredListener;

		private bool _disposed;

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IMtxReceiptValidationService receiptValidationService { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public PremiumCurrencyCatalogUpdatedSignal premiumCurrencyCatalogUpdatedSignal { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistanceService { get; set; }

		[Inject]
		public ProcessNextPendingTransactionSignal processNextPendingTransactionSignal { get; set; }

		[Inject]
		public DisplayRedemptionConfirmationSignal displayRedemptionConfirmationSignal { get; set; }

		[Inject]
		public SavePlayerSignal savePlayerSignal { get; set; }

		[Inject]
		public PlayerSavedSignal playerSavedSignal { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public AwardLevelSignal awardLevelSignal { get; set; }

		[Inject]
		public ReInitializeGameSignal reInitializeGameSignal { get; set; }

		private State CurrentState
		{
			get
			{
				return s_CurrentState;
			}
			set
			{
				logger.Debug("{0}CurrentState switched: {1}->{2}", TAG, s_CurrentState, value);
				s_CurrentState = value;
				if (s_CurrentState == State.Idle && OnIdleState != null)
				{
					Action onIdleState = OnIdleState;
					OnIdleState = null;
					onIdleState();
				}
				if (s_CurrentState != State.TransactionPending)
				{
					transactionPendingStateData = null;
				}
			}
		}

		private TransactionPendingState CurrentTransactionPendingState
		{
			get
			{
				if (CurrentState != State.TransactionPending)
				{
					logger.Error("{0}CurrentTransactionPendingState getter: illegal read access to sub state of TransactionPending state. Current state: {1}", TAG, CurrentStateString());
				}
				return s_CurrentTransactionPendingState;
			}
		}

		private bool TransactionHandlingPaused
		{
			get
			{
				return transactionHandlingPaused;
			}
			set
			{
				logger.Debug("{0}TransactionHandlingPaused flag set: {1}->{2}", TAG, transactionHandlingPaused, value);
				transactionHandlingPaused = value;
			}
		}

		public NimbleCurrencyService()
		{
			if (sInstance != null)
			{
				UnregisterNimbleListeners(sInstance);
			}
			sInstance = this;
		}

		private bool IfIdleSwitchStateTo(State newState, string callerTag)
		{
			if (CurrentState != 0)
			{
				logger.Error("{0}{1} IfIdleSwitchStateTo(): skip setting of {2} state because current state is {3}, not Idle", TAG, callerTag, newState, CurrentStateString());
				return false;
			}
			CurrentState = newState;
			return true;
		}

		private void SetIdleStateIfCurrentIs(State expectedState)
		{
			if (CurrentState == expectedState)
			{
				CurrentState = State.Idle;
				return;
			}
			logger.Error("{0}SetIdleStateIfCurrentIs(): skip setting of Idle state because expected({1}) does not match to current ({2})", TAG, expectedState, CurrentStateString());
		}

		private void SetTransactionPendingState(TransactionPendingState state, TransactionPendingStateData data = null)
		{
			if (CurrentState != State.TransactionPending)
			{
				logger.Error("{0}SetTransactionPendingState: illegal write access to sub state of TransactionPending state. Current state: {1}", TAG, CurrentStateString());
			}
			logger.Debug("{0}CurrentTransactionPendingState switched: {1}->{2}", TAG, s_CurrentTransactionPendingState, state);
			s_CurrentTransactionPendingState = state;
			transactionPendingStateData = data;
		}

		private string CurrentStateString()
		{
			State currentState = CurrentState;
			if (currentState == State.TransactionPending)
			{
				return string.Format("{0},{1}", CurrentState, CurrentTransactionPendingState);
			}
			return CurrentState.ToString();
		}

		private bool NeedDeferCallback()
		{
			if (TransactionHandlingPaused)
			{
				logger.Debug("{0}NeedDeferCallback(): defer reason: transaction handing is paused", TAG);
				return true;
			}
			return false;
		}

		private static void DeferCallbackInvocation(DeferredCallback callback, string tag)
		{
			s_deferredCallbacks.Enqueue(new DeferredCallbackEntry
			{
				callback = callback,
				tag = tag
			});
		}

		private void ProcessDeferredCallbacks()
		{
			Queue<DeferredCallbackEntry> queue = new Queue<DeferredCallbackEntry>(s_deferredCallbacks);
			logger.Debug("{0}ProcessDeferredCallbacks(): clear s_deferredCallbacks list before processing", TAG);
			s_deferredCallbacks.Clear();
			logger.Debug("{0}ProcessDeferredCallbacks(): deferred callbacks count {1}", TAG, queue.Count);
			while (queue.Count > 0)
			{
				DeferredCallbackEntry deferredCallbackEntry = queue.Dequeue();
				logger.Debug("{0}ProcessDeferredCallbacks(): Executing deferred callback {1}", TAG, deferredCallbackEntry.tag);
				deferredCallbackEntry.callback(this);
			}
			if (s_deferredCallbacks.Count <= 0)
			{
				return;
			}
			logger.Error("{0}ProcessDeferredCallbacks(): Unexpected deferred callbacks count {1}. It possibly can cause hang of transaction handling.", TAG, s_deferredCallbacks.Count);
			foreach (DeferredCallbackEntry s_deferredCallback in s_deferredCallbacks)
			{
				logger.Error("{0}ProcessDeferredCallbacks(): Unexpected deferred callback {1}", TAG, s_deferredCallback.tag);
			}
		}

		private bool CurrentInstance()
		{
			return sInstance == this;
		}

		[PostConstruct]
		public void PostConstruct()
		{
			CheckDisposed();
			logger.Debug("{0}PostConstruct() current state: {1}, TransactionHandlingPaused {2}", TAG, CurrentStateString(), TransactionHandlingPaused);
			localPersistanceService.PutData("MtxPurchaseInProgress", "False");
			if (CurrentState == State.TransactionPending)
			{
				switch (CurrentTransactionPendingState)
				{
				case TransactionPendingState.ReceiptValidation:
					SetIdleStateIfCurrentIs(State.TransactionPending);
					break;
				case TransactionPendingState.PlayerDataSaving:
					SetIdleStateIfCurrentIs(State.TransactionPending);
					break;
				}
			}
			if (CurrentState == State.Idle)
			{
				List<NimbleBridge_MTXCatalogItem> availableCatalogItems = GetAvailableCatalogItems();
				if (availableCatalogItems == null || availableCatalogItems.Count == 0)
				{
					RefreshAvailableCatalogItems();
				}
			}
			reInitializeGameSignal.AddListener(OnGameReinitialize);
			awardLevelSignal.AddListener(OnAwardLevel);
		}

		private void OnGameReinitialize(string levelToLoad)
		{
			reInitializeGameSignal.RemoveListener(OnGameReinitialize);
			awardLevelSignal.RemoveListener(OnAwardLevel);
		}

		private void OnAwardLevel(TransactionDefinition td)
		{
			if (base.playerService.GetQuantity(StaticItem.LEVEL_ID) == 1)
			{
				logger.Debug("{0}OnAwardLevel(): resume pending transactions", TAG);
				ResumePendingTransactions();
			}
			else
			{
				logger.Debug("{0}OnAwardLevel(): no-op", TAG);
			}
		}

		public override string GetPriceWithCurrencyAndFormat(string SKU)
		{
			CheckDisposed();
			using (NimbleBridge_MTXCatalogItem nimbleBridge_MTXCatalogItem = GetCatalogItem(SKU))
			{
				if (nimbleBridge_MTXCatalogItem != null)
				{
					string text = nimbleBridge_MTXCatalogItem.GetPriceWithCurrencyAndFormat();
					if (text != null)
					{
						if (text.Contains("₺"))
						{
							text = text.Replace("₺", string.Empty) + " TL";
						}
						return text;
					}
					logger.Error("{0}GetPriceWithCurrencyAndFormat(SKU): price not found for sku {1}", TAG, SKU);
				}
			}
			logger.Warning("{0}GetPriceWithCurrencyAndFormat(SKU): Item is null, so price was not found for SKU {1}", TAG, SKU);
			return localService.GetString("StoreBuy");
		}

		public override void RequestPurchase(KampaiPendingTransaction item)
		{
			CheckDisposed();
			string externalIdentifier = item.ExternalIdentifier;
			logger.Debug("{0}PurchaseItem() sku {1}: current state {2}", TAG, externalIdentifier, CurrentStateString());
			if (!IfIdleSwitchStateTo(State.TransactionPending, "RequestPurchase()"))
			{
				OnPurchaseError(externalIdentifier, uint.MaxValue);
				return;
			}
			SetTransactionPendingState(TransactionPendingState.Purchasing);
			MtxBookendTelemetryInfo mtxBookendTelemetryInfo = new MtxBookendTelemetryInfo();
			mtxBookendTelemetryInfo.productId = externalIdentifier;
			mtxBookendTelemetryInfo.purchaseStage = MTXPurchaseStage.Initiate;
			mtxBookendTelemetryInfo.timeToComplete = 0;
			PurchaseStartTimeStamp = timeService.CurrentTime();
			telemetryService.Send_Telemetry_EVT_MTX_BOOKEND_EVENT(mtxBookendTelemetryInfo);
			using (NimbleBridge_Error nimbleBridge_Error = NimbleBridge_MTX.GetComponent().PurchaseItem(externalIdentifier, OnUnverifiedReceiptReceived, OnPurchaseComplete))
			{
				if (nimbleBridge_Error != null && !nimbleBridge_Error.IsNull())
				{
					logger.Error("{0}PurchaseItem(): NimbleBridge_MTX.PurchaseItem error: code {1}, cause {2}", TAG, nimbleBridge_Error.GetCode(), nimbleBridge_Error.GetCause());
					SetIdleStateIfCurrentIs(State.TransactionPending);
					if (nimbleBridge_Error.GetCode() == (NimbleBridge_Error.Code)20008u)
					{
						logger.Info("{0}PurchaseItem(): resume recover transaction on TRANSACTION_PENDING error", TAG);
						ResumeRecoveredTransaction();
					}
					else
					{
						OnPurchaseError(externalIdentifier, (uint)nimbleBridge_Error.GetCode());
					}
				}
				else
				{
					localPersistanceService.PutData("MtxPurchaseInProgress", "True");
				}
			}
		}

		public override void PauseTransactionsHandling()
		{
			CheckDisposed();
			logger.Debug("{0}PauseTransactionsHandling(): pause transaction handling", TAG);
			TransactionHandlingPaused = true;
		}

		public override void ResumeTransactionsHandling()
		{
			CheckDisposed();
			TransactionHandlingPaused = false;
			logger.Debug("{0}ResumeTransactionsHandling()", TAG);
			if (CurrentState == State.CatalogRefresh)
			{
				logger.Debug("{0}ResumeTransactionsHandling(): wait for catalog refresh update before resuming.", TAG);
				OnIdleState = delegate
				{
					ResumePendingTransactions();
				};
			}
			else
			{
				logger.Debug("{0}ResumeTransactionsHandling(): resume pending transactions", TAG);
				ResumePendingTransactions();
			}
		}

		private NimbleBridge_MTXCatalogItem GetCatalogItem(string sku)
		{
			List<NimbleBridge_MTXCatalogItem> catalogItems = GetCatalogItems();
			return catalogItems.Find((NimbleBridge_MTXCatalogItem item) => item.GetSku().Equals(sku));
		}

		private List<NimbleBridge_MTXCatalogItem> GetCatalogItems()
		{
			List<NimbleBridge_MTXCatalogItem> availableCatalogItems = GetAvailableCatalogItems();
			if (availableCatalogItems == null || availableCatalogItems.Count == 0)
			{
				RefreshAvailableCatalogItems();
				return new List<NimbleBridge_MTXCatalogItem>();
			}
			return availableCatalogItems;
		}

		private static List<NimbleBridge_MTXCatalogItem> RemoveAndroidTestPurchases(NimbleBridge_MTXCatalogItem[] items)
		{
			if (items == null)
			{
				return null;
			}
			List<NimbleBridge_MTXCatalogItem> list = new List<NimbleBridge_MTXCatalogItem>(items.Length);
			foreach (NimbleBridge_MTXCatalogItem nimbleBridge_MTXCatalogItem in items)
			{
				if (!nimbleBridge_MTXCatalogItem.GetSku().StartsWith("android.test"))
				{
					list.Add(nimbleBridge_MTXCatalogItem);
				}
			}
			return list;
		}

		private List<NimbleBridge_MTXCatalogItem> GetAvailableCatalogItems()
		{
			NimbleBridge_MTXCatalogItem[] availableCatalogItems = NimbleBridge_MTX.GetComponent().GetAvailableCatalogItems();
			return RemoveAndroidTestPurchases(availableCatalogItems);
		}

		public override void RefreshCatalog()
		{
			CheckDisposed();
			RefreshAvailableCatalogItems();
		}

		private void RefreshAvailableCatalogItems()
		{
			if (IfIdleSwitchStateTo(State.CatalogRefresh, "RefreshAvailableCatalogItems()"))
			{
				logger.Debug("{0}RefreshAvailableCatalogItems()", TAG);
				if (m_mtxCatalogRefreshListener == null)
				{
					RegisterCatalogRefreshListener();
				}
				NimbleBridge_MTX.GetComponent().RefreshAvailableCatalogItems();
			}
		}

		private void RegisterCatalogRefreshListener()
		{
			logger.Debug("{0}RegisterCatalogRefreshListener(): register REFRESH_CATALOG_FINISHED listener", TAG);
			m_mtxCatalogRefreshListener = new NimbleBridge_NotificationListener(OnMTXCatalogRefreshed);
			NimbleBridge_NotificationCenter.RegisterListener("nimble.notification.mtx.refreshcatalogfinished", m_mtxCatalogRefreshListener);
		}

		private void OnMTXCatalogRefreshed(string name, Dictionary<string, object> userData, NimbleBridge_NotificationListener listener)
		{
			if (!CurrentInstance())
			{
				logger.Debug("{0}OnMTXCatalogRefreshed(): callback called for old instance, redirect to new instance.", TAG);
				sInstance.OnMTXCatalogRefreshed(name, userData, null);
				return;
			}
			if (NeedDeferCallback())
			{
				logger.Debug("{0}OnMTXCatalogRefreshed(): defer callback because transaction handling is paused", TAG);
				DeferCallbackInvocation(delegate(NimbleCurrencyService ncs)
				{
					ncs.OnMTXCatalogRefreshed(name, userData, null);
				}, "OnMTXCatalogRefreshed");
				return;
			}
			logger.Debug("{0}OnMTXCatalogRefreshed(): name {1}", TAG, name);
			if (name == "nimble.notification.mtx.refreshcatalogfinished")
			{
				SetIdleStateIfCurrentIs(State.CatalogRefresh);
				if (userData != null)
				{
					if ((string)userData["result"] == "1")
					{
						logger.Debug("{0}OnMTXCatalogRefreshed(): catalog items have been refreshed", TAG);
						premiumCurrencyCatalogUpdatedSignal.Dispatch();
					}
					else
					{
						logger.Error("{0}OnMTXCatalogRefreshed(): Failed to refresh catalog items", TAG);
					}
				}
			}
			else
			{
				logger.Error("{0}OnMTXCatalogRefreshed(): unexpected event {1}", TAG, name);
			}
		}

		private void OnUnverifiedReceiptReceived(NimbleBridge_MTXTransaction transaction)
		{
			if (!CurrentInstance())
			{
				logger.Debug("{0}OnUnverifiedReceiptReceived(): callback called for old instance, redirect to new instance.", TAG);
				sInstance.OnUnverifiedReceiptReceived(transaction);
			}
			else if (NeedDeferCallback())
			{
				logger.Debug("{0}OnUnverifiedReceiptReceived(): defer callback because transaction handling is paused", TAG);
				DeferCallbackInvocation(delegate(NimbleCurrencyService ncs)
				{
					ncs.OnUnverifiedReceiptReceived(transaction);
				}, "OnUnverifiedReceiptReceived");
			}
			else
			{
				logger.Debug("{0}OnUnverifiedReceiptReceived() transaction for sku {1}", TAG, transaction.GetItemSku());
			}
		}

		private void OnPurchaseComplete(NimbleBridge_MTXTransaction transaction)
		{
			logger.Debug("{0}OnPurchaseComplete() transaction for sku {1}. State: {2}", TAG, transaction.GetItemSku(), CurrentStateString());
			if (!CurrentInstance())
			{
				logger.Debug("{0}OnPurchaseComplete(): callback called for old instance, redirect to new instance.", TAG);
				sInstance.OnPurchaseComplete(transaction);
				return;
			}
			if (NeedDeferCallback())
			{
				logger.Debug("{0}OnPurchaseComplete(): defer callback because transaction handling is paused", TAG);
				DeferCallbackInvocation(delegate(NimbleCurrencyService ncs)
				{
					ncs.OnPurchaseComplete(transaction);
				}, "OnPurchaseComplete");
				return;
			}
			bool flag = false;
			bool flag2 = false;
			using (NimbleBridge_Error nimbleBridge_Error = transaction.GetError())
			{
				if (nimbleBridge_Error != null && !nimbleBridge_Error.IsNull())
				{
					LogMtxError(nimbleBridge_Error, "OnPurchaseComplete()");
					if (TransactionCanceled(transaction))
					{
						logger.Debug("{0}OnPurchaseComplete() canceled transaction for sku {1}", TAG, transaction.GetItemSku());
						flag = true;
					}
					else if (nimbleBridge_Error.GetCode() == (NimbleBridge_Error.Code)20018u)
					{
						logger.Debug("{0}OnPurchaseComplete() ask-to-buy used for sku {1}", TAG, transaction.GetItemSku());
						flag2 = true;
					}
				}
			}
			IMtxReceipt mtxReceipt = ((!flag && !flag2) ? GetReceipt(transaction) : null);
			if (flag2)
			{
				SetTransactionPendingState(TransactionPendingState.WaitingForDefferedTransactionApprovement);
				PurchaseDeferredCallback(transaction.GetItemSku());
			}
			else if (mtxReceipt != null)
			{
				logger.Debug("{0}OnPurchaseComplete() receipt exists, check if user already has the item. Sku: {1}", TAG, transaction.GetItemSku());
				string platformStoreTransactionId = GetPlatformStoreTransactionId(transaction);
				if (!HandleDuplicateTransaction(transaction.GetItemSku(), transaction.GetTransactionId(), platformStoreTransactionId, "OnPurchaseComplete()"))
				{
					logger.Debug("{0}OnPurchaseComplete() receipt exists, start validation. Sku: {1}", TAG, transaction.GetItemSku());
					logger.Debug("{0}OnPurchaseComplete() receipt {1}", TAG, mtxReceipt);
					ValidateReceipt(transaction, mtxReceipt);
				}
			}
			else
			{
				logger.Debug("{0}OnPurchaseComplete() no receipt, start tr-n finalization.  Sku: {1}", TAG, transaction.GetItemSku());
				FinalizeTransaction(transaction.GetTransactionId());
			}
			telemetryService.SendInAppPurchaseEventOnPurchaseComplete(GetIapTelemetryEvent(transaction));
			MtxBookendTelemetryInfo mtxBookendTelemetryInfo = new MtxBookendTelemetryInfo();
			mtxBookendTelemetryInfo.productId = transaction.GetItemSku();
			NimbleBridge_Error error = transaction.GetError();
			if (error != null && !error.IsNull() && error.GetCode() != 0)
			{
				mtxBookendTelemetryInfo.purchaseStage = MTXPurchaseStage.Complete_Fail;
				mtxBookendTelemetryInfo.failedReason = error.GetCode().ToString();
			}
			else
			{
				mtxBookendTelemetryInfo.purchaseStage = MTXPurchaseStage.Complete_Success;
			}
			mtxBookendTelemetryInfo.timeToComplete = timeService.CurrentTime() - PurchaseStartTimeStamp;
			telemetryService.Send_Telemetry_EVT_MTX_BOOKEND_EVENT(mtxBookendTelemetryInfo);
		}

		private bool HandleDuplicateTransaction(string sku, string nimbleTransactionId, string platformStoreTransactionId, string callerTag)
		{
			if (base.playerService.PlayerAlreadyHasPlatformStoreTransactionID(platformStoreTransactionId))
			{
				logger.Debug("{0}{1}: cancel duplicate transaction.", TAG, callerTag);
				PurchaseCanceledCallback(sku, 30001u);
				logger.Debug("{0}{1} Consuming duplicate transaction. Sku: {2}, nimble tr-n Id {3}, platform store tr-n Id {4}", TAG, callerTag, sku, nimbleTransactionId, platformStoreTransactionId);
				OnMtxTransactionFulfilment(nimbleTransactionId);
				return true;
			}
			return false;
		}

		private static bool TransactionCanceled(NimbleBridge_MTXTransaction transaction)
		{
			using (NimbleBridge_Error nimbleBridge_Error = transaction.GetError())
			{
				if (nimbleBridge_Error != null && !nimbleBridge_Error.IsNull())
				{
					return nimbleBridge_Error.GetCode() == (NimbleBridge_Error.Code)20003u;
				}
			}
			return false;
		}

		private IMtxReceipt GetReceipt(NimbleBridge_MTXTransaction transaction)
		{
			string receipt = transaction.GetReceipt();
			object value;
			if (transaction.GetAdditionalInfoDictionary().TryGetValue("purchaseData", out value) && value != null)
			{
				string text = value.ToString();
				if (!string.IsNullOrEmpty(text))
				{
					GooglePlayReceipt googlePlayReceipt = new GooglePlayReceipt();
					googlePlayReceipt.signedData = text;
					googlePlayReceipt.signature = receipt;
					return googlePlayReceipt;
				}
			}
			return null;
		}

		private void ValidateReceipt(NimbleBridge_MTXTransaction transaction, IMtxReceipt receipt)
		{
			receiptValidationService.AddPendingReceipt(transaction.GetItemSku(), transaction.GetTransactionId(), GetPlatformStoreTransactionId(transaction), receipt);
			SetTransactionPendingState(TransactionPendingState.ReceiptValidation);
			receiptValidationService.ValidatePendingReceipt();
		}

		private string GetPlatformStoreTransactionId(NimbleBridge_MTXTransaction transaction)
		{
			string text = string.Empty;
			Dictionary<string, object> additionalInfoDictionary = transaction.GetAdditionalInfoDictionary();
			object value;
			if (additionalInfoDictionary.TryGetValue("orderId", out value) && value != null)
			{
				string text2 = value.ToString();
				if (!string.IsNullOrEmpty(text2))
				{
					text = text2;
				}
			}
			object value2;
			if (string.IsNullOrEmpty(text) && additionalInfoDictionary.TryGetValue("token", out value2) && value2 != null)
			{
				text = value2.ToString();
			}
			return text;
		}

		public override void ReceiptValidationCallback(ReceiptValidationResult result)
		{
			CheckDisposed();
			logger.Debug("{0}ReceiptValidationCallback() for sku {1}, result code {2}, state: {3}", TAG, result.sku, result.code, CurrentStateString());
			if (!CurrentInstance())
			{
				logger.Debug("{0}OnPurchaseComplete(): callback called for old instance, redirect to new instance.", TAG);
				sInstance.ReceiptValidationCallback(result);
				return;
			}
			if (NeedDeferCallback())
			{
				logger.Debug("{0}OnPurchaseComplete(): defer callback because transaction handling is paused", TAG);
				DeferCallbackInvocation(delegate(NimbleCurrencyService ncs)
				{
					ncs.ReceiptValidationCallback(result);
				}, "ReceiptValidationCallback");
				return;
			}
			switch (result.code)
			{
			case ReceiptValidationResult.Code.SUCCESS:
			case ReceiptValidationResult.Code.RECEIPT_DUPLICATE:
				logger.Debug("{0}ReceiptValidationCallback(): result code: {1}", TAG, result.code);
				if (HandleDuplicateTransaction(result.sku, result.nimbleTransactionId, result.platformStoreTransactionId, "ReceiptValidationCallback()"))
				{
					break;
				}
				logger.Debug("{0}ReceiptValidationCallback(): granting purchased product to user.", TAG);
				if (definitionService.getSKUSalePackType(result.sku).Equals(SalePackType.Redeemable))
				{
					logger.Info("{0}ReceiptValidationCallback(): granting redeemable item");
					base.playerService.addPendingRemption(result);
					if (LoadState.Get() == LoadStateType.STARTED)
					{
						displayRedemptionConfirmationSignal.Dispatch();
					}
				}
				else
				{
					finalizeReceiptValidation(result);
				}
				break;
			case ReceiptValidationResult.Code.RECEIPT_INVALID:
				OnReceiptValidationError(result.nimbleTransactionId);
				OnPurchaseError(result.sku, 30000u);
				break;
			case ReceiptValidationResult.Code.VALIDATION_UNAVAILABLE:
				break;
			}
		}

		private void finalizeReceiptValidation(ReceiptValidationResult result)
		{
			logger.Info("{0}finalizeReceiptValidation(): pendingRedemption.sku = {1}", TAG, result.sku);
			PurchaseSucceededAndValidatedCallback(result.sku);
			base.playerService.AddPlatformStoreTransactionID(result.platformStoreTransactionId);
			logger.Debug("{0}ReceiptValidationCallback(): saving updated player data.", TAG);
			SavePlayerData(result);
		}

		private void SavePlayerData(ReceiptValidationResult result)
		{
			logger.Debug("{0}SavePlayerData(): state {1}", TAG, CurrentStateString());
			PlayerDataSavingStateData playerDataSavingStateData = new PlayerDataSavingStateData(result.nimbleTransactionId);
			SetTransactionPendingState(TransactionPendingState.PlayerDataSaving, playerDataSavingStateData);
			playerSavedSignal.AddListener(OnPlayerSaved);
			savePlayerSignal.Dispatch(new Tuple<SaveLocation, string, bool>(SaveLocation.REMOTE, playerDataSavingStateData.NimbleTransactionId, playerDataSavingStateData.SaveImmediatelly));
		}

		private void OnPlayerSaved(SaveLocation saveLocation, string saveID, DateTime saveTimestampUTC, bool success)
		{
			if (saveLocation != 0)
			{
				logger.Debug("{0}OnPlayerSaved(): ignore non remote player saving", TAG);
				return;
			}
			logger.Debug("{0}OnPlayerSaved(): saveID: {1}, state {2}, success: {3}", TAG, saveID, CurrentStateString(), success);
			PlayerDataSavingStateData playerDataSavingStateData = transactionPendingStateData as PlayerDataSavingStateData;
			if (playerDataSavingStateData == null)
			{
				logger.Error("{0}OnPlayerSaved(): saveID: {1}, state {2}. Current PlayerDataSavingStateData is null", TAG, saveID, CurrentStateString());
				playerSavedSignal.RemoveListener(OnPlayerSaved);
			}
			else if (saveTimestampUTC < playerDataSavingStateData.SaveTimestamp)
			{
				logger.Debug("{0}OnPlayerSaved(): ignore saving that occured before product has been delivered to user(save ID {1}, saveTimestampUTC {2}, expected timestamp {3})", TAG, saveID, saveTimestampUTC, playerDataSavingStateData.SaveTimestamp);
			}
			else if (success)
			{
				logger.Debug("{0}OnPlayerSaved(): assume that player data have been saved, consume tr-n, nimble tr-n id: {1}", TAG, playerDataSavingStateData.NimbleTransactionId);
				playerSavedSignal.RemoveListener(OnPlayerSaved);
				OnMtxTransactionFulfilment(playerDataSavingStateData.NimbleTransactionId);
			}
			else
			{
				logger.Debug("{0}OnPlayerSaved(): saving failed, waiting for next save attempt.");
			}
		}

		private void OnPurchaseError(string sku, uint errorCode)
		{
			logger.Debug("{0}OnPurchaseError(): sku: {1}, state {2}, errorCode: {3}", TAG, sku, CurrentStateString(), errorCode);
			PurchaseCanceledCallback(sku, errorCode);
		}

		private void OnMtxTransactionFulfilment(string mtxTransactionId)
		{
			logger.Debug("{0}OnMtxTransactionFulfilment(): mtxTransactionId {1}, state {2}", TAG, mtxTransactionId, CurrentStateString());
			SetTransactionPendingState(TransactionPendingState.TransactionConsuming);
			using (NimbleBridge_Error nimbleBridge_Error = NimbleBridge_MTX.GetComponent().ItemGranted(mtxTransactionId, NimbleBridge_MTXCatalogItem.Type.CONSUMABLE, ItemGrantedCallback))
			{
				if (nimbleBridge_Error != null && !nimbleBridge_Error.IsNull())
				{
					logger.Error("{0}OnMtxTransactionFulfilment(): ItemGranted() error {1} - {2}", TAG, nimbleBridge_Error.GetCode(), nimbleBridge_Error.GetReason());
					FinalizeTransaction(mtxTransactionId);
				}
			}
			receiptValidationService.RemovePendingReceipt(mtxTransactionId);
		}

		private void OnReceiptValidationError(string mtxTransactionId)
		{
			FinalizeTransaction(mtxTransactionId);
			receiptValidationService.RemovePendingReceipt(mtxTransactionId);
		}

		private void ItemGrantedCallback(NimbleBridge_MTXTransaction transaction)
		{
			if (!CurrentInstance())
			{
				logger.Debug("{0}ItemGrantedCallback(): callback called for old instance, redirect to new instance.", TAG);
				sInstance.ItemGrantedCallback(transaction);
				return;
			}
			if (NeedDeferCallback())
			{
				logger.Debug("{0}ItemGrantedCallback(): defer callback because transaction handling is paused", TAG);
				DeferCallbackInvocation(delegate(NimbleCurrencyService ncs)
				{
					ncs.ItemGrantedCallback(transaction);
				}, "ItemGrantedCallback");
				return;
			}
			FinalizeTransaction(transaction.GetTransactionId());
			MtxBookendTelemetryInfo mtxBookendTelemetryInfo = new MtxBookendTelemetryInfo();
			mtxBookendTelemetryInfo.productId = transaction.GetItemSku();
			NimbleBridge_Error error = transaction.GetError();
			if (error != null && !error.IsNull() && error.GetCode() != 0)
			{
				mtxBookendTelemetryInfo.purchaseStage = MTXPurchaseStage.Complete_ItemsAwarded;
				mtxBookendTelemetryInfo.failedReason = error.GetCode().ToString();
			}
			else
			{
				mtxBookendTelemetryInfo.purchaseStage = MTXPurchaseStage.Complete_ItemsAwarded;
			}
			mtxBookendTelemetryInfo.timeToComplete = timeService.CurrentTime() - PurchaseStartTimeStamp;
			telemetryService.Send_Telemetry_EVT_MTX_BOOKEND_EVENT(mtxBookendTelemetryInfo);
		}

		private void FinalizeTransaction(string transactionId)
		{
			logger.Debug("{0}FinalizeTransaction(): finalizing transaction, id = {1}, state {2}", TAG, transactionId, CurrentStateString());
			SetTransactionPendingState(TransactionPendingState.Finalizing);
			using (NimbleBridge_Error nimbleBridge_Error = NimbleBridge_MTX.GetComponent().FinalizeTransaction(transactionId, FinalizeCallback))
			{
				if (nimbleBridge_Error != null && !nimbleBridge_Error.IsNull() && nimbleBridge_Error.GetCode() != 0)
				{
					logger.Debug("FinalizeTransaction(): FinalizeTransaction through NimbleBridge_MTX failed with error {0}", nimbleBridge_Error.GetCode().ToString());
				}
			}
		}

		private void FinalizeCallback(NimbleBridge_MTXTransaction transaction)
		{
			logger.Debug("{0}FinalizeCallback(): transaction finalized, id = {1}, state {2}", TAG, transaction.GetTransactionId(), CurrentStateString());
			if (!CurrentInstance())
			{
				logger.Debug("{0}FinalizeCallback(): callback called for old instance, redirect to new instance.", TAG);
				sInstance.FinalizeCallback(transaction);
				return;
			}
			if (NeedDeferCallback())
			{
				logger.Debug("{0}FinalizeCallback(): defer callback because transaction handling is paused", TAG);
				DeferCallbackInvocation(delegate(NimbleCurrencyService ncs)
				{
					ncs.FinalizeCallback(transaction);
				}, "FinalizeCallback");
				return;
			}
			bool flag = false;
			using (NimbleBridge_Error nimbleBridge_Error = transaction.GetError())
			{
				if (nimbleBridge_Error != null && !nimbleBridge_Error.IsNull())
				{
					LogMtxError(nimbleBridge_Error, "FinalizeCallback()");
					if (TransactionCanceled(transaction))
					{
						logger.Debug("{0}FinalizeCallback() canceled transaction for sku {1}", TAG, transaction.GetItemSku());
						PurchaseCanceledCallback(transaction.GetItemSku(), (uint)nimbleBridge_Error.GetCode());
					}
					else if (nimbleBridge_Error.GetCode() == (NimbleBridge_Error.Code)20001u)
					{
						logger.Debug("{0}FinalizeCallback() do not notify client about ITEM_ALREADY_OWNED error(try to resume transaction), sku {1}", TAG, transaction.GetItemSku());
						flag = true;
						RestorePurchases();
					}
					else
					{
						logger.Error("{0}FinalizeCallback() notify client code about tr-n error", TAG);
						OnPurchaseError(transaction.GetItemSku(), (uint)nimbleBridge_Error.GetCode());
					}
				}
			}
			localPersistanceService.PutData("MtxPurchaseInProgress", "False");
			if (flag)
			{
				if (CurrentState != 0)
				{
					OnIdleState = HandleItemAlreadyOwnedError;
				}
				else
				{
					HandleItemAlreadyOwnedError();
				}
			}
			SetIdleStateIfCurrentIs(State.TransactionPending);
		}

		private void HandleItemAlreadyOwnedError()
		{
			logger.Debug("{0}HandleItemAlreadyOwnedError()...", TAG);
			if (CurrentState != 0)
			{
				logger.Error("{0}HandleItemAlreadyOwnedError(): unexpecte state {1}, expected Idle", TAG, CurrentStateString());
			}
			if (!ResumeRecoveredTransaction())
			{
				logger.Debug("{0}HandleItemAlreadyOwnedError(): Google Play specific: try to restore transactions to proceed after ItemAlreadyOwned error", TAG);
				RestorePurchases();
			}
			logger.Debug("{0}...HandleItemAlreadyOwnedError()", TAG);
		}

		private void ResumePendingTransactions()
		{
			logger.Debug("{0}ResumePendingTransactions()...: state: {1}", TAG, CurrentStateString());
			if (!TransactionProcessingEnabled())
			{
				logger.Debug("{0}ResumePendingTransactions(): skip transaction processing based on player level: {1}", TAG, base.playerService.GetQuantity(StaticItem.LEVEL_ID));
				return;
			}
			if (s_deferredCallbacks.Count > 0)
			{
				logger.Debug("{0}ResumePendingTransactions(): process deferred callbacks", TAG);
				ProcessDeferredCallbacks();
			}
			logger.Debug("{0}ResumePendingTransactions() after deferred callbacks processed: state: {1}", TAG, CurrentStateString());
			if (CurrentState == State.TransactionPending && CurrentTransactionPendingState == TransactionPendingState.PlayerDataSaving)
			{
				logger.Debug("{0}ResumePendingTransactions(): ignore, wait for player data saving: {1}", TAG, CurrentStateString());
			}
			else if (receiptValidationService.HasPendingReceipts())
			{
				logger.Debug("{0}ResumePendingTransactions(): proceed validation of pending receipts", TAG);
				if (CurrentState != State.TransactionPending && !IfIdleSwitchStateTo(State.TransactionPending, "ResumePendingTransactions"))
				{
					logger.Debug("{0}ResumePendingTransactions(): skip resuming, current state: {1}", TAG, CurrentStateString());
					return;
				}
				SetTransactionPendingState(TransactionPendingState.ReceiptValidation);
				receiptValidationService.ValidatePendingReceipt();
			}
			else if (!ResumeRecoveredTransaction())
			{
				if (CurrentState != 0)
				{
					logger.Debug("{0}ResumePendingTransactions(): waiting for NCS to settle", TAG);
					OnIdleState = delegate
					{
						ResumePendingTransactions();
					};
					return;
				}
				logger.Debug("{0}ResumePendingTransactions(): start to handle Kampai pending transactions. State: {1}", TAG, CurrentStateString());
				processNextPendingTransactionSignal.Dispatch();
				if (CurrentState != 0)
				{
					logger.Debug("{0}ResumePendingTransactions(): processing pending Kampai trns", TAG);
					OnIdleState = delegate
					{
						ResumePendingTransactions();
					};
				}
			}
			logger.Debug("{0}...ResumePendingTransactions(): state: {1}", TAG, CurrentStateString());
		}

		private bool ResumeRecoveredTransaction()
		{
			logger.Debug("{0}ResumeRecoveredTransaction(): state {1}", TAG, CurrentStateString());
			if (!TransactionProcessingEnabled())
			{
				logger.Debug("{0}ResumeRecoveredTransaction(): skip transaction processing based on player level: {1}", TAG, base.playerService.GetQuantity(StaticItem.LEVEL_ID));
				return false;
			}
			if (m_mtxTransactionsRecoveredListener == null)
			{
				RegisterTransactionRecoveredListener();
			}
			NimbleBridge_MTXTransaction[] recoveredTransactions = NimbleBridge_MTX.GetComponent().GetRecoveredTransactions();
			logger.Debug("{0}ResumeRecoveredTransaction(): GetRecoveredTransactions().Length: {1}", TAG, recoveredTransactions.Length);
			if (recoveredTransactions.Length == 0)
			{
				logger.Debug("{0}ResumeRecoveredTransaction(): Nothing to do, no recovered transactions.", TAG);
				return false;
			}
			if (!IfIdleSwitchStateTo(State.TransactionPending, "ResumeRecoveredTransaction()"))
			{
				OnIdleState = delegate
				{
					ResumePendingTransactions();
				};
				return true;
			}
			SetTransactionPendingState(TransactionPendingState.Recovering);
			ResumeFirstRecoveredTransaction(recoveredTransactions[0]);
			return true;
		}

		private void RegisterTransactionRecoveredListener()
		{
			logger.Debug("{0}RegisterTransactionRecoveringListener: register TRANSACTIONS_RECOVERED listener", TAG);
			m_mtxTransactionsRecoveredListener = new NimbleBridge_NotificationListener(OnMTXTransactionsRecovered);
			NimbleBridge_NotificationCenter.RegisterListener("nimble.notification.mtx.transactionsrecovered", m_mtxTransactionsRecoveredListener);
		}

		private void OnMTXTransactionsRecovered(string name, Dictionary<string, object> userData, NimbleBridge_NotificationListener listener)
		{
			if (name == "nimble.notification.mtx.transactionsrecovered")
			{
				logger.Debug("{0}OnMTXTransactionsRecovered(): resuming recovered transactions", TAG);
				ResumeRecoveredTransaction();
			}
			else
			{
				logger.Error("{0}OnMTXTransactionsRecovered(): unexpected event {1}", TAG, name);
			}
		}

		public override void CollectRedemption(ReceiptValidationResult pendingRedemption)
		{
			logger.Info("{0}CollectRedemption(): pendingRedemption.sku = {1}", TAG, pendingRedemption.sku);
			CheckDisposed();
			finalizeReceiptValidation(pendingRedemption);
		}

		public override void RestorePurchases()
		{
			logger.Debug("{0}RestorePurchases()", TAG);
			NimbleBridge_MTX.GetComponent().RestorePurchasedTransactions();
		}

		private void ResumeFirstRecoveredTransaction(NimbleBridge_MTXTransaction transaction)
		{
			logger.Debug("{0}ResumeFirstRecoveredTransaction(): recovered tr-n: sku {1}, tr-n id: {2}, state: {3}", TAG, transaction.GetItemSku(), transaction.GetTransactionId(), transaction.GetTransactionState());
			OnIdleState = delegate
			{
				ResumePendingTransactions();
			};
			bool flag = false;
			if (transaction.GetTransactionState() == NimbleBridge_MTXTransaction.State.WAITING_FOR_PLATFORM_RESPONSE)
			{
				flag = true;
			}
			using (NimbleBridge_Error nimbleBridge_Error = NimbleBridge_MTX.GetComponent().ResumeTransaction(transaction.GetTransactionId(), OnUnverifiedReceiptReceived, OnPurchaseComplete, ItemGrantedCallback, FinalizeCallback))
			{
				if (nimbleBridge_Error != null && !nimbleBridge_Error.IsNull())
				{
					LogMtxError(nimbleBridge_Error, "ResumeFirstRecoveredTransaction()");
					SetIdleStateIfCurrentIs(State.TransactionPending);
				}
			}
			if (flag)
			{
				logger.Debug("{0}ResumeFirstRecoveredTransaction(): Google Play specific: force call RestorePurchasedTransactions on WAITING_FOR_PLATFORM_RESPONSE tr-n status to properly resume transaction", TAG);
				RestorePurchases();
			}
		}

		private void LogMtxError(NimbleBridge_Error error, string callerTag)
		{
			if (error != null)
			{
				if (error.IsNull())
				{
					return;
				}
				logger.Error("{0}{1}: Nimble error {2} - {3}", TAG, callerTag, error.GetCode(), error.GetReason());
				using (NimbleBridge_Error nimbleBridge_Error = error.GetCause())
				{
					if (nimbleBridge_Error != null && !nimbleBridge_Error.IsNull())
					{
						logger.Error("{0}{1}: Nimble error cause {2} - {3}", TAG, callerTag, nimbleBridge_Error.GetCode(), nimbleBridge_Error.GetReason());
					}
					else
					{
						logger.Error("{0}{1}: Nimble error cause is null", TAG, callerTag);
					}
					return;
				}
			}
			logger.Error("{0}{1}: error is null", TAG, callerTag);
		}

		private static void UnregisterNimbleListeners(NimbleCurrencyService instance)
		{
			if (instance.m_mtxCatalogRefreshListener != null)
			{
				NimbleBridge_NotificationCenter.UnregisterListener(instance.m_mtxCatalogRefreshListener);
				instance.m_mtxCatalogRefreshListener = null;
			}
			if (instance.m_mtxTransactionsRecoveredListener != null)
			{
				NimbleBridge_NotificationCenter.UnregisterListener(instance.m_mtxTransactionsRecoveredListener);
				instance.m_mtxTransactionsRecoveredListener = null;
			}
		}

		private IapTelemetryEvent GetIapTelemetryEvent(NimbleBridge_MTXTransaction transaction)
		{
			IapTelemetryEvent iapTelemetryEvent = new IapTelemetryEvent();
			using (NimbleBridge_Error nimbleBridge_Error = transaction.GetError())
			{
				if (nimbleBridge_Error != null && !nimbleBridge_Error.IsNull())
				{
					iapTelemetryEvent.nimbleMtxErrorCode = (uint)nimbleBridge_Error.GetCode();
				}
			}
			iapTelemetryEvent.productId = transaction.GetItemSku();
			iapTelemetryEvent.productPrice = transaction.GetPriceDecimal();
			Dictionary<string, object> additionalInfoDictionary = transaction.GetAdditionalInfoDictionary();
			object value;
			if (additionalInfoDictionary.TryGetValue("localCurrency", out value) && value != null)
			{
				iapTelemetryEvent.currency = value.ToString();
			}
			else
			{
				iapTelemetryEvent.currency = string.Empty;
			}
			GooglePlayReceipt googlePlayReceipt = GetReceipt(transaction) as GooglePlayReceipt;
			if (googlePlayReceipt != null)
			{
				iapTelemetryEvent.googlePurchaseData = googlePlayReceipt.signedData;
				iapTelemetryEvent.googleDataSignature = googlePlayReceipt.signature;
			}
			iapTelemetryEvent.googleOrderId = GetGooglePlayOrderId(transaction);
			return iapTelemetryEvent;
		}

		private string GetGooglePlayOrderId(NimbleBridge_MTXTransaction transaction)
		{
			object value;
			if (transaction.GetAdditionalInfoDictionary().TryGetValue("orderId", out value) && value != null)
			{
				return value.ToString();
			}
			return string.Empty;
		}

		private void CheckDisposed()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(typeof(NimbleCurrencyService).ToString());
			}
		}

		protected virtual void Dispose(bool fromDispose)
		{
			if (fromDispose)
			{
				if (m_mtxCatalogRefreshListener != null)
				{
					m_mtxCatalogRefreshListener.Dispose();
				}
				if (m_mtxTransactionsRecoveredListener != null)
				{
					m_mtxTransactionsRecoveredListener.Dispose();
				}
			}
		}

		public void Dispose()
		{
			CheckDisposed();
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~NimbleCurrencyService()
		{
			Dispose(false);
		}
	}
}
