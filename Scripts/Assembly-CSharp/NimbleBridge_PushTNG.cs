using System;
using System.Runtime.InteropServices;
using AOT;

public class NimbleBridge_PushTNG
{
	public enum DisabledReason
	{
		OPT_OUT = 0,
		GAME_SERVER = 1,
		REGISTER_FAILURE = 2
	}

	public delegate void OnRegistrationSuccessDelegate(int statusCode, string registrationToken);

	public delegate void OnConnectionErrorDelegate(int statusCode, string errorMessage);

	public delegate void OnTrackingSuccessDelegate(int statusCode, string trackingData);

	public delegate void OnGetInAppSuccessDelegate(int statusCode, string inAppNotificationData);

	private delegate void BridgeOnRegistrationSuccessDelegate(int statusCode, string registrationToken, IntPtr callbackData);

	private delegate void BridgeOnConnectionErrorDelegate(int statusCode, string errorMessage, IntPtr callbackData);

	private delegate void BridgeOnTrackingSuccessDelegate(int statusCode, string trackingData, IntPtr callbackData);

	private delegate void BridgeOnGetInAppSuccessDelegate(int statusCode, string inAppNotificationData, IntPtr callbackData);

	private NimbleBridge_PushTNG()
	{
	}

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_PushTNG_start(string userAlias, double dateOfBirth, bool sandbox, BridgeOnRegistrationSuccessDelegate registrationSuccessDelegate, IntPtr registrationSuccessData, BridgeOnConnectionErrorDelegate connectionErrorDelegate, IntPtr connectionErrorData, BridgeOnTrackingSuccessDelegate trackingSuccessDelegate, IntPtr trackingSuccessData, BridgeOnGetInAppSuccessDelegate getInAppSuccessDelegate, IntPtr getInAppSuccessData);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_PushTNG_startDisabled(string userAlias, double dateOfBirth, int disabledReason, bool sandbox, BridgeOnRegistrationSuccessDelegate registrationSuccessDelegate, IntPtr registrationSuccessData, BridgeOnConnectionErrorDelegate connectionErrorDelegate, IntPtr connectionErrorData, BridgeOnTrackingSuccessDelegate trackingSuccessDelegate, IntPtr trackingSuccessData, BridgeOnGetInAppSuccessDelegate getInAppSuccessDelegate, IntPtr getInAppSuccessData);

	[DllImport("NimbleCInterface")]
	private static extern bool NimbleBridge_PushTNG_getRegistrationStatus();

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_PushTNG_getDisableStatus();

	[MonoPInvokeCallback(typeof(BridgeOnRegistrationSuccessDelegate))]
	private static void OnRegistrationSuccess(int statusCode, string registrationToken, IntPtr callbackData)
	{
		NimbleBridge_CallbackHelper nimbleBridge_CallbackHelper = NimbleBridge_CallbackHelper.Get();
		OnRegistrationSuccessDelegate callback = (OnRegistrationSuccessDelegate)nimbleBridge_CallbackHelper.GetData(callbackData);
		if (callback != null)
		{
			nimbleBridge_CallbackHelper.RunOnMainThread(delegate
			{
				callback(statusCode, registrationToken);
			});
		}
	}

	[MonoPInvokeCallback(typeof(BridgeOnConnectionErrorDelegate))]
	private static void OnConnectionError(int statusCode, string errorMessage, IntPtr callbackData)
	{
		NimbleBridge_CallbackHelper nimbleBridge_CallbackHelper = NimbleBridge_CallbackHelper.Get();
		OnConnectionErrorDelegate callback = (OnConnectionErrorDelegate)nimbleBridge_CallbackHelper.GetData(callbackData);
		if (callback != null)
		{
			nimbleBridge_CallbackHelper.RunOnMainThread(delegate
			{
				callback(statusCode, errorMessage);
			});
		}
	}

	[MonoPInvokeCallback(typeof(BridgeOnTrackingSuccessDelegate))]
	private static void OnTrackingSuccess(int statusCode, string trackingData, IntPtr callbackData)
	{
		NimbleBridge_CallbackHelper nimbleBridge_CallbackHelper = NimbleBridge_CallbackHelper.Get();
		OnTrackingSuccessDelegate callback = (OnTrackingSuccessDelegate)nimbleBridge_CallbackHelper.GetData(callbackData);
		if (callback != null)
		{
			nimbleBridge_CallbackHelper.RunOnMainThread(delegate
			{
				callback(statusCode, trackingData);
			});
		}
	}

	[MonoPInvokeCallback(typeof(BridgeOnGetInAppSuccessDelegate))]
	private static void OnGetInAppSuccess(int statusCode, string inAppNotificationData, IntPtr callbackData)
	{
		NimbleBridge_CallbackHelper nimbleBridge_CallbackHelper = NimbleBridge_CallbackHelper.Get();
		OnGetInAppSuccessDelegate callback = (OnGetInAppSuccessDelegate)nimbleBridge_CallbackHelper.GetData(callbackData);
		if (callback != null)
		{
			nimbleBridge_CallbackHelper.RunOnMainThread(delegate
			{
				callback(statusCode, inAppNotificationData);
			});
		}
	}

	public static NimbleBridge_PushTNG GetComponent()
	{
		return new NimbleBridge_PushTNG();
	}

	public void Start(string userAlias, double dateOfBirth, bool sandbox)
	{
		Start(userAlias, dateOfBirth, sandbox, null, null, null, null);
	}

	public void Start(string userAlias, double dateOfBirth, bool sandbox, OnRegistrationSuccessDelegate registrationSuccessDelegate, OnConnectionErrorDelegate connectionErrorDelegate, OnTrackingSuccessDelegate trackingSuccessDelegate, OnGetInAppSuccessDelegate getInAppSuccessDelegate)
	{
		NimbleBridge_CallbackHelper nimbleBridge_CallbackHelper = NimbleBridge_CallbackHelper.Get();
		IntPtr registrationSuccessData = nimbleBridge_CallbackHelper.MakeCallbackData(registrationSuccessDelegate);
		IntPtr connectionErrorData = nimbleBridge_CallbackHelper.MakeCallbackData(connectionErrorDelegate);
		IntPtr trackingSuccessData = nimbleBridge_CallbackHelper.MakeCallbackData(trackingSuccessDelegate);
		IntPtr getInAppSuccessData = nimbleBridge_CallbackHelper.MakeCallbackData(getInAppSuccessDelegate);
		NimbleBridge_PushTNG_start(userAlias, dateOfBirth, sandbox, OnRegistrationSuccess, registrationSuccessData, OnConnectionError, connectionErrorData, OnTrackingSuccess, trackingSuccessData, OnGetInAppSuccess, getInAppSuccessData);
	}

	public void StartDisabled(string userAlias, double dateOfBirth, DisabledReason disabledReason, bool sandbox, OnRegistrationSuccessDelegate registrationSuccessDelegate, OnConnectionErrorDelegate connectionErrorDelegate, OnTrackingSuccessDelegate trackingSuccessDelegate, OnGetInAppSuccessDelegate getInAppSuccessDelegate)
	{
		NimbleBridge_CallbackHelper nimbleBridge_CallbackHelper = NimbleBridge_CallbackHelper.Get();
		IntPtr registrationSuccessData = nimbleBridge_CallbackHelper.MakeCallbackData(registrationSuccessDelegate);
		IntPtr connectionErrorData = nimbleBridge_CallbackHelper.MakeCallbackData(connectionErrorDelegate);
		IntPtr trackingSuccessData = nimbleBridge_CallbackHelper.MakeCallbackData(trackingSuccessDelegate);
		IntPtr getInAppSuccessData = nimbleBridge_CallbackHelper.MakeCallbackData(getInAppSuccessDelegate);
		NimbleBridge_PushTNG_startDisabled(userAlias, dateOfBirth, (int)disabledReason, sandbox, OnRegistrationSuccess, registrationSuccessData, OnConnectionError, connectionErrorData, OnTrackingSuccess, trackingSuccessData, OnGetInAppSuccess, getInAppSuccessData);
	}

	public bool getRegistrationStatus()
	{
		return NimbleBridge_PushTNG_getRegistrationStatus();
	}

	public string getDisableStatus()
	{
		return NimbleBridge_PushTNG_getDisableStatus();
	}
}
