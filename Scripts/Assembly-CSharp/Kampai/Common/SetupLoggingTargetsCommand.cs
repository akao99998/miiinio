using System.Collections.Generic;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Elevation.Logging.Targets;
using Kampai.Game;
using Kampai.Main;
using Kampai.Splash;
using Kampai.Util;
using Kampai.Util.Logging.Hosted;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Common
{
	public class SetupLoggingTargetsCommand : Command
	{
		[Inject]
		public IDownloadService downloadService { get; set; }

		[Inject]
		public IClientVersion clientVersion { get; set; }

		[Inject]
		public IRequestFactory requestFactory { get; set; }

		[Inject]
		public ILogglyDtoCache logglyDtoCache { get; set; }

		[Inject]
		public IUserSessionService userSessionService { get; set; }

		[Inject]
		public IConfigurationsService configurationsService { get; set; }

		[Inject("game.server.environment")]
		public string serverEnvironment { get; set; }

		[Inject]
		public ConfigurationsLoadedSignal configurationsLoadedSignal { get; set; }

		[Inject]
		public UserSessionGrantedSignal userSessionGrantedSignal { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistService { get; set; }

		[Inject]
		public AppEarlyPauseSignal appEarlyPauseSignal { get; set; }

		[Inject]
		public LogClientMetricsSignal clientMetricsSignal { get; set; }

		[Inject(BaseElement.CONTEXT)]
		public ICrossContextCapable baseContext { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		public override void Execute()
		{
			LogManager.RegisterTarget("UnityEngine.Debug", UnityEditorTarget.Build);
			LogManager.RegisterTarget("Kampai.Fatal", BuildFatal);
			LogManager.RegisterTarget("Kampai.Loggly", BuildLoggly);
			LogManager.RegisterTarget("Kampai.Native", KampaiNativeTarget.Build);
			LogManager.Instance.SetConfig(GameConstants.StaticConfig.LOGGING_CONFIG);
		}

		private KampaiFatalTarget BuildFatal(Dictionary<string, object> config)
		{
			KampaiFatalTarget kampaiFatalTarget = new KampaiFatalTarget(baseContext, localService, clientMetricsSignal, "Kampai.Fatal");
			kampaiFatalTarget.UpdateConfig(config);
			return kampaiFatalTarget;
		}

		private KampaiLogglyTarget BuildLoggly(Dictionary<string, object> config)
		{
			KampaiLogglyTarget kampaiLogglyTarget = new KampaiLogglyTarget("Kampai.Loggly", LogLevel.Trace);
			kampaiLogglyTarget.Initialize(downloadService, requestFactory, userSessionService, configurationsService, serverEnvironment, clientVersion, logglyDtoCache, localPersistService, configurationsLoadedSignal, userSessionGrantedSignal, appEarlyPauseSignal);
			kampaiLogglyTarget.UpdateConfig(config);
			return kampaiLogglyTarget;
		}
	}
}
