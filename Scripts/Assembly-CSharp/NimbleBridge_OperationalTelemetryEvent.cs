using System;
using System.Runtime.InteropServices;

public class NimbleBridge_OperationalTelemetryEvent : SafeHandle
{
	public override bool IsInvalid
	{
		get
		{
			return base.IsClosed || handle == IntPtr.Zero;
		}
	}

	private NimbleBridge_OperationalTelemetryEvent()
		: base(IntPtr.Zero, true)
	{
	}

	internal NimbleBridge_OperationalTelemetryEvent(IntPtr handle)
		: base(IntPtr.Zero, true)
	{
		SetHandle(handle);
	}

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_OperationalTelemetryEvent_Dispose(NimbleBridge_OperationalTelemetryEvent wrapper);

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_OperationalTelemetryEvent_getEventType(NimbleBridge_OperationalTelemetryEvent wrapper);

	[DllImport("NimbleCInterface")]
	private static extern double NimbleBridge_OperationalTelemetryEvent_getLoggedTime(NimbleBridge_OperationalTelemetryEvent wrapper);

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_OperationalTelemetryEvent_getEventDictionary(NimbleBridge_OperationalTelemetryEvent wrapper);

	protected override bool ReleaseHandle()
	{
		NimbleBridge_OperationalTelemetryEvent_Dispose(this);
		return true;
	}

	public string GetEventType()
	{
		return NimbleBridge_OperationalTelemetryEvent_getEventType(this);
	}

	public double GetLoggedTime()
	{
		return NimbleBridge_OperationalTelemetryEvent_getLoggedTime(this);
	}

	public string GetEventDictionary()
	{
		return NimbleBridge_OperationalTelemetryEvent_getEventDictionary(this);
	}
}
