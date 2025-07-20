using System.Collections;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Util;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.Main.View
{
	public class NetworkMonitorMediator : Mediator
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("NetworkMonitorMediator") as IKampaiLogger;

		[Inject]
		public NetworkModel model { get; set; }

		[Inject]
		public NetworkConnectionLostSignal connectionLostSignal { get; set; }

		[Inject]
		public NetworkTypeChangedSignal typeChangedSignal { get; set; }

		private IEnumerator Start()
		{
			model.reachability = NetworkUtil.GetNetworkReachability();
			logger.Info("Initial network connection type: {0}.", model.reachability);
			while (true)
			{
				if (!model.isConnectionLost && model.reachability == NetworkReachability.NotReachable)
				{
					connectionLostSignal.Dispatch();
				}
				yield return new WaitForSeconds(15f);
			}
		}

		private void Update()
		{
			NetworkReachability networkReachability = NetworkUtil.GetNetworkReachability();
			if (networkReachability != model.reachability)
			{
				logger.Info("Network connection type switched: {0} -> {1}.", model.reachability, networkReachability);
				typeChangedSignal.Dispatch(networkReachability);
				model.reachability = networkReachability;
			}
		}
	}
}
