using System.Collections.Generic;

namespace Kampai.Common.Service.HealthMetrics
{
	public interface IClientHealthService
	{
		Dictionary<string, int> MeterEvents { get; }

		Dictionary<string, float> TimerEvents { get; }

		void MarkMeterEvent(string eventName);

		void MarkTimerEvent(string eventName, float duration);

		void ClearMeterEvents();

		void ClearTimerEvents();
	}
}
