using UnityEngine;

namespace Kampai.Util
{
	public static class NetworkUtil
	{
		private static readonly AndroidJavaClass miscUtils = new AndroidJavaClass("com.ea.gp.minions.utils.Misc");

		public static bool IsConnected()
		{
			return miscUtils.CallStatic<bool>("isConnected", new object[0]);
		}

		public static bool IsNetworkWiFi()
		{
			bool flag = false;
			return miscUtils.CallStatic<bool>("isNetworkWiFi", new object[0]);
		}

		public static NetworkReachability GetNetworkReachability()
		{
			return IsNetworkWiFi() ? NetworkReachability.ReachableViaLocalAreaNetwork : (IsConnected() ? NetworkReachability.ReachableViaCarrierDataNetwork : NetworkReachability.NotReachable);
		}
	}
}
