using System.Collections.Generic;
using System.Text;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Elevation.Logging.Targets;
using Kampai.Common;
using Kampai.Game;
using Kampai.Splash;
using Kampai.Util;
using Kampai.Util.Logging.Hosted;

namespace Kampai.Main
{
	public class KampaiLogglyTarget : LogglyTarget
	{
		private const string TOKEN = "05946d11-d631-48e1-a74e-b344673d86f9";

		private const string TAG = "client.{0},{1}";

		private const int RETRY_ATTEMPTS = 1;

		private const int DEFAULT_SEND_RATE = 180;

		private const bool DEFAULT_KILL_SWITCH_STATE = false;

		private const int DEFAULT_MAX_BUFFER_SIZE_BYTES = 1048576;

		private const bool DEFAULT_LOGGING_ENABLED_STATE = true;

		private IDownloadService downloadService;

		private IRequestFactory requestFactory;

		private ILogglyDtoCache logglyDtoCache;

		private LogglyDto logglyDto;

		private bool newUser;

		private string logglyTag;

		public KampaiLogglyTarget(string name, LogLevel level, string logFolder = null)
			: base(name, level, 180, 1048576, logFolder, "05946d11-d631-48e1-a74e-b344673d86f9", null)
		{
		}

		public void Initialize(IDownloadService downloadService, IRequestFactory requestFactory, IUserSessionService userSessionService, IConfigurationsService configurationsService, string serverEnvironment, IClientVersion clientVersion, ILogglyDtoCache dtoCache, ILocalPersistanceService localPersistService, ConfigurationsLoadedSignal configurationsLoadedSignal, UserSessionGrantedSignal userSessionGrantedSignal, AppEarlyPauseSignal earlyPauseSignal)
		{
			this.downloadService = downloadService;
			this.requestFactory = requestFactory;
			logglyDtoCache = dtoCache;
			logglyTag = string.Format("client.{0},{1}", serverEnvironment, clientVersion.GetClientPlatform());
			logglyDtoCache.Initialize(userSessionService, configurationsService);
			logglyDtoCache.RefreshClientVersionValues();
			logglyDtoCache.RefreshConfigurationValues();
			logglyDtoCache.RefreshUserSessionValues();
			string data = localPersistService.GetData("UserID");
			newUser = string.IsNullOrEmpty(data);
			SetupCache();
			configurationsLoadedSignal.AddListener(OnConfigurationsLoaded);
			userSessionGrantedSignal.AddListener(OnUserSessionGranted);
			earlyPauseSignal.AddListener(AppEarlyPaused);
		}

		protected override void SendRequest(byte[] bytes)
		{
			downloadService.Perform(requestFactory.Resource(_url).WithHeaderParam("X-LOGGLY-TAG", logglyTag).WithContentType("application/json")
				.WithMethod("POST")
				.WithBody(bytes)
				.WithGZip(true)
				.WithRetry(true, 1));
		}

		protected override void SerializeProperties(StringBuilder sb, LogEvent logEvent)
		{
			if (logglyDto != null)
			{
				SerializeProperty(sb, "userId", logglyDto.UserId);
				SerializeProperty(sb, "clientVersion", logglyDto.ClientVersion);
				SerializeProperty(sb, "clientDeviceType", logglyDto.ClientDeviceType);
				SerializeProperty(sb, "clientPlatform", logglyDto.ClientPlatform);
				SerializeProperty(sb, "newUser", logglyDto.NewUser);
				SerializeProperty(sb, "synergyId", logglyDto.SynergyId);
				SerializeProperty(sb, "configUrl", logglyDto.ConfigUrl);
				SerializeProperty(sb, "configVariant", logglyDto.ConfigVariant);
				SerializeProperty(sb, "definitionId", logglyDto.DefinitionId);
			}
		}

		protected void SerializeProperty(StringBuilder sb, string name, object value)
		{
			sb.AppendFormat("\"{0}\":\"{1}\",", name, value);
		}

		public void OnConfigurationsLoaded(bool init)
		{
			if (!base.Disposed && logglyDtoCache != null)
			{
				logglyDtoCache.RefreshConfigurationValues();
				SetupCache();
			}
		}

		public void AppEarlyPaused()
		{
			if (!base.Disposed)
			{
				Flush();
			}
		}

		public void OnUserSessionGranted()
		{
			if (!base.Disposed && logglyDtoCache != null)
			{
				logglyDtoCache.RefreshUserSessionValues();
				SetupCache();
				Flush();
			}
		}

		private void SetupCache()
		{
			logglyDto = logglyDtoCache.GetCachedDto();
			logglyDto.NewUser = newUser.ToString();
		}

		public override void UpdateConfig(Dictionary<string, object> config)
		{
			if (!base.Disposed)
			{
				base.UpdateConfig(config);
			}
		}
	}
}
