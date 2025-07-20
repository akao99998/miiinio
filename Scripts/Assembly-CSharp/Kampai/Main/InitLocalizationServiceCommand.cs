using Elevation.Logging;
using Kampai.Splash;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Main
{
	public class InitLocalizationServiceCommand : Command
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("InitLocalizationServiceCommand") as IKampaiLogger;

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public LocalizationServiceInitializedSignal localizationServiceInitializedSignal { get; set; }

		[Inject(LocalizationServices.EVENT)]
		public ILocalizationService localEventService { get; set; }

		[Inject]
		public SplashProgressUpdateSignal splashProgressDoneSignal { get; set; }

		public override void Execute()
		{
			if (!localService.IsInitialized())
			{
				string deviceLanguage = Native.GetDeviceLanguage();
				logger.Info("Got language code {0}", deviceLanguage);
				localService.Initialize(deviceLanguage);
				localEventService.Initialize("EN-US");
				localizationServiceInitializedSignal.Dispatch();
				splashProgressDoneSignal.Dispatch(20, 5f);
			}
		}
	}
}
