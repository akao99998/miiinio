using System.Runtime.InteropServices;

public class NimbleBridge_SynergyIdManager
{
	public const string NOTIFICATION_SYNERGY_ID_CHANGED = "nimble.synergyidmanager.notification.synergy_id_changed";

	public const string NOTIFICATION_ANONYMOUS_SYNERGY_ID_CHANGED = "nimble.synergyidmanager.notification.anonymous_synergy_id_changed";

	private NimbleBridge_SynergyIdManager()
	{
	}

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_SynergyIdManager_getSynergyId();

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_SynergyIdManager_getAnonymousSynergyId();

	[DllImport("NimbleCInterface")]
	private static extern NimbleBridge_Error NimbleBridge_SynergyIdManager_login(string userSynergyId, string authenticatorIdentifier);

	[DllImport("NimbleCInterface")]
	private static extern NimbleBridge_Error NimbleBridge_SynergyIdManager_logout(string authenticatorIdentifier);

	public static NimbleBridge_SynergyIdManager GetComponent()
	{
		return new NimbleBridge_SynergyIdManager();
	}

	public string GetSynergyId()
	{
		return NimbleBridge_SynergyIdManager_getSynergyId();
	}

	public string GetAnonymousSynergyId()
	{
		return NimbleBridge_SynergyIdManager_getAnonymousSynergyId();
	}

	public NimbleBridge_Error Login(string userSynergyId, string authenticatorIdentifier)
	{
		return NimbleBridge_SynergyIdManager_login(userSynergyId, authenticatorIdentifier);
	}

	public NimbleBridge_Error Logout(string authenticatorIdentifier)
	{
		return NimbleBridge_SynergyIdManager_logout(authenticatorIdentifier);
	}
}
