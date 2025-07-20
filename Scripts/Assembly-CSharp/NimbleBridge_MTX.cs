using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;

public class NimbleBridge_MTX
{
	private delegate void BridgeMTXTransactionCallback(IntPtr transactionPtr, IntPtr callbackDataPtr);

	private delegate void BridgeMTXRefreshReceiptCallback(IntPtr errorPtr, IntPtr callbackDataPtr);

	public const int ERROR_BILLING_UNAVAILABLE = 10001;

	public const int ERROR_USER_CANCELED = 10002;

	public const int ERROR_ITEM_ALREADY_PURCHASED = 10003;

	public const int ERROR_ITEM_UNAVAILABLE = 10004;

	public const int ERROR_PURCHASE_VERIFICATION_FAILED = 10005;

	public const string NOTIFICATION_REFRESH_CATALOG_FINISHED = "nimble.notification.mtx.refreshcatalogfinished";

	public const string NOTIFICATION_RESTORE_PURCHASED_TRANSACTIONS_FINISHED = "nimble.notification.mtx.restorepurchasedtransactionsfinished";

	public const string NOTIFICATION_TRANSACTIONS_RECOVERED = "nimble.notification.mtx.transactionsrecovered";

	private NimbleBridge_MTX()
	{
	}

	[MonoPInvokeCallback(typeof(BridgeMTXTransactionCallback))]
	private static void OnMTXTransactionCallback(IntPtr transactionPtr, IntPtr callbackDataPtr)
	{
		NimbleBridge_CallbackHelper nimbleBridge_CallbackHelper = NimbleBridge_CallbackHelper.Get();
		NimbleBridge_MTXTransaction transaction = new NimbleBridge_MTXTransaction(transactionPtr);
		MTXTransactionCallback callback = (MTXTransactionCallback)nimbleBridge_CallbackHelper.GetData(callbackDataPtr);
		nimbleBridge_CallbackHelper.RunOnMainThread(delegate
		{
			callback(transaction);
		});
	}

	[MonoPInvokeCallback(typeof(BridgeMTXRefreshReceiptCallback))]
	private static void OnMTXRefreshReceiptCallback(IntPtr errorPtr, IntPtr callbackDataPtr)
	{
		NimbleBridge_CallbackHelper nimbleBridge_CallbackHelper = NimbleBridge_CallbackHelper.Get();
		NimbleBridge_Error error = new NimbleBridge_Error(errorPtr);
		MTXRefreshReceiptCallback callback = (MTXRefreshReceiptCallback)nimbleBridge_CallbackHelper.GetData(callbackDataPtr);
		nimbleBridge_CallbackHelper.RunOnMainThread(delegate
		{
			callback(error);
		});
	}

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_MTX_deleteTransactionArray(IntPtr array);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_MTX_deleteItemArray(IntPtr array);

	[DllImport("NimbleCInterface")]
	private static extern NimbleBridge_Error NimbleBridge_MTX_purchaseItem(string sku, BridgeMTXTransactionCallback receiptCallback, IntPtr receiptCallbackData, BridgeMTXTransactionCallback purchaseCallback, IntPtr purchaseCallbackData);

	[DllImport("NimbleCInterface")]
	private static extern NimbleBridge_Error NimbleBridge_MTX_itemGranted(string transactionId, int itemType, BridgeMTXTransactionCallback callback, IntPtr callbackData);

	[DllImport("NimbleCInterface")]
	private static extern NimbleBridge_Error NimbleBridge_MTX_finalizeTransaction(string transactionId, BridgeMTXTransactionCallback callback, IntPtr callbackData);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_MTX_restorePurchasedTransactions();

	[DllImport("NimbleCInterface")]
	private static extern IntPtr NimbleBridge_MTX_getPurchasedTransactions();

	[DllImport("NimbleCInterface")]
	private static extern IntPtr NimbleBridge_MTX_getPendingTransactions();

	[DllImport("NimbleCInterface")]
	private static extern IntPtr NimbleBridge_MTX_getRecoveredTransactions();

	[DllImport("NimbleCInterface")]
	private static extern NimbleBridge_Error NimbleBridge_MTX_resumeTransaction(string transactionId, BridgeMTXTransactionCallback receiptCallback, IntPtr receiptCallbackData, BridgeMTXTransactionCallback purchaseCallback, IntPtr purchaseCallbackData, BridgeMTXTransactionCallback itemGrantedCallback, IntPtr itemGrantedCallbackData, BridgeMTXTransactionCallback finalizeCallback, IntPtr finalizeCallbackData);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_MTX_refreshAvailableCatalogItems();

	[DllImport("NimbleCInterface")]
	private static extern IntPtr NimbleBridge_MTX_getAvailableCatalogItems();

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_MTX_setPlatformParameters(IntPtr parameters);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_MTXRefreshReceipt(BridgeMTXRefreshReceiptCallback refreshReceiptCallback, IntPtr refreshReceiptCallbackData);

	public static NimbleBridge_MTX GetComponent()
	{
		return new NimbleBridge_MTX();
	}

	public NimbleBridge_Error PurchaseItem(string sku, MTXTransactionCallback receiptCallback, MTXTransactionCallback purchaseCallback)
	{
		IntPtr receiptCallbackData = NimbleBridge_CallbackHelper.Get().MakeCallbackData(receiptCallback);
		IntPtr purchaseCallbackData = NimbleBridge_CallbackHelper.Get().MakeCallbackData(purchaseCallback);
		return NimbleBridge_MTX_purchaseItem(sku, OnMTXTransactionCallback, receiptCallbackData, OnMTXTransactionCallback, purchaseCallbackData);
	}

	public NimbleBridge_Error ItemGranted(string transactionId, NimbleBridge_MTXCatalogItem.Type itemType, MTXTransactionCallback callback)
	{
		IntPtr callbackData = NimbleBridge_CallbackHelper.Get().MakeCallbackData(callback);
		return NimbleBridge_MTX_itemGranted(transactionId, (int)itemType, OnMTXTransactionCallback, callbackData);
	}

	public NimbleBridge_Error FinalizeTransaction(string transactionId, MTXTransactionCallback callback)
	{
		IntPtr callbackData = NimbleBridge_CallbackHelper.Get().MakeCallbackData(callback);
		return NimbleBridge_MTX_finalizeTransaction(transactionId, OnMTXTransactionCallback, callbackData);
	}

	public void RestorePurchasedTransactions()
	{
		NimbleBridge_MTX_restorePurchasedTransactions();
	}

	public NimbleBridge_MTXTransaction[] GetPurchasedTransactions()
	{
		List<NimbleBridge_MTXTransaction> list = new List<NimbleBridge_MTXTransaction>();
		IntPtr intPtr = NimbleBridge_MTX_getPurchasedTransactions();
		IntPtr intPtr2 = Marshal.ReadIntPtr(intPtr);
		int num = 1;
		while (intPtr2 != IntPtr.Zero)
		{
			list.Add(new NimbleBridge_MTXTransaction(intPtr2));
			intPtr2 = Marshal.ReadIntPtr(intPtr, num * Marshal.SizeOf(typeof(IntPtr)));
			num++;
		}
		NimbleBridge_MTX_deleteTransactionArray(intPtr);
		return list.ToArray();
	}

	public NimbleBridge_MTXTransaction[] GetPendingTransactions()
	{
		List<NimbleBridge_MTXTransaction> list = new List<NimbleBridge_MTXTransaction>();
		IntPtr intPtr = NimbleBridge_MTX_getPendingTransactions();
		IntPtr intPtr2 = Marshal.ReadIntPtr(intPtr);
		int num = 1;
		while (intPtr2 != IntPtr.Zero)
		{
			list.Add(new NimbleBridge_MTXTransaction(intPtr2));
			intPtr2 = Marshal.ReadIntPtr(intPtr, num * Marshal.SizeOf(typeof(IntPtr)));
			num++;
		}
		NimbleBridge_MTX_deleteTransactionArray(intPtr);
		return list.ToArray();
	}

	public NimbleBridge_MTXTransaction[] GetRecoveredTransactions()
	{
		List<NimbleBridge_MTXTransaction> list = new List<NimbleBridge_MTXTransaction>();
		IntPtr intPtr = NimbleBridge_MTX_getRecoveredTransactions();
		if (intPtr == IntPtr.Zero)
		{
			return list.ToArray();
		}
		IntPtr intPtr2 = Marshal.ReadIntPtr(intPtr);
		int num = 1;
		while (intPtr2 != IntPtr.Zero)
		{
			list.Add(new NimbleBridge_MTXTransaction(intPtr2));
			intPtr2 = Marshal.ReadIntPtr(intPtr, num * Marshal.SizeOf(typeof(IntPtr)));
			num++;
		}
		NimbleBridge_MTX_deleteTransactionArray(intPtr);
		return list.ToArray();
	}

	public NimbleBridge_Error ResumeTransaction(string transactionId, MTXTransactionCallback receiptCallback, MTXTransactionCallback purchaseCallback, MTXTransactionCallback itemGrantedCallback, MTXTransactionCallback finalizeCallback)
	{
		NimbleBridge_CallbackHelper nimbleBridge_CallbackHelper = NimbleBridge_CallbackHelper.Get();
		IntPtr receiptCallbackData = nimbleBridge_CallbackHelper.MakeCallbackData(receiptCallback);
		IntPtr purchaseCallbackData = nimbleBridge_CallbackHelper.MakeCallbackData(purchaseCallback);
		IntPtr itemGrantedCallbackData = nimbleBridge_CallbackHelper.MakeCallbackData(itemGrantedCallback);
		IntPtr finalizeCallbackData = nimbleBridge_CallbackHelper.MakeCallbackData(finalizeCallback);
		return NimbleBridge_MTX_resumeTransaction(transactionId, OnMTXTransactionCallback, receiptCallbackData, OnMTXTransactionCallback, purchaseCallbackData, OnMTXTransactionCallback, itemGrantedCallbackData, OnMTXTransactionCallback, finalizeCallbackData);
	}

	public void RefreshAvailableCatalogItems()
	{
		NimbleBridge_MTX_refreshAvailableCatalogItems();
	}

	public NimbleBridge_MTXCatalogItem[] GetAvailableCatalogItems()
	{
		List<NimbleBridge_MTXCatalogItem> list = new List<NimbleBridge_MTXCatalogItem>();
		IntPtr intPtr = NimbleBridge_MTX_getAvailableCatalogItems();
		if (intPtr != IntPtr.Zero)
		{
			IntPtr intPtr2 = Marshal.ReadIntPtr(intPtr);
			int num = 1;
			while (intPtr2 != IntPtr.Zero)
			{
				list.Add(new NimbleBridge_MTXCatalogItem(intPtr2));
				intPtr2 = Marshal.ReadIntPtr(intPtr, num * Marshal.SizeOf(typeof(IntPtr)));
				num++;
			}
			NimbleBridge_MTX_deleteItemArray(intPtr);
		}
		return list.ToArray();
	}

	public void SetPlatformParameters(Dictionary<string, string> parameters)
	{
		IntPtr intPtr = IntPtr.Zero;
		try
		{
			intPtr = MarshalUtility.ConvertDictionaryToPtr(parameters);
			NimbleBridge_MTX_setPlatformParameters(intPtr);
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				MarshalUtility.DisposeMapPtr(intPtr);
			}
		}
	}

	public void RefreshReceipt(MTXRefreshReceiptCallback refreshReceiptCallback)
	{
		NimbleBridge_CallbackHelper nimbleBridge_CallbackHelper = NimbleBridge_CallbackHelper.Get();
		IntPtr refreshReceiptCallbackData = nimbleBridge_CallbackHelper.MakeCallbackData(refreshReceiptCallback);
		NimbleBridge_MTXRefreshReceipt(OnMTXRefreshReceiptCallback, refreshReceiptCallbackData);
	}
}
