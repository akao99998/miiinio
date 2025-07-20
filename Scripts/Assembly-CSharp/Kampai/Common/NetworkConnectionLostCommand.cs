using Elevation.Logging;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Common
{
	public class NetworkConnectionLostCommand : Command
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("NetworkConnectionLostCommand") as IKampaiLogger;

		[Inject]
		public NetworkModel networkModel { get; set; }

		[Inject]
		public ShowOfflinePopupSignal showOfflinePopupSignal { get; set; }

		public override void Execute()
		{
			logger.Info("NetworkConnectionLostCommand");
			networkModel.isConnectionLost = true;
			showOfflinePopupSignal.Dispatch(true);
		}
	}
}
