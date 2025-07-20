using Elevation.Logging;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Main
{
	public class LoadLocalizationServiceCommand : Command
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("LoadLocalizationServiceCommand") as IKampaiLogger;

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject(LocalizationServices.EVENT)]
		public ILocalizationService localEventService { get; set; }

		public override void Execute()
		{
			logger.Debug("Start loading localization Service");
			logger.EventStart("LoadLocalizationServiceCommand.Execute");
			if (localService.IsInitialized())
			{
				localService.Update();
			}
			else
			{
				logger.Fatal(FatalCode.CMD_LOAD_LOCALIZATION, "Localization service hasn't been initialized yet.");
			}
			if (localEventService.IsInitialized())
			{
				localEventService.Update();
			}
			else
			{
				logger.Error("Event Localization service hasn't been initialized yet.");
			}
			logger.EventStop("LoadLocalizationServiceCommand.Execute");
		}
	}
}
