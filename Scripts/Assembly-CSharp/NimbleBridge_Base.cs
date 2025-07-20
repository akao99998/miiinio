using System;
using System.Runtime.InteropServices;

public class NimbleBridge_Base
{
	public enum NimbleBridge_Base_NimbleConfiguration
	{
		CONFIGURATION_UNKNOWN = 0,
		CONFIGURATION_INTEGRATION = 1,
		CONFIGURATION_STAGE = 2,
		CONFIGURATION_LIVE = 3,
		CONFIGURATION_CUSTOMIZED = 4
	}

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_Base_setupNimble();

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_Base_teardownNimble();

	[DllImport("NimbleCInterface")]
	private static extern IntPtr NimbleBridge_Base_getComponentList();

	[DllImport("NimbleCInterface")]
	private static extern int NimbleBridge_Base_getConfiguration();

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_Base_restartWithConfiguration(int configuration);

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_Base_configurationToName(int configuration);

	[DllImport("NimbleCInterface")]
	private static extern int NimbleBridge_Base_configurationFromName(string config);

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_Base_getSdkVersion();

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_Base_getReleaseVersion();

	public static void SetupNimble()
	{
		NimbleBridge_Base_setupNimble();
	}

	public static void TeardownNimble()
	{
		NimbleBridge_Base_teardownNimble();
	}

	public static string[] GetComponentList()
	{
		return MarshalUtility.ConvertPtrToArray(NimbleBridge_Base_getComponentList());
	}

	public static NimbleBridge_Base_NimbleConfiguration GetConfiguration()
	{
		return (NimbleBridge_Base_NimbleConfiguration)NimbleBridge_Base_getConfiguration();
	}

	public static void RestartWithConfiguration(NimbleBridge_Base_NimbleConfiguration configuration)
	{
		NimbleBridge_Base_restartWithConfiguration((int)configuration);
	}

	public static string ConfigurationToName(NimbleBridge_Base_NimbleConfiguration configuration)
	{
		return NimbleBridge_Base_configurationToName((int)configuration);
	}

	public static NimbleBridge_Base_NimbleConfiguration ConfigurationFromName(string config)
	{
		return (NimbleBridge_Base_NimbleConfiguration)NimbleBridge_Base_configurationFromName(config);
	}

	public static string GetSdkVersion()
	{
		return NimbleBridge_Base_getSdkVersion();
	}

	public static string GetReleaseVersion()
	{
		return NimbleBridge_Base_getReleaseVersion();
	}
}
