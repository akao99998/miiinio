using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class NimbleBridge_OperationalTelemetryDispatch
{
	public const string EVENTTYPE_TRACKING_SYNERGY_PAYLOADS = "com.ea.nimble.trackingimpl.synergy";

	public const string EVENTTYPE_NETWORK_METRICS = "com.ea.nimble.network";

	public const string NOTIFICATION_OT_EVENT_THRESHOLD_WARNING = "nimble.notification.ot.eventthresholdwarning";

	private NimbleBridge_OperationalTelemetryDispatch()
	{
	}

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_OperationalTelemetryDispatch_deleteEventsArray(IntPtr array);

	[DllImport("NimbleCInterface")]
	private static extern IntPtr NimbleBridge_OperationalTelemetryDispatch_getEvents(string type);

	public static NimbleBridge_OperationalTelemetryDispatch GetComponent()
	{
		return new NimbleBridge_OperationalTelemetryDispatch();
	}

	public NimbleBridge_OperationalTelemetryEvent[] GetEvents(string type)
	{
		List<NimbleBridge_OperationalTelemetryEvent> list = new List<NimbleBridge_OperationalTelemetryEvent>();
		IntPtr intPtr = NimbleBridge_OperationalTelemetryDispatch_getEvents(type);
		IntPtr intPtr2 = Marshal.ReadIntPtr(intPtr);
		int num = 1;
		while (intPtr2 != IntPtr.Zero)
		{
			list.Add(new NimbleBridge_OperationalTelemetryEvent(intPtr2));
			intPtr2 = Marshal.ReadIntPtr(intPtr, num * Marshal.SizeOf(typeof(IntPtr)));
			num++;
		}
		NimbleBridge_OperationalTelemetryDispatch_deleteEventsArray(intPtr);
		return list.ToArray();
	}

	public void SetMaxEventCount(string type, int count)
	{
		NimbleBridge_OperationalTelemetryDispatch_setMaxEventCount(type, count);
	}

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_OperationalTelemetryDispatch_setMaxEventCount(string type, int count);

	public int GetMaxEventCount(string type)
	{
		return NimbleBridge_OperationalTelemetryDispatch_getMaxEventCount(type);
	}

	[DllImport("NimbleCInterface")]
	private static extern int NimbleBridge_OperationalTelemetryDispatch_getMaxEventCount(string type);
}
