namespace Kampai.Common
{
	public interface ISwrveService : IIapTelemetryService, ITelemetrySender
	{
		void UpdateResources();

		void SendUserStatsUpdate();
	}
}
