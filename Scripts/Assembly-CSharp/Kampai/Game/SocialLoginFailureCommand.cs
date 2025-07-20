using Elevation.Logging;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class SocialLoginFailureCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("SocialLoginFailureCommand") as IKampaiLogger;

		[Inject]
		public ISocialService socialService { get; set; }

		public override void Execute()
		{
			logger.Debug("{0} Login Failed", socialService.type.ToString());
		}
	}
}
