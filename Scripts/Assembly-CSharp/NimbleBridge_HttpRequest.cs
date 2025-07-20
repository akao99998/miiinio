using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class NimbleBridge_HttpRequest : SafeHandle
{
	public enum Method
	{
		HTTP_GET = 0,
		HTTP_HEAD = 1,
		HTTP_POST = 2,
		HTTP_PUT = 3
	}

	public enum DownloadOverwritePolicy
	{
		OVERWRITE = 0,
		RESUME_DOWNLOAD = 1,
		DATE_CHECK = 2,
		LENGTH_CHECK = 4,
		SMART = 15
	}

	public override bool IsInvalid
	{
		get
		{
			return base.IsClosed || handle == IntPtr.Zero;
		}
	}

	internal NimbleBridge_HttpRequest()
		: base(IntPtr.Zero, true)
	{
	}

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_HttpRequest_Dispose(NimbleBridge_HttpRequest httpRequestWrapper);

	[DllImport("NimbleCInterface")]
	private static extern NimbleBridge_HttpRequest NimbleBridge_HttpRequest_requestWithUrl(string url);

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_HttpRequest_getUrl(NimbleBridge_HttpRequest httpRequestWrapper);

	[DllImport("NimbleCInterface")]
	private static extern int NimbleBridge_HttpRequest_getMethod(NimbleBridge_HttpRequest httpRequestWrapper);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_HttpRequest_setMethod(NimbleBridge_HttpRequest httpRequestWrapper, int method);

	[DllImport("NimbleCInterface")]
	private static extern IntPtr NimbleBridge_HttpRequest_getData(NimbleBridge_HttpRequest httpRequestWrapper);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_HttpRequest_setData(NimbleBridge_HttpRequest httpRequestWrapper, IntPtr dataWrapper);

	[DllImport("NimbleCInterface")]
	private static extern IntPtr NimbleBridge_HttpRequest_getHeaders(NimbleBridge_HttpRequest httpRequestWrapper);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_HttpRequest_setHeaders(NimbleBridge_HttpRequest httpRequestWrapper, IntPtr map);

	[DllImport("NimbleCInterface")]
	private static extern double NimbleBridge_HttpRequest_getTimeout(NimbleBridge_HttpRequest httpRequestWrapper);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_HttpRequest_setTimeout(NimbleBridge_HttpRequest httpRequestWrapper, double timeout);

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_HttpRequest_getTargetFilePath(NimbleBridge_HttpRequest httpRequestWrapper);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_HttpRequest_setTargetFilePath(NimbleBridge_HttpRequest httpRequestWrapper, string targetFilePath);

	[DllImport("NimbleCInterface")]
	private static extern int NimbleBridge_HttpRequest_getOverwritePolicy(NimbleBridge_HttpRequest httpRequestWrapper);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_HttpRequest_setOverwritePolicy(NimbleBridge_HttpRequest httpRequestWrapper, int policy);

	[DllImport("NimbleCInterface")]
	private static extern bool NimbleBridge_HttpRequest_getRunInBackground(NimbleBridge_HttpRequest httpRequestWrapper);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_HttpRequest_setRunInBackground(NimbleBridge_HttpRequest httpRequestWrapper, bool runInBackground);

	protected override bool ReleaseHandle()
	{
		NimbleBridge_HttpRequest_Dispose(this);
		return true;
	}

	public static NimbleBridge_HttpRequest RequestWithUrl(string url)
	{
		return NimbleBridge_HttpRequest_requestWithUrl(url);
	}

	public string GetUrl()
	{
		return NimbleBridge_HttpRequest_getUrl(this);
	}

	public Method GetMethod()
	{
		return (Method)NimbleBridge_HttpRequest_getMethod(this);
	}

	public void SetMethod(Method method)
	{
		NimbleBridge_HttpRequest_setMethod(this, (int)method);
	}

	public byte[] GetData()
	{
		IntPtr dataPtr = NimbleBridge_HttpRequest_getData(this);
		return MarshalUtility.ConvertPtrToData(dataPtr);
	}

	public void SetData(byte[] data)
	{
		IntPtr intPtr = IntPtr.Zero;
		try
		{
			intPtr = MarshalUtility.ConvertDataToPtr(data);
			NimbleBridge_HttpRequest_setData(this, intPtr);
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				MarshalUtility.DisposeDataPtr(intPtr);
			}
		}
	}

	public Dictionary<string, string> GetHeaders()
	{
		IntPtr mapPtr = NimbleBridge_HttpRequest_getHeaders(this);
		return MarshalUtility.ConvertPtrToDictionary(mapPtr);
	}

	public void SetHeaders(Dictionary<string, string> dictionary)
	{
		IntPtr intPtr = IntPtr.Zero;
		try
		{
			intPtr = MarshalUtility.ConvertDictionaryToPtr(dictionary);
			NimbleBridge_HttpRequest_setHeaders(this, intPtr);
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				MarshalUtility.DisposeMapPtr(intPtr);
			}
		}
	}

	public double GetTimeout()
	{
		return NimbleBridge_HttpRequest_getTimeout(this);
	}

	public void SetTimeout(double timeout)
	{
		NimbleBridge_HttpRequest_setTimeout(this, timeout);
	}

	public string GetTargetFilePath()
	{
		return NimbleBridge_HttpRequest_getTargetFilePath(this);
	}

	public void SetTargetFilePath(string targetFilePath)
	{
		NimbleBridge_HttpRequest_setTargetFilePath(this, targetFilePath);
	}

	public int GetOverwritePolicy()
	{
		return NimbleBridge_HttpRequest_getOverwritePolicy(this);
	}

	public void SetOverwritePolicy(int policy)
	{
		NimbleBridge_HttpRequest_setOverwritePolicy(this, policy);
	}

	public bool GetRunInBackground()
	{
		return NimbleBridge_HttpRequest_getRunInBackground(this);
	}

	public void SetRunInBackground(bool runInBackground)
	{
		NimbleBridge_HttpRequest_setRunInBackground(this, runInBackground);
	}
}
