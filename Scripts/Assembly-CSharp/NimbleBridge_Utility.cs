using System.Runtime.InteropServices;

public class NimbleBridge_Utility
{
	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_Utility_getUTCDateStringFormat(double date);

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_Utility_SHA256HashString(string str);

	public static string GetUTCDateStringFormat(double date)
	{
		return NimbleBridge_Utility_getUTCDateStringFormat(date);
	}

	public static string SHA256HashString(string str)
	{
		return NimbleBridge_Utility_SHA256HashString(str);
	}
}
