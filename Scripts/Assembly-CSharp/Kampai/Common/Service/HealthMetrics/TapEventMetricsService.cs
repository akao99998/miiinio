namespace Kampai.Common.Service.HealthMetrics
{
	public class TapEventMetricsService : ITapEventMetricsService
	{
		private int count;

		int ITapEventMetricsService.Count
		{
			get
			{
				return count;
			}
		}

		void ITapEventMetricsService.Mark()
		{
			count++;
		}

		void ITapEventMetricsService.Clear()
		{
			count = 0;
		}
	}
}
