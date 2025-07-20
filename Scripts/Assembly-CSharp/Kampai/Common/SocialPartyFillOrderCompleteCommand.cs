using Elevation.Logging;
using Kampai.Game;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Common
{
	public class SocialPartyFillOrderCompleteCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("SocialPartyFillOrderCompleteCommand") as IKampaiLogger;

		[Inject]
		public StuartTicketFilledSignal stuartTicketFilledSignal { get; set; }

		public override void Execute()
		{
			logger.Info("Fill Order Complete");
			stuartTicketFilledSignal.Dispatch();
		}
	}
}
