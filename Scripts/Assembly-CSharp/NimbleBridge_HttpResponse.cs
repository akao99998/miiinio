using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class NimbleBridge_HttpResponse : SafeHandle
{
	public override bool IsInvalid
	{
		get
		{
			return base.IsClosed || handle == IntPtr.Zero;
		}
	}

	internal NimbleBridge_HttpResponse()
		: base(IntPtr.Zero, true)
	{
	}

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_HttpResponse_Dispose(NimbleBridge_HttpResponse httpResponseWrapper);

	[DllImport("NimbleCInterface")]
	private static extern bool NimbleBridge_HttpResponse_isCompleted(NimbleBridge_HttpResponse httpResponseWrapper);

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_HttpResponse_getUrl(NimbleBridge_HttpResponse httpResponseWrapper);

	[DllImport("NimbleCInterface")]
	private static extern int NimbleBridge_HttpResponse_getStatusCode(NimbleBridge_HttpResponse httpResponseWrapper);

	[DllImport("NimbleCInterface")]
	private static extern IntPtr NimbleBridge_HttpResponse_getHeaders(NimbleBridge_HttpResponse httpResponseWrapper);

	[DllImport("NimbleCInterface")]
	private static extern long NimbleBridge_HttpResponse_getExpectedContentLength(NimbleBridge_HttpResponse httpResponseWrapper);

	[DllImport("NimbleCInterface")]
	private static extern long NimbleBridge_HttpResponse_getDownloadedContentLength(NimbleBridge_HttpResponse httpResponseWrapper);

	[DllImport("NimbleCInterface")]
	private static extern double NimbleBridge_HttpResponse_getLastModified(NimbleBridge_HttpResponse httpResponseWrapper);

	[DllImport("NimbleCInterface")]
	private static extern IntPtr NimbleBridge_HttpResponse_getData(NimbleBridge_HttpResponse httpResponseWrapper);

	[DllImport("NimbleCInterface")]
	private static extern NimbleBridge_Error NimbleBridge_HttpResponse_getError(NimbleBridge_HttpResponse httpResponseWrapper);

	protected override bool ReleaseHandle()
	{
		NimbleBridge_HttpResponse_Dispose(this);
		return true;
	}

	public bool IsCompleted()
	{
		return NimbleBridge_HttpResponse_isCompleted(this);
	}

	public string GetUrl()
	{
		return NimbleBridge_HttpResponse_getUrl(this);
	}

	public int GetStatusCode()
	{
		return NimbleBridge_HttpResponse_getStatusCode(this);
	}

	public Dictionary<string, string> GetHeaders()
	{
		IntPtr mapPtr = NimbleBridge_HttpResponse_getHeaders(this);
		return MarshalUtility.ConvertPtrToDictionary(mapPtr);
	}

	public long GetExpectedContentLength()
	{
		return NimbleBridge_HttpResponse_getExpectedContentLength(this);
	}

	public long GetDownloadedContentLength()
	{
		return NimbleBridge_HttpResponse_getDownloadedContentLength(this);
	}

	public double GetLastModified()
	{
		return NimbleBridge_HttpResponse_getLastModified(this);
	}

	public byte[] GetData()
	{
		IntPtr dataPtr = NimbleBridge_HttpResponse_getData(this);
		return MarshalUtility.ConvertPtrToData(dataPtr);
	}

	public NimbleBridge_Error GetError()
	{
		return NimbleBridge_HttpResponse_getError(this);
	}
}
