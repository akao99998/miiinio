using System.Runtime.InteropServices;

public class NimbleBridge_ApplicationEnvironment
{
	public const string NOTIFICATION_AGE_COMPLIANCE_REFRESHED = "nimble.notification.age_compliance_refreshed";

	private NimbleBridge_ApplicationEnvironment()
	{
	}

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_ApplicationEnvironment_getApplicationBundleId();

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_ApplicationEnvironment_setApplicationBundleId(string bundleId);

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_ApplicationEnvironment_getApplicationName();

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_ApplicationEnvironment_getApplicationVersion();

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_ApplicationEnvironment_getApplicationLanguageCode();

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_ApplicationEnvironment_setApplicationLanguageCode(string languageCode);

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_ApplicationEnvironment_getDocumentPath();

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_ApplicationEnvironment_getCachePath();

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_ApplicationEnvironment_getTempPath();

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_ApplicationEnvironment_getCarrier();

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_ApplicationEnvironment_getDeviceString();

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_ApplicationEnvironment_getMACAddress();

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_ApplicationEnvironment_getIPAddress();

	[DllImport("NimbleCInterface")]
	private static extern bool NimbleBridge_ApplicationEnvironment_isAppCracked();

	[DllImport("NimbleCInterface")]
	private static extern bool NimbleBridge_ApplicationEnvironment_isDeviceJailbroken();

	[DllImport("NimbleCInterface")]
	private static extern int NimbleBridge_ApplicationEnvironment_getAgeCompliance();

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_ApplicationEnvironment_refreshAgeCompliance();

	[DllImport("NimbleCInterface")]
	private static extern bool NimbleBridge_ApplicationEnvironment_getIadAttribution();

	public static NimbleBridge_ApplicationEnvironment GetComponent()
	{
		return new NimbleBridge_ApplicationEnvironment();
	}

	public string GetApplicationBundleId()
	{
		return NimbleBridge_ApplicationEnvironment_getApplicationBundleId();
	}

	public void SetApplicationBundleId(string bundleId)
	{
		NimbleBridge_ApplicationEnvironment_setApplicationBundleId(bundleId);
	}

	public string GetApplicationName()
	{
		return NimbleBridge_ApplicationEnvironment_getApplicationName();
	}

	public string GetApplicationVersion()
	{
		return NimbleBridge_ApplicationEnvironment_getApplicationVersion();
	}

	public string GetApplicationLanguageCode()
	{
		return NimbleBridge_ApplicationEnvironment_getApplicationLanguageCode();
	}

	public void SetApplicationLanguageCode(string languageCode)
	{
		NimbleBridge_ApplicationEnvironment_setApplicationLanguageCode(languageCode);
	}

	public string GetDocumentPath()
	{
		return NimbleBridge_ApplicationEnvironment_getDocumentPath();
	}

	public string GetCachePath()
	{
		return NimbleBridge_ApplicationEnvironment_getCachePath();
	}

	public string GetTempPath()
	{
		return NimbleBridge_ApplicationEnvironment_getTempPath();
	}

	public string GetCarrier()
	{
		return NimbleBridge_ApplicationEnvironment_getCarrier();
	}

	public string GetDeviceString()
	{
		return NimbleBridge_ApplicationEnvironment_getDeviceString();
	}

	public string GetMACAddress()
	{
		return NimbleBridge_ApplicationEnvironment_getMACAddress();
	}

	public string GetIPAddress()
	{
		return NimbleBridge_ApplicationEnvironment_getIPAddress();
	}

	public bool IsAppCracked()
	{
		return NimbleBridge_ApplicationEnvironment_isAppCracked();
	}

	public bool IsDeviceJailbroken()
	{
		return NimbleBridge_ApplicationEnvironment_isDeviceJailbroken();
	}

	public int GetAgeCompliance()
	{
		return NimbleBridge_ApplicationEnvironment_getAgeCompliance();
	}

	public void RefreshAgeCompliance()
	{
		NimbleBridge_ApplicationEnvironment_refreshAgeCompliance();
	}

	public bool GetIadAttribution()
	{
		return NimbleBridge_ApplicationEnvironment_getIadAttribution();
	}
}
