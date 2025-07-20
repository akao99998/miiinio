using System;
using System.Runtime.InteropServices;
using AOT;

public class NimbleBridge_SynergyNetworkConnectionHandle : SafeHandle
{
	public override bool IsInvalid
	{
		get
		{
			return base.IsClosed || handle == IntPtr.Zero;
		}
	}

	private NimbleBridge_SynergyNetworkConnectionHandle()
		: base(IntPtr.Zero, true)
	{
	}

	internal NimbleBridge_SynergyNetworkConnectionHandle(IntPtr handle)
		: base(IntPtr.Zero, true)
	{
		SetHandle(handle);
	}

	[MonoPInvokeCallback(typeof(BridgeSynergyNetworkConnectionCallback))]
	internal static void OnSynergyNetworkConnectionCallback(IntPtr handlePtr, IntPtr callbackDataPtr)
	{
		NimbleBridge_CallbackHelper nimbleBridge_CallbackHelper = NimbleBridge_CallbackHelper.Get();
		NimbleBridge_SynergyNetworkConnectionHandle handle = new NimbleBridge_SynergyNetworkConnectionHandle(handlePtr);
		SynergyNetworkConnectionCallback callback = (SynergyNetworkConnectionCallback)nimbleBridge_CallbackHelper.GetData(callbackDataPtr);
		nimbleBridge_CallbackHelper.RunOnMainThread(delegate
		{
			callback(handle);
		});
	}

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_SynergyNetworkConnectionHandleWrapper_Dispose(NimbleBridge_SynergyNetworkConnectionHandle handleWrapper);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_SynergyNetworkConnectionHandle_wait(NimbleBridge_SynergyNetworkConnectionHandle synergyNetworkConnectionHandleWrapper);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_SynergyNetworkConnectionHandle_cancel(NimbleBridge_SynergyNetworkConnectionHandle synergyNetworkConnectionHandleWrapper);

	[DllImport("NimbleCInterface")]
	private static extern NimbleBridge_SynergyRequest NimbleBridge_SynergyNetworkConnectionHandle_getRequest(NimbleBridge_SynergyNetworkConnectionHandle synergyNetworkConnectionHandleWrapper);

	[DllImport("NimbleCInterface")]
	private static extern NimbleBridge_SynergyResponse NimbleBridge_SynergyNetworkConnectionHandle_getResponse(NimbleBridge_SynergyNetworkConnectionHandle synergyNetworkConnectionHandleWrapper);

	[DllImport("NimbleCInterface")]
	private static extern IntPtr NimbleBridge_SynergyNetworkConnectionHandle_getHeaderCallback(NimbleBridge_SynergyNetworkConnectionHandle synergyNetworkConnectionHandleWrapper);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_SynergyNetworkConnectionHandle_setHeaderCallback(NimbleBridge_SynergyNetworkConnectionHandle synergyNetworkConnectionHandleWrapper, BridgeSynergyNetworkConnectionCallback callback, IntPtr callbackData);

	[DllImport("NimbleCInterface")]
	private static extern IntPtr NimbleBridge_SynergyNetworkConnectionHandle_getProgressCallback(NimbleBridge_SynergyNetworkConnectionHandle synergyNetworkConnectionHandleWrapper);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_SynergyNetworkConnectionHandle_setProgressCallback(NimbleBridge_SynergyNetworkConnectionHandle synergyNetworkConnectionHandleWrapper, BridgeSynergyNetworkConnectionCallback callback, IntPtr callbackData);

	[DllImport("NimbleCInterface")]
	private static extern IntPtr NimbleBridge_SynergyNetworkConnectionHandle_getCompletionCallback(NimbleBridge_SynergyNetworkConnectionHandle synergyNetworkConnectionHandleWrapper);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_SynergyNetworkConnectionHandle_setCompletionCallback(NimbleBridge_SynergyNetworkConnectionHandle synergyNetworkConnectionHandleWrapper, BridgeSynergyNetworkConnectionCallback callback, IntPtr callbackData);

	protected override bool ReleaseHandle()
	{
		NimbleBridge_SynergyNetworkConnectionHandleWrapper_Dispose(this);
		return true;
	}

	public void Wait()
	{
		NimbleBridge_SynergyNetworkConnectionHandle_wait(this);
	}

	public void Cancel()
	{
		NimbleBridge_SynergyNetworkConnectionHandle_cancel(this);
	}

	public NimbleBridge_SynergyRequest GetRequest()
	{
		return NimbleBridge_SynergyNetworkConnectionHandle_getRequest(this);
	}

	public NimbleBridge_SynergyResponse GetResponse()
	{
		return NimbleBridge_SynergyNetworkConnectionHandle_getResponse(this);
	}

	public SynergyNetworkConnectionCallback GetHeaderCallback()
	{
		IntPtr dataPtr = NimbleBridge_SynergyNetworkConnectionHandle_getHeaderCallback(this);
		return (SynergyNetworkConnectionCallback)NimbleBridge_CallbackHelper.Get().GetData(dataPtr);
	}

	public void SetHeaderCallback(SynergyNetworkConnectionCallback synergyNetworkConnectionCallback)
	{
		IntPtr callbackData = NimbleBridge_CallbackHelper.Get().MakeCallbackData(synergyNetworkConnectionCallback);
		NimbleBridge_SynergyNetworkConnectionHandle_setHeaderCallback(this, OnSynergyNetworkConnectionCallback, callbackData);
	}

	public SynergyNetworkConnectionCallback GetProgressCallback()
	{
		IntPtr dataPtr = NimbleBridge_SynergyNetworkConnectionHandle_getProgressCallback(this);
		return (SynergyNetworkConnectionCallback)NimbleBridge_CallbackHelper.Get().GetData(dataPtr);
	}

	public void SetProgressCallback(SynergyNetworkConnectionCallback synergyNetworkConnectionCallback)
	{
		IntPtr callbackData = NimbleBridge_CallbackHelper.Get().MakeCallbackData(synergyNetworkConnectionCallback);
		NimbleBridge_SynergyNetworkConnectionHandle_setProgressCallback(this, OnSynergyNetworkConnectionCallback, callbackData);
	}

	public SynergyNetworkConnectionCallback GetCompletionCallback()
	{
		IntPtr dataPtr = NimbleBridge_SynergyNetworkConnectionHandle_getCompletionCallback(this);
		return (SynergyNetworkConnectionCallback)NimbleBridge_CallbackHelper.Get().GetData(dataPtr);
	}

	public void SetCompletionCallback(SynergyNetworkConnectionCallback synergyNetworkConnectionCallback)
	{
		IntPtr callbackData = NimbleBridge_CallbackHelper.Get().MakeCallbackData(synergyNetworkConnectionCallback);
		NimbleBridge_SynergyNetworkConnectionHandle_setCompletionCallback(this, OnSynergyNetworkConnectionCallback, callbackData);
	}
}
