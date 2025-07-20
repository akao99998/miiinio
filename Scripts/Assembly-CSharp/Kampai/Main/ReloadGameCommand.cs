using Elevation.Logging;
using Kampai.Common;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Main
{
	public class ReloadGameCommand : Command
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("ReloadGameCommand") as IKampaiLogger;

		[Inject]
		public ReInitializeGameSignal reInitializeGameSignal { get; set; }

		public override void Execute()
		{
			logger.Warning("Reloading Game");
			reInitializeGameSignal.Dispatch(string.Empty);
		}
	}
}
