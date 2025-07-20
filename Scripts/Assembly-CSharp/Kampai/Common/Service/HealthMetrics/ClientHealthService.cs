using System.Collections.Generic;
using Kampai.Game;
using Kampai.Util;

namespace Kampai.Common.Service.HealthMetrics
{
	public class ClientHealthService : IClientHealthService
	{
		private static Dictionary<string, int> meterEvents = new Dictionary<string, int>();

		private static Dictionary<string, float> timerEvents = new Dictionary<string, float>();

		Dictionary<string, int> IClientHealthService.MeterEvents
		{
			get
			{
				return meterEvents;
			}
		}

		Dictionary<string, float> IClientHealthService.TimerEvents
		{
			get
			{
				return timerEvents;
			}
		}

		[Inject]
		public IConfigurationsService configurationsService { get; set; }

		[Inject]
		public IUserSessionService userSessionService { get; set; }

		void IClientHealthService.MarkMeterEvent(string eventName)
		{
			if (ClientHealthUtil.isHealthMetricsEnabled(configurationsService, userSessionService))
			{
				int num = 0;
				if (meterEvents.ContainsKey(eventName))
				{
					num = meterEvents[eventName];
				}
				num++;
				meterEvents[eventName] = num;
			}
		}

		void IClientHealthService.MarkTimerEvent(string eventName, float duration)
		{
			if (ClientHealthUtil.isHealthMetricsEnabled(configurationsService, userSessionService))
			{
				if (timerEvents.ContainsKey(eventName))
				{
					timerEvents[eventName] = duration;
				}
				else
				{
					timerEvents.Add(eventName, duration);
				}
			}
		}

		void IClientHealthService.ClearMeterEvents()
		{
			meterEvents.Clear();
		}

		void IClientHealthService.ClearTimerEvents()
		{
			timerEvents.Clear();
		}
	}
}
