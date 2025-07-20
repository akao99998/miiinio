using Kampai.Game;

namespace Kampai.Util.Logging.Hosted
{
	public interface ILogglyDtoCache
	{
		void Initialize(IUserSessionService userSessionService, IConfigurationsService configurationsService);

		LogglyDto GetCachedDto();

		void RefreshConfigurationValues();

		void RefreshUserSessionValues();

		void RefreshClientVersionValues();
	}
}
