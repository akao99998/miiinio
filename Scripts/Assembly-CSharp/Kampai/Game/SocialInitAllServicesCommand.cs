using Elevation.Logging;
using Kampai.Common;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class SocialInitAllServicesCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("SocialInitAllServicesCommand") as IKampaiLogger;

		[Inject(SocialServices.FACEBOOK)]
		public ISocialService facebookService { get; set; }

		[Inject(SocialServices.GOOGLEPLAY)]
		public ISocialService gpService { get; set; }

		[Inject]
		public SocialInitSignal socialInitSignal { get; set; }

		[Inject]
		public ICoppaService coppaService { get; set; }

		public override void Execute()
		{
			logger.EventStart("SocialInitAllServicesCommand.Execute");
			socialInitSignal.Dispatch(facebookService);
			if (!coppaService.Restricted())
			{
				socialInitSignal.Dispatch(gpService);
			}
			logger.EventStop("SocialInitAllServicesCommand.Execute");
		}
	}
}
