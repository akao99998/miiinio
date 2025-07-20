using System.Collections;
using System.Collections.Generic;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Splash
{
	public class DownloadResumeCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("DownloadResumeCommand") as IKampaiLogger;

		private IEnumerator restartDownloadCoroutine;

		[Inject]
		public DLCModel dlcModel { get; set; }

		[Inject]
		public LaunchDownloadSignal launchSignal { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public AppPauseSignal pauseSignal { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		public override void Execute()
		{
			if (LoadState.Get() != LoadStateType.STARTED)
			{
				Native.LogInfo(string.Format("AppResume, DownloadResumeCommand, realtimeSinceStartup: {0}", timeService.RealtimeSinceStartup()));
			}
			if (LoadState.Get() != LoadStateType.DLC)
			{
				logger.Info("DownloadResumeCommand.Execute(): do not restart DLC downloading, it was not started yet.");
				return;
			}
			if (dlcModel.NeededBundles.Count == 0)
			{
				logger.Info("DownloadResumeCommand.Execute(): no more DLC needed.");
				return;
			}
			restartDownloadCoroutine = RestartDownload();
			routineRunner.StartCoroutine(restartDownloadCoroutine);
		}

		private IEnumerator RestartDownload()
		{
			pauseSignal.AddOnce(OnPause);
			yield return null;
			logger.Info("DownloadResumeCommand.RestartDownload(): restart DLC downloading.");
			Queue<IRequest> pendingRequests = dlcModel.PendingRequests;
			if (pendingRequests != null && pendingRequests.Count != 0)
			{
				pendingRequests.Clear();
			}
			List<IRequest> runningRequests = dlcModel.RunningRequests;
			if (runningRequests != null && runningRequests.Count != 0)
			{
				int retries = 10;
				while (runningRequests.Count != 0 && retries-- != 0)
				{
					logger.Info("DownloadResumeCommand.RestartDownload: wait for {0} request(s) [{1} retry(ies) left].", runningRequests.Count, retries);
					yield return new WaitForSeconds(0.1f);
				}
				runningRequests.Clear();
			}
			pauseSignal.RemoveListener(OnPause);
			restartDownloadCoroutine = null;
			logger.Info("DownloadResumeCommand.RestartDownload: launch DLC downloading.");
			launchSignal.Dispatch(dlcModel.ShouldLoadAudio);
		}

		private void OnPause()
		{
			if (restartDownloadCoroutine != null)
			{
				routineRunner.StopCoroutine(restartDownloadCoroutine);
				restartDownloadCoroutine = null;
			}
		}
	}
}
