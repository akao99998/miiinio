using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;

public class NimbleBridge_SynergyRequest : SafeHandle
{
	private delegate void BridgeSynergyRequestPreparingCallback(IntPtr requestPtr, IntPtr callbackDataPtr);

	public override bool IsInvalid
	{
		get
		{
			return base.IsClosed || handle == IntPtr.Zero;
		}
	}

	private NimbleBridge_SynergyRequest()
		: base(IntPtr.Zero, true)
	{
	}

	internal NimbleBridge_SynergyRequest(IntPtr handle)
		: base(IntPtr.Zero, true)
	{
		SetHandle(handle);
	}

	public NimbleBridge_SynergyRequest(string api, NimbleBridge_HttpRequest.Method method, SynergyRequestPreparingCallback callback)
		: this()
	{
		IntPtr callbackData = NimbleBridge_CallbackHelper.Get().MakeCallbackData(callback);
		IntPtr intPtr = NimbleBridge_SynergyRequest_SynergyRequest(api, (int)method, OnSynergyRequestPreparingCallback, callbackData);
		SetHandle(intPtr);
	}

	[MonoPInvokeCallback(typeof(BridgeSynergyRequestPreparingCallback))]
	private static void OnSynergyRequestPreparingCallback(IntPtr requestPtr, IntPtr callbackDataPtr)
	{
		NimbleBridge_CallbackHelper nimbleBridge_CallbackHelper = NimbleBridge_CallbackHelper.Get();
		NimbleBridge_SynergyRequest request = new NimbleBridge_SynergyRequest(requestPtr);
		SynergyRequestPreparingCallback callback = (SynergyRequestPreparingCallback)nimbleBridge_CallbackHelper.GetData(callbackDataPtr);
		nimbleBridge_CallbackHelper.RunOnMainThread(delegate
		{
			callback(request);
		});
	}

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_SynergyRequest_Dispose(NimbleBridge_SynergyRequest synergyRequestWrapper);

	[DllImport("NimbleCInterface")]
	private static extern IntPtr NimbleBridge_SynergyRequest_SynergyRequest(string api, int method, BridgeSynergyRequestPreparingCallback callback, IntPtr callbackData);

	[DllImport("NimbleCInterface")]
	private static extern NimbleBridge_HttpRequest NimbleBridge_SynergyRequest_getHttpRequest(NimbleBridge_SynergyRequest synergyRequestWrapper);

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_SynergyRequest_getBaseUrl(NimbleBridge_SynergyRequest synergyRequestWrapper);

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_SynergyRequest_getApi(NimbleBridge_SynergyRequest synergyRequestWrapper);

	[DllImport("NimbleCInterface")]
	private static extern IntPtr NimbleBridge_SynergyRequest_getUrlParameters(NimbleBridge_SynergyRequest synergyRequestWrapper);

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_SynergyRequest_getJsonData(NimbleBridge_SynergyRequest synergyRequestWrapper);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_SynergyRequest_setHttpRequest(NimbleBridge_SynergyRequest synergyRequestWrapper, NimbleBridge_HttpRequest requestWrapper);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_SynergyRequest_setBaseUrl(NimbleBridge_SynergyRequest synergyRequestWrapper, string url);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_SynergyRequest_setApi(NimbleBridge_SynergyRequest synergyRequestWrapper, string api);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_SynergyRequest_setUrlParameters(NimbleBridge_SynergyRequest synergyRequestWrapper, IntPtr parameters);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_SynergyRequest_setJsonData(NimbleBridge_SynergyRequest synergyRequestWrapper, string jsonData);

	[DllImport("NimbleCInterface")]
	private static extern int NimbleBridge_SynergyRequest_getMethod(NimbleBridge_SynergyRequest synergyRequestWrapper);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_SynergyRequest_setMethod(NimbleBridge_SynergyRequest synergyRequestWrapper, int method);

	[DllImport("NimbleCInterface")]
	private static extern IntPtr NimbleBridge_SynergyRequest_getPrepareRequestCallback(NimbleBridge_SynergyRequest synergyRequestWrapper);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_SynergyRequest_setPrepareRequestCallback(NimbleBridge_SynergyRequest synergyRequestWrapper, BridgeSynergyRequestPreparingCallback callback, IntPtr callbackData);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_SynergyRequest_send(NimbleBridge_SynergyRequest synergyRequestWrapper);

	protected override bool ReleaseHandle()
	{
		NimbleBridge_SynergyRequest_Dispose(this);
		return true;
	}

	public NimbleBridge_HttpRequest GetHttpRequest()
	{
		return NimbleBridge_SynergyRequest_getHttpRequest(this);
	}

	public string GetBaseUrl()
	{
		return NimbleBridge_SynergyRequest_getBaseUrl(this);
	}

	public string GetApi()
	{
		return NimbleBridge_SynergyRequest_getApi(this);
	}

	public Dictionary<string, string> GetUrlParameters()
	{
		IntPtr mapPtr = NimbleBridge_SynergyRequest_getUrlParameters(this);
		return MarshalUtility.ConvertPtrToDictionary(mapPtr);
	}

	public string GetJsonData()
	{
		return NimbleBridge_SynergyRequest_getJsonData(this);
	}

	public void SetHttpRequest(NimbleBridge_HttpRequest request)
	{
		NimbleBridge_SynergyRequest_setHttpRequest(this, request);
	}

	public void SetBaseUrl(string url)
	{
		NimbleBridge_SynergyRequest_setBaseUrl(this, url);
	}

	public void SetApi(string api)
	{
		NimbleBridge_SynergyRequest_setApi(this, api);
	}

	public void SetUrlParameters(Dictionary<string, string> parameters)
	{
		IntPtr intPtr = IntPtr.Zero;
		try
		{
			intPtr = MarshalUtility.ConvertDictionaryToPtr(parameters);
			NimbleBridge_SynergyRequest_setUrlParameters(this, intPtr);
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				MarshalUtility.DisposeMapPtr(intPtr);
			}
		}
	}

	public void SetJsonData(string jsonData)
	{
		NimbleBridge_SynergyRequest_setJsonData(this, jsonData);
	}

	public NimbleBridge_HttpRequest.Method GetMethod()
	{
		return (NimbleBridge_HttpRequest.Method)NimbleBridge_SynergyRequest_getMethod(this);
	}

	public void SetMethod(NimbleBridge_HttpRequest.Method method)
	{
		NimbleBridge_SynergyRequest_setMethod(this, (int)method);
	}

	public SynergyRequestPreparingCallback GetPrepareRequestCallback()
	{
		IntPtr dataPtr = NimbleBridge_SynergyRequest_getPrepareRequestCallback(this);
		return (SynergyRequestPreparingCallback)NimbleBridge_CallbackHelper.Get().GetData(dataPtr);
	}

	public void SetPrepareRequestCallback(SynergyRequestPreparingCallback callback)
	{
		IntPtr callbackData = NimbleBridge_CallbackHelper.Get().MakeCallbackData(callback);
		NimbleBridge_SynergyRequest_setPrepareRequestCallback(this, OnSynergyRequestPreparingCallback, callbackData);
	}

	public void Send()
	{
		NimbleBridge_SynergyRequest_send(this);
	}
}
