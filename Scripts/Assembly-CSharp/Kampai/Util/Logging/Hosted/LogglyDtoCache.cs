using System;
using System.Collections.Generic;
using Kampai.Game;

namespace Kampai.Util.Logging.Hosted
{
	public class LogglyDtoCache : ILogglyDtoCache
	{
		public enum DtoProperty
		{
			ClientVersion = 0,
			ClientDeviceType = 1,
			ClientPlatform = 2,
			ConfigUrl = 3,
			ConfigVariant = 4,
			DefinitionId = 5,
			UserId = 6,
			SynergyId = 7
		}

		private const string DEFAULT_VALUE = "unknown";

		private IUserSessionService userSessionService;

		private IConfigurationsService configurationsService;

		private readonly Dictionary<DtoProperty, CachedValue<string>> cache;

		private readonly CachedValue<IList<string>> definitionVariantsCache;

		[Inject]
		public IClientVersion clientVersion { get; set; }

		public LogglyDtoCache()
		{
			cache = new Dictionary<DtoProperty, CachedValue<string>>();
			Func<string> valueSetter = () => (clientVersion != null) ? clientVersion.GetClientVersion() : null;
			cache.Add(DtoProperty.ClientVersion, new CachedValue<string>(valueSetter));
			Func<string> valueSetter2 = () => (clientVersion != null) ? clientVersion.GetClientDeviceType() : null;
			cache.Add(DtoProperty.ClientDeviceType, new CachedValue<string>(valueSetter2));
			Func<string> valueSetter3 = () => (clientVersion != null) ? clientVersion.GetClientPlatform() : null;
			cache.Add(DtoProperty.ClientPlatform, new CachedValue<string>(valueSetter3));
			Func<string> valueSetter4 = () => (configurationsService != null) ? configurationsService.GetConfigURL() : null;
			cache.Add(DtoProperty.ConfigUrl, new CachedValue<string>(valueSetter4));
			Func<string> valueSetter5 = () => (configurationsService != null) ? configurationsService.GetConfigVariant() : null;
			cache.Add(DtoProperty.ConfigVariant, new CachedValue<string>(valueSetter5));
			Func<string> valueSetter6 = delegate
			{
				if (configurationsService != null)
				{
					ConfigurationDefinition configurations = configurationsService.GetConfigurations();
					if (configurations != null)
					{
						return configurations.definitionId;
					}
				}
				return (string)null;
			};
			cache.Add(DtoProperty.DefinitionId, new CachedValue<string>(valueSetter6));
			Func<string> valueSetter7 = delegate
			{
				if (userSessionService != null)
				{
					UserSession userSession2 = userSessionService.UserSession;
					if (userSession2 != null)
					{
						return userSession2.UserID;
					}
				}
				return (string)null;
			};
			cache.Add(DtoProperty.UserId, new CachedValue<string>(valueSetter7));
			Func<string> valueSetter8 = delegate
			{
				if (userSessionService != null)
				{
					UserSession userSession = userSessionService.UserSession;
					if (userSession != null)
					{
						return userSession.SynergyID;
					}
				}
				return (string)null;
			};
			cache.Add(DtoProperty.SynergyId, new CachedValue<string>(valueSetter8));
			Func<IList<string>> valueSetter9 = delegate
			{
				string text = string.Empty;
				if (configurationsService != null)
				{
					text = configurationsService.GetDefinitionVariants();
				}
				return (text != null) ? text.Split(new char[1] { '_' }, StringSplitOptions.RemoveEmptyEntries) : null;
			};
			definitionVariantsCache = new CachedValue<IList<string>>(valueSetter9);
		}

		void ILogglyDtoCache.Initialize(IUserSessionService userSessionService, IConfigurationsService configurationsService)
		{
			this.userSessionService = userSessionService;
			this.configurationsService = configurationsService;
		}

		LogglyDto ILogglyDtoCache.GetCachedDto()
		{
			LogglyDto logglyDto = new LogglyDto();
			logglyDto.ClientVersion = cache[DtoProperty.ClientVersion].GetValue() ?? "unknown";
			logglyDto.ClientDeviceType = cache[DtoProperty.ClientDeviceType].GetValue() ?? "unknown";
			logglyDto.ClientPlatform = cache[DtoProperty.ClientPlatform].GetValue() ?? "unknown";
			logglyDto.UserId = cache[DtoProperty.UserId].GetValue() ?? "unknown";
			logglyDto.SynergyId = cache[DtoProperty.SynergyId].GetValue() ?? "unknown";
			logglyDto.ConfigUrl = cache[DtoProperty.ConfigUrl].GetValue() ?? "unknown";
			logglyDto.ConfigVariant = cache[DtoProperty.ConfigVariant].GetValue() ?? "unknown";
			logglyDto.DefinitionId = cache[DtoProperty.DefinitionId].GetValue() ?? "unknown";
			logglyDto.DefinitionVariants = definitionVariantsCache.GetValue() ?? new string[1] { "unknown" };
			return logglyDto;
		}

		void ILogglyDtoCache.RefreshConfigurationValues()
		{
			cache[DtoProperty.ConfigUrl].SetFresh(false);
			cache[DtoProperty.ConfigVariant].SetFresh(false);
			cache[DtoProperty.DefinitionId].SetFresh(false);
			definitionVariantsCache.SetFresh(false);
		}

		void ILogglyDtoCache.RefreshUserSessionValues()
		{
			cache[DtoProperty.UserId].SetFresh(false);
			cache[DtoProperty.SynergyId].SetFresh(false);
		}

		void ILogglyDtoCache.RefreshClientVersionValues()
		{
			cache[DtoProperty.ClientVersion].SetFresh(false);
			cache[DtoProperty.ClientDeviceType].SetFresh(false);
			cache[DtoProperty.ClientPlatform].SetFresh(false);
		}
	}
}
