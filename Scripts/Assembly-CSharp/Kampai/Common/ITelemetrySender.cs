namespace Kampai.Common
{
	public interface ITelemetrySender
	{
		void SendEvent(TelemetryEvent gameEvent);

		void COPPACompliance();

		void SharingUsage(bool enabled);
	}
}
