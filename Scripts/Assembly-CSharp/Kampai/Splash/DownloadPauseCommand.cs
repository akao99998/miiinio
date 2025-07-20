using System.Collections.Generic;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Splash
{
	public class DownloadPauseCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("DownloadPauseCommand") as IKampaiLogger;

		[Inject]
		public DLCModel dlcModel { get; set; }

		[Inject]
		public NetworkModel networkModel { get; set; }

		[Inject]
		public IDownloadService downloadService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		public override void Execute()
		{
			if (LoadState.Get() != LoadStateType.STARTED)
			{
				Native.LogInfo(string.Format("AppPause, DownloadPauseCommand, realtimeSinceStartup: {0}", timeService.RealtimeSinceStartup()));
			}
			if (LoadState.Get() != LoadStateType.DLC)
			{
				logger.Info("DownloadPauseCommand.Execute(): do not abort DLC downloading, it was not started yet.");
				return;
			}
			if (dlcModel.NeededBundles.Count == 0)
			{
				logger.Info("DownloadPauseCommand.Execute(): no DLC to abort.");
				return;
			}
			logger.Info("DownloadPauseCommand.Execute(): abort DLC downloading.");
			Queue<IRequest> pendingRequests = dlcModel.PendingRequests;
			if (pendingRequests != null && pendingRequests.Count != 0)
			{
				pendingRequests.Clear();
			}
			List<IRequest> runningRequests = dlcModel.RunningRequests;
			if (runningRequests == null || runningRequests.Count == 0)
			{
				return;
			}
			foreach (IRequest item in runningRequests)
			{
				item.Abort();
			}
			if (networkModel.isConnectionLost)
			{
				downloadService.ProcessQueue();
			}
		}
	}
}
