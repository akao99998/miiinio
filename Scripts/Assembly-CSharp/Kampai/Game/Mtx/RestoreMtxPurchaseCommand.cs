using System;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game.Mtx
{
	public class RestoreMtxPurchaseCommand : Command, IDisposable
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("RestoreMtxPurchaseCommand") as IKampaiLogger;

		private NimbleBridge_NotificationListener mtxRestorePurchaseListener;

		private bool _isDisposed;

		[Inject]
		public ICurrencyService currencyService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject(UIElement.CONTEXT)]
		public ICrossContextCapable uiContext { get; set; }

		[Inject]
		public PopupMessageSignal popupMessageSignal { get; set; }

		public override void Execute()
		{
			DisposedCheck();
			mtxRestorePurchaseListener = new NimbleBridge_NotificationListener(OnMTXRestorePurchases);
			NimbleBridge_NotificationCenter.RegisterListener("nimble.notification.mtx.restorepurchasedtransactionsfinished", mtxRestorePurchaseListener);
			currencyService.RestorePurchases();
		}

		private void OnMTXRestorePurchases(string name, Dictionary<string, object> userData, NimbleBridge_NotificationListener listener)
		{
			if (!name.Equals("nimble.notification.mtx.restorepurchasedtransactionsfinished"))
			{
				return;
			}
			NimbleBridge_NotificationCenter.UnregisterListener(mtxRestorePurchaseListener);
			if (!userData.ContainsKey("result"))
			{
				popupMessageSignal.Dispatch(localService.GetString("RestorePurchasesFail"), PopupMessageType.NORMAL);
				return;
			}
			if (!"1".Equals(userData["result"]))
			{
				popupMessageSignal.Dispatch(localService.GetString("RestorePurchasesFail"), PopupMessageType.NORMAL);
				return;
			}
			IList<string> list = new List<string>(playerService.GetMTXPurchaseTracking());
			NimbleBridge_MTXTransaction[] purchasedTransactions = NimbleBridge_MTX.GetComponent().GetPurchasedTransactions();
			NimbleBridge_MTXTransaction[] array = purchasedTransactions;
			foreach (NimbleBridge_MTXTransaction nimbleBridge_MTXTransaction in array)
			{
				using (NimbleBridge_Error nimbleBridge_Error = nimbleBridge_MTXTransaction.GetError())
				{
					if (nimbleBridge_Error != null && !nimbleBridge_Error.IsNull())
					{
						logger.Warning("Skipping an invalid transaction: {0}", nimbleBridge_MTXTransaction.GetItemSku());
						continue;
					}
				}
				if (nimbleBridge_MTXTransaction.GetTransactionType() != NimbleBridge_MTXTransaction.Type.RESTORE)
				{
					logger.Warning("Skipping a transaction that is not type RESTORE: {0}", nimbleBridge_MTXTransaction.GetItemSku());
					continue;
				}
				string itemSku = nimbleBridge_MTXTransaction.GetItemSku();
				if (list.Contains(itemSku))
				{
					list.Remove(itemSku);
					continue;
				}
				List<PackDefinition> all = definitionService.GetAll<PackDefinition>();
				foreach (PackDefinition item in all)
				{
					if (item.PlatformStoreSku == null)
					{
						continue;
					}
					foreach (PlatformStoreSkuDefinition item2 in item.PlatformStoreSku)
					{
						if (ItemUtil.CompareSKU(itemSku, item2.appleAppstore))
						{
							logger.Info("Restoring purchase: {0}", nimbleBridge_MTXTransaction.GetItemSku());
							KampaiPendingTransaction kampaiPendingTransaction = new KampaiPendingTransaction();
							kampaiPendingTransaction.ExternalIdentifier = itemSku;
							kampaiPendingTransaction.StoreItemDefinitionId = item.ID;
							kampaiPendingTransaction.TransactionInstance = item.TransactionDefinition;
							kampaiPendingTransaction.UTCTimeCreated = timeService.CurrentTime();
							playerService.QueuePendingTransaction(kampaiPendingTransaction);
							uiContext.injectionBinder.GetInstance<FinishPremiumPurchaseSignal>().Dispatch(itemSku);
							break;
						}
					}
				}
			}
			popupMessageSignal.Dispatch(localService.GetString("RestorePurchasesSuccess"), PopupMessageType.NORMAL);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool fromDispose)
		{
			if (fromDispose)
			{
				DisposedCheck();
				mtxRestorePurchaseListener.Dispose();
			}
			_isDisposed = true;
		}

		private void DisposedCheck()
		{
			if (_isDisposed)
			{
				throw new ObjectDisposedException(ToString());
			}
		}

		~RestoreMtxPurchaseCommand()
		{
			Dispose(false);
		}
	}
}
