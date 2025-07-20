using System.Collections.Generic;

namespace Kampai.Common
{
	public class TelemetryEvent
	{
		public SynergyTrackingEventType Type { get; set; }

		public IList<TelemetryParameter> Parameters { get; set; }

		public TelemetryEvent(SynergyTrackingEventType type)
		{
			Type = type;
		}
	}
}
