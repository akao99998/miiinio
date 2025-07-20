using Elevation.Logging;
using Kampai.Common;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class KillSwitchChangedCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("KillSwitchChangedCommand") as IKampaiLogger;

		[Inject(SocialServices.FACEBOOK)]
		public ISocialService facebookService { get; set; }

		[Inject(SocialServices.GOOGLEPLAY)]
		public ISocialService gpService { get; set; }

		[Inject]
		public IConfigurationsService configService { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public NimbleTelemetrySender nimbleTelemetryService { get; set; }

		public override void Execute()
		{
			facebookService.updateKillSwitchFlag();
			gpService.updateKillSwitchFlag();
			if (configService.isKillSwitchOn(KillSwitch.SYNERGY))
			{
				logger.Info("=======================================================================================================================================================================================");
				logger.Info("=======================================================================================================================================================================================");
				logger.Info("#                                                                                                                                                                                     #");
				logger.Info("#                  SYNERGY KillSwitch ON                                                                                                                                              #");
				logger.Info("#                                                                                                                                                                                     #");
				logger.Info("=======================================================================================================================================================================================");
				telemetryService.SharingUsage(nimbleTelemetryService, false);
			}
			else
			{
				logger.Info("=======================================================================================================================================================================================");
				logger.Info("=======================================================================================================================================================================================");
				logger.Info("#                                                                                                                                                                                     #");
				logger.Info("#                  SYNERGY KillSwitch OFF                                                                                                                                             #");
				logger.Info("#                                                                                                                                                                                     #");
				logger.Info("=======================================================================================================================================================================================");
				telemetryService.SharingUsage(nimbleTelemetryService, true);
			}
		}
	}
}
