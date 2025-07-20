using System;
using System.Runtime.InteropServices;

public class NimbleBridge_Network
{
	public enum NimbleBridge_Network_Status
	{
		STATUS_UNKNOWN = 0,
		STATUS_NONE = 1,
		STATUS_DEAD = 2,
		STATUS_OK = 3
	}

	private NimbleBridge_Network()
	{
	}

	[DllImport("NimbleCInterface")]
	private static extern NimbleBridge_NetworkConnectionHandle NimbleBridge_Network_sendGetRequest(string url, BridgeNetworkConnectionCallback callback, IntPtr callbackData);

	[DllImport("NimbleCInterface")]
	private static extern NimbleBridge_NetworkConnectionHandle NimbleBridge_Network_sendPostRequest(string url, IntPtr data, BridgeNetworkConnectionCallback callback, IntPtr callbackData);

	[DllImport("NimbleCInterface")]
	private static extern NimbleBridge_NetworkConnectionHandle NimbleBridge_Network_sendRequest(NimbleBridge_HttpRequest request, BridgeNetworkConnectionCallback callback, IntPtr callbackData);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_Network_forceRedetectNetworkStatus();

	[DllImport("NimbleCInterface")]
	private static extern int NimbleBridge_Network_getNetworkStatus();

	[DllImport("NimbleCInterface")]
	private static extern bool NimbleBridge_Network_isNetworkWifi();

	public static NimbleBridge_Network GetComponent()
	{
		return new NimbleBridge_Network();
	}

	public NimbleBridge_NetworkConnectionHandle SendGetRequest(string url, NetworkConnectionCallback callback)
	{
		IntPtr callbackData = NimbleBridge_CallbackHelper.Get().MakeCallbackData(callback);
		return NimbleBridge_Network_sendGetRequest(url, NimbleBridge_NetworkConnectionHandle.OnNetworkConnectionCallback, callbackData);
	}

	public NimbleBridge_NetworkConnectionHandle SendPostRequest(string url, byte[] data, NetworkConnectionCallback callback)
	{
		IntPtr intPtr = IntPtr.Zero;
		try
		{
			intPtr = MarshalUtility.ConvertDataToPtr(data);
			IntPtr callbackData = NimbleBridge_CallbackHelper.Get().MakeCallbackData(callback);
			return NimbleBridge_Network_sendPostRequest(url, intPtr, NimbleBridge_NetworkConnectionHandle.OnNetworkConnectionCallback, callbackData);
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				MarshalUtility.DisposeDataPtr(intPtr);
			}
		}
	}

	public NimbleBridge_NetworkConnectionHandle SendRequest(NimbleBridge_HttpRequest request, NetworkConnectionCallback callback)
	{
		IntPtr callbackData = NimbleBridge_CallbackHelper.Get().MakeCallbackData(callback);
		return NimbleBridge_Network_sendRequest(request, NimbleBridge_NetworkConnectionHandle.OnNetworkConnectionCallback, callbackData);
	}

	public void ForceRedetectNetworkStatus()
	{
		NimbleBridge_Network_forceRedetectNetworkStatus();
	}

	public NimbleBridge_Network_Status GetNetworkStatus()
	{
		return (NimbleBridge_Network_Status)NimbleBridge_Network_getNetworkStatus();
	}

	public bool IsNetworkWifi()
	{
		return NimbleBridge_Network_isNetworkWifi();
	}
}
