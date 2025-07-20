using Elevation.Logging;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class SocialInitCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("SocialInitCommand") as IKampaiLogger;

		[Inject]
		public ISocialService socialService { get; set; }

		[Inject]
		public SocialInitSuccessSignal initSuccess { get; set; }

		[Inject]
		public SocialInitFailureSignal initFailure { get; set; }

		public override void Execute()
		{
			logger.Debug("Social Init Command Called With {0}", socialService.type.ToString());
			socialService.Init(initSuccess, initFailure);
		}
	}
}
