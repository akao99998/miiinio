using UnityEngine;

namespace Kampai.Common
{
	public class NetworkModel
	{
		public bool isConnectionLost { get; set; }

		public NetworkReachability reachability { get; set; }
	}
}
