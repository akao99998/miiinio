using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Main;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Splash
{
	public class DownloadDLCPartCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("DownloadDLCPartCommand") as IKampaiLogger;

		[Inject]
		public IDownloadService downloadService { get; set; }

		[Inject]
		public DLCModel dlcModel { get; set; }

		public override void Execute()
		{
			if (dlcModel.PendingRequests == null || dlcModel.RunningRequests == null)
			{
				logger.Error("Attempting to run DLCPartCommand with no model!");
				return;
			}
			LoadState.Set(LoadStateType.DLC);
			while (dlcModel.PendingRequests.Count > 0 && dlcModel.RunningRequests.Count < 5)
			{
				IRequest request = dlcModel.PendingRequests.Dequeue();
				dlcModel.RunningRequests.Add(request);
				downloadService.Perform(request);
			}
		}
	}
}
