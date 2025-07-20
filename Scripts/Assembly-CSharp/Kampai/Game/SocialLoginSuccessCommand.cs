using Elevation.Logging;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class SocialLoginSuccessCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("SocialLoginSuccessCommand") as IKampaiLogger;

		[Inject]
		public ISocialService socialService { get; set; }

		[Inject]
		public SocialInitSuccessSignal socialInitSuccess { get; set; }

		public override void Execute()
		{
			logger.Debug("Social Login Success");
			socialService.SendLoginTelemetry("Game Screen");
			socialInitSuccess.Dispatch(socialService);
		}
	}
}
