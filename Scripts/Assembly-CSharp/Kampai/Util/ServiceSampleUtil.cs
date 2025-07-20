using System.Collections.Generic;
using Kampai.Game;

namespace Kampai.Util
{
	public static class ServiceSampleUtil
	{
		public enum ServiceType
		{
			ClientHealthMetrics = 0,
			Loggly = 1
		}

		private sealed class ServiceState
		{
			public bool IsServiceEnabled { get; set; }

			public bool IsServiceEnabledCalculated { get; set; }
		}

		private static Dictionary<ServiceType, ServiceState> serviceStateLookup = new Dictionary<ServiceType, ServiceState>();

		public static bool IsServiceEnabled(ServiceType serviceType, float samplePercentage, IUserSessionService sessionService)
		{
			if (!serviceStateLookup.ContainsKey(serviceType))
			{
				serviceStateLookup.Add(serviceType, new ServiceState());
			}
			ServiceState serviceState = serviceStateLookup[serviceType];
			if (!serviceState.IsServiceEnabledCalculated)
			{
				if (sessionService.UserSession == null)
				{
					serviceState.IsServiceEnabled = true;
				}
				else
				{
					string userID = sessionService.UserSession.UserID;
					int num = 4;
					if (userID == null || userID.Length < num)
					{
						serviceState.IsServiceEnabled = true;
					}
					else
					{
						string s = userID.Substring(userID.Length - num, num);
						int result;
						if (!int.TryParse(s, out result))
						{
							result = 0;
						}
						float num2 = (float)result / 100f;
						if (num2 < samplePercentage)
						{
							serviceState.IsServiceEnabled = true;
						}
						else
						{
							serviceState.IsServiceEnabled = false;
						}
					}
				}
				serviceState.IsServiceEnabledCalculated = true;
			}
			return serviceState.IsServiceEnabled;
		}

		public static void Reset()
		{
			foreach (KeyValuePair<ServiceType, ServiceState> item in serviceStateLookup)
			{
				item.Value.IsServiceEnabledCalculated = false;
			}
		}
	}
}
