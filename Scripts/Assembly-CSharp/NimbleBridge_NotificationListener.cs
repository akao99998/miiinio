using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using SimpleJSON;

public class NimbleBridge_NotificationListener : SafeHandle
{
	private delegate void BridgeNotificationCallback(string name, string userData, IntPtr callbackDataPtr);

	private NotificationCallback m_callback;

	private IntPtr m_dataPtr;

	internal IntPtr DataPtr
	{
		get
		{
			return m_dataPtr;
		}
	}

	public override bool IsInvalid
	{
		get
		{
			return base.IsClosed || handle == IntPtr.Zero;
		}
	}

	private NimbleBridge_NotificationListener()
		: base(IntPtr.Zero, true)
	{
	}

	private NimbleBridge_NotificationListener(IntPtr handle)
		: this()
	{
		SetHandle(handle);
	}

	public NimbleBridge_NotificationListener(NotificationCallback callback)
		: this()
	{
		m_callback = callback;
	}

	[MonoPInvokeCallback(typeof(BridgeNotificationCallback))]
	internal static void OnNotificationCallback(string name, string userData, IntPtr callbackDataPtr)
	{
		JSONNode jSONNode = JSON.Parse(userData);
		Dictionary<string, object> userDataDict = MarshalUtility.ConvertJsonToDictionary((JSONClass)jSONNode);
		NimbleBridge_CallbackHelper nimbleBridge_CallbackHelper = NimbleBridge_CallbackHelper.Get();
		NimbleBridge_NotificationListener listener = (NimbleBridge_NotificationListener)nimbleBridge_CallbackHelper.GetData(callbackDataPtr);
		nimbleBridge_CallbackHelper.RunOnMainThread(delegate
		{
			listener.m_callback(name, userDataDict, listener);
		});
	}

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_NotificationListener_Dispose(NimbleBridge_NotificationListener listenerWrapper);

	[DllImport("NimbleCInterface")]
	private static extern IntPtr NimbleBridge_NotificationListener_NotificationListener(BridgeNotificationCallback callback, IntPtr callbackData);

	protected override bool ReleaseHandle()
	{
		NimbleBridge_NotificationListener_Dispose(this);
		return true;
	}

	internal void EnsureInitialized()
	{
		if (m_dataPtr == IntPtr.Zero)
		{
			m_dataPtr = NimbleBridge_CallbackHelper.Get().MakeCallbackData(this);
			SetHandle(NimbleBridge_NotificationListener_NotificationListener(OnNotificationCallback, m_dataPtr));
		}
	}

	internal void Unregister()
	{
		if (m_dataPtr != IntPtr.Zero)
		{
			NimbleBridge_NotificationListener_Dispose(this);
			m_dataPtr = IntPtr.Zero;
			SetHandleAsInvalid();
		}
	}
}
