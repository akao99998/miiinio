using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class NimbleBridge_Tracking
{
	private NimbleBridge_Tracking()
	{
	}

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_Tracking_logEvent(string type, IntPtr map);

	[DllImport("NimbleCInterface")]
	private static extern bool NimbleBridge_Tracking_isEnabled();

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_Tracking_setEnabled(bool enable);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_Tracking_addCustomSessionData(string keyString, string valueString);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_Tracking_removeCustomSessionData(string keyString);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_Tracking_setTrackingAttribute(string keyString, string valueString);

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_Tracking_getSessionId();

	[DllImport("NimbleCInterface")]
	private static extern bool NimbleBridge_Tracking_isNimbleStandardEvent(string type);

	[DllImport("NimbleCInterface")]
	private static extern bool NimbleBridge_Tracking_isEventTypeEqual(string _event, string otherEvent);

	[DllImport("NimbleCInterface")]
	private static extern bool NimbleBridge_Tracking_isEventTypeMemberOfSet(string _event, string[] eventTypeArray);

	public static NimbleBridge_Tracking GetComponent()
	{
		return new NimbleBridge_Tracking();
	}

	public void LogEvent(string type, Dictionary<string, string> parameters)
	{
		IntPtr intPtr = IntPtr.Zero;
		try
		{
			intPtr = MarshalUtility.ConvertDictionaryToPtr(parameters);
			NimbleBridge_Tracking_logEvent(type, intPtr);
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				MarshalUtility.DisposeMapPtr(intPtr);
			}
		}
	}

	public bool IsEnabled()
	{
		return NimbleBridge_Tracking_isEnabled();
	}

	public void SetEnabled(bool enable)
	{
		NimbleBridge_Tracking_setEnabled(enable);
	}

	public void AddCustomSessionValue(string key, string value)
	{
		NimbleBridge_Tracking_addCustomSessionData(key, value);
	}

	public void RemoveCustomSessionValue(string key)
	{
		NimbleBridge_Tracking_removeCustomSessionData(key);
	}

	public void ClearCustomSessionData()
	{
	}

	public void SetTrackingAttribute(string key, string value)
	{
		NimbleBridge_Tracking_setTrackingAttribute(key, value);
	}

	public string GetSessionId()
	{
		return NimbleBridge_Tracking_getSessionId();
	}

	public static bool IsNimbleStandardEvent(string type)
	{
		return NimbleBridge_Tracking_isNimbleStandardEvent(type);
	}

	public static bool IsEventTypeEqual(string _event, string otherEvent)
	{
		return NimbleBridge_Tracking_isEventTypeEqual(_event, otherEvent);
	}

	public static bool IsEventTypeMemberOfSet(string _event, List<string> eventTypeSet)
	{
		string[] array = new string[eventTypeSet.Count + 1];
		eventTypeSet.CopyTo(array);
		array[eventTypeSet.Count] = null;
		return NimbleBridge_Tracking_isEventTypeMemberOfSet(_event, array);
	}
}
