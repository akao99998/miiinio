using System;
using System.Runtime.InteropServices;

public class NimbleBridge_Error : SafeHandle
{
	public enum Code : uint
	{
		OK = 0u,
		SYSTEM_UNEXPECT = 100u,
		NETWORK_UNSUPPORTED_CONNECTION_TYPE = 1001u,
		NETWORK_NO_CONNECTION = 1002u,
		NETWORK_UNREACHABLE = 1003u,
		NETWORK_OVERSIZE_DATA = 1004u,
		NETWORK_OPERATION_CANCELLED = 1005u,
		NETWORK_INVALID_SERVER_RESPONSE = 1006u,
		NETWORK_TIMEOUT = 1007u,
		SYNERGY_GET_DIRECTION_TIMED_OUT = 2001u,
		SYNERGY_SERVER_FULL = 2002u,
		SYNERGY_GET_EA_DEVICE_ID_FAILURE = 2003u,
		SYNERGY_VALIDATE_EA_DEVICE_ID_FAILURE = 2004u,
		SYNERGY_GET_ANONYMOUS_ID_FAILURE = 2005u,
		MTX_BILLING_UNAVAILABLE = 3001u,
		MTX_USER_CANCELED = 3002u,
		MTX_ITEM_ALREADY_PURCHASED = 3003u,
		MTX_MTX_ITEM_UNAVAILABLE = 3004u,
		MTX_PURCHASE_VERIFICATION_FAILED = 3005u,
		MISSING_CALLBACK = 4000u,
		INVALID_ARGUMENT = 4001u,
		UNKNOWN = uint.MaxValue
	}

	public override bool IsInvalid
	{
		get
		{
			return base.IsClosed || handle == IntPtr.Zero;
		}
	}

	private NimbleBridge_Error()
		: base(IntPtr.Zero, true)
	{
	}

	internal NimbleBridge_Error(IntPtr handle)
		: base(IntPtr.Zero, true)
	{
		SetHandle(handle);
	}

	public NimbleBridge_Error(Code code, string reason)
		: this(NimbleBridge_Error_Error((int)code, reason))
	{
	}

	public NimbleBridge_Error(Code code, string reason, NimbleBridge_Error cause)
		: this(NimbleBridge_Error_ErrorWithCause((int)code, reason, cause))
	{
	}

	public NimbleBridge_Error(Code code, string reason, NimbleBridge_Error cause, string domain)
		: this(NimbleBridge_Error_ErrorWithDomain((int)code, reason, cause, domain))
	{
	}

	[DllImport("NimbleCInterface")]
	private static extern IntPtr NimbleBridge_Error_Error(int code, string reason);

	[DllImport("NimbleCInterface")]
	private static extern IntPtr NimbleBridge_Error_ErrorWithCause(int code, string reason, NimbleBridge_Error cause);

	[DllImport("NimbleCInterface")]
	private static extern IntPtr NimbleBridge_Error_ErrorWithDomain(int code, string reason, NimbleBridge_Error cause, string domain);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_Error_Dispose(NimbleBridge_Error errorWrapper);

	[DllImport("NimbleCInterface")]
	private static extern bool NimbleBridge_Error_isNull(NimbleBridge_Error errorWrapper);

	[DllImport("NimbleCInterface")]
	private static extern int NimbleBridge_Error_getCode(NimbleBridge_Error errorWrapper);

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_Error_getReason(NimbleBridge_Error errorWrapper);

	[DllImport("NimbleCInterface")]
	private static extern NimbleBridge_Error NimbleBridge_Error_getCause(NimbleBridge_Error errorWrapper);

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_Error_getDomain(NimbleBridge_Error errorWrapper);

	protected override bool ReleaseHandle()
	{
		NimbleBridge_Error_Dispose(this);
		return true;
	}

	public bool IsNull()
	{
		return NimbleBridge_Error_isNull(this);
	}

	public Code GetCode()
	{
		return (Code)NimbleBridge_Error_getCode(this);
	}

	public string GetReason()
	{
		return NimbleBridge_Error_getReason(this);
	}

	public NimbleBridge_Error GetCause()
	{
		return NimbleBridge_Error_getCause(this);
	}

	public string GetDomain()
	{
		return NimbleBridge_Error_getDomain(this);
	}

	public override string ToString()
	{
		string text = string.Format("{0}({1}): {2}", GetDomain(), GetCode(), GetReason());
		if (!GetCause().IsNull())
		{
			return text + string.Format(" Cause: {0}", GetCause().ToString());
		}
		return text + " Cause: null";
	}
}
