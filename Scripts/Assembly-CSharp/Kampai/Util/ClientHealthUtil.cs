using Kampai.Game;

namespace Kampai.Util
{
	public static class ClientHealthUtil
	{
		public static bool isHealthMetricsEnabled(IConfigurationsService configService, IUserSessionService sessionService)
		{
			if (configService == null || configService.GetConfigurations() == null)
			{
				return false;
			}
			int healthMetricPercentage = configService.GetConfigurations().healthMetricPercentage;
			return ServiceSampleUtil.IsServiceEnabled(ServiceSampleUtil.ServiceType.ClientHealthMetrics, healthMetricPercentage, sessionService);
		}
	}
}
