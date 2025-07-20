using Elevation.Logging;
using Kampai.Main;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.ForcedUpgrade
{
	public class ForcedUpgradeStartCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("ForcedUpgradeStartCommand") as IKampaiLogger;

		[Inject]
		public InitLocalizationServiceSignal initLocalizationServiceSignal { get; set; }

		public override void Execute()
		{
			logger.Info("ForcedUpgrade scene starting...");
			initLocalizationServiceSignal.Dispatch();
		}
	}
}
