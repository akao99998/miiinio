using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Elevation.Logging;
using ICSharpCode.SharpZipLib.GZip;
using Kampai.Common;
using Kampai.Game;
using Kampai.Splash;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Main
{
	public class PostDownloadManifestCommand : Command
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("PostDownloadManifestCommand") as IKampaiLogger;

		[Inject]
		public SetupManifestSignal setupManifestSignal { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public ReconcileDLCSignal reconcileDLCSignal { get; set; }

		[Inject]
		public DLCModel dlcModel { get; set; }

		[Inject]
		public IDLCService dlcService { get; set; }

		[Inject]
		public IManifestService manifestService { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public CheckAvailableStorageSignal checkAvailableStorageSignal { get; set; }

		[Inject]
		public LoginUserSignal loginSignal { get; set; }

		[Inject]
		public LaunchDownloadSignal launchDownloadSignal { get; set; }

		[Inject]
		public IVideoService videoService { get; set; }

		[Inject]
		public IBackgroundDownloadDlcService backgroundDownloadDlcService { get; set; }

		[Inject]
		public IConfigurationsService configurationsService { get; set; }

		[Inject]
		public IAssetsPreloadService assetsPreloadService { get; set; }

		public override void Execute()
		{
			TimeProfiler.EndSection("retrieve manifest");
			logger.Info("PostDownloadManifestCommand setup manifest");
			telemetryService.Send_Telemetry_EVT_USER_GAME_LOAD_FUNNEL("30 - Loaded DLC Manifest", playerService.SWRVEGroup, dlcService.GetDownloadQualityLevel());
			try
			{
				setupManifestSignal.Dispatch();
			}
			catch (FatalException ex)
			{
				logger.FatalNoThrow(ex.FatalCode, ex.ReferencedId, "Message: {0}, Reason: {1}", ex.Message, (ex.InnerException == null) ? ex.ToString() : ex.InnerException.ToString());
				return;
			}
			IList<BundleInfo> unstreamablePackagedBundlesList = manifestService.GetUnstreamablePackagedBundlesList();
			if (unstreamablePackagedBundlesList.Count > 0)
			{
				routineRunner.StartCoroutine(CopyStreamingAssets(unstreamablePackagedBundlesList));
			}
			else
			{
				CompleteManifestDownload();
			}
		}

		private void CompleteManifestDownload()
		{
			logger.Debug("[Manifest] CompleteManifestDownload");
			routineRunner.StartCoroutine(WaitAFrame(delegate
			{
				reconcileDLCSignal.Dispatch(true);
				ulong num = dlcModel.TotalSize;
				if (ShouldPlayVideo() && !videoService.IsIntroCached())
				{
					num += 5242880;
				}
				if (num != 0L)
				{
					checkAvailableStorageSignal.Dispatch(GameConstants.PERSISTENT_DATA_PATH, num, TryPlayVideoStartDownloadDLC);
				}
				else
				{
					TryPlayVideoStartDownloadDLC();
				}
			}));
		}

		private void TryPlayVideoStartDownloadDLC()
		{
			logger.Debug("[Manifest] TryPlayVideoStartDownloadDLC");
			bool flag = dlcModel.NeededBundles.Count == 0;
			if (ShouldPlayVideo())
			{
				PlayVideo((!flag) ? new Action(VideoStartedPlayingCallback) : null);
			}
			else if (!flag)
			{
				DownloadDlcInForeground();
			}
			if (flag)
			{
				assetsPreloadService.PreloadAllAssets();
				loginSignal.Dispatch();
			}
		}

		private void VideoStartedPlayingCallback()
		{
			logger.Info("[Manifest] Video playing; starting background DLC");
			backgroundDownloadDlcService.Start();
			logger.Info("[Manifest] Waiting for video to finish");
			routineRunner.StartCoroutine(StopBackgroundDlcDownloading(6));
		}

		private void DownloadDlcInForeground()
		{
			logger.Info("[Manifest] Downloading DLC in foreground");
			launchDownloadSignal.Dispatch(false);
		}

		private IEnumerator StopBackgroundDlcDownloading(int frames)
		{
			while (frames-- > 0)
			{
				yield return new WaitForEndOfFrame();
			}
			LoadState.Set(LoadStateType.BOOTING);
			logger.Info("[Manifest]: stopping downloading and wait until it finished");
			backgroundDownloadDlcService.Stop();
			while (!backgroundDownloadDlcService.Stopped)
			{
				logger.Info("[Manifest]: waiting");
				yield return new WaitForSeconds(0.1f);
			}
			logger.Info("[Manifest]: background downloading finished, reconcile DLC again.");
			telemetryService.Send_Telemetry_EVT_USER_GAME_LOAD_FUNNEL("50 - Played Intro Video", "anyVariant", dlcService.GetDownloadQualityLevel());
			CompleteManifestDownload();
		}

		private bool ShouldPlayVideo()
		{
			if (!PlayerPrefs.HasKey("intro_video_played"))
			{
				logger.Info("[Manifest] PostDownloadManifestCommand.ShouldPlayVideo: {0}", true);
				return true;
			}
			int @int = PlayerPrefs.GetInt("intro_video_played");
			logger.Info("[Manifest] PostDownloadManifestCommand.ShouldPlayVideo: {0}", @int == 0);
			return @int == 0;
		}

		private void PlayVideo(Action callback)
		{
			bool dEBUG_ENABLED = GameConstants.StaticConfig.DEBUG_ENABLED;
			logger.Info("[Manifest] PostDownloadManifestCommand.PlayVideo skippable: {0}", dEBUG_ENABLED);
			videoService.playIntro(false, dEBUG_ENABLED, callback, configurationsService.GetConfigurations().videoUri);
		}

		private IEnumerator WaitAFrame(Action a)
		{
			yield return null;
			a();
		}

		private IEnumerator CopyStreamingAssets(IList<BundleInfo> unstreambleBundles)
		{
			logger.Info("[Manifest] Copying streaming assets: {0}", unstreambleBundles.Count.ToString());
			string StreamingAssetsDLCPath = Path.Combine(Application.streamingAssetsPath, "DLC");
			if (!Directory.Exists(GameConstants.DLC_PATH))
			{
				Directory.CreateDirectory(GameConstants.DLC_PATH);
			}
			byte[] buffer = new byte[4096];
			foreach (BundleInfo bundle in unstreambleBundles)
			{
				string dlcPathName = Path.Combine(GameConstants.DLC_PATH, bundle.name + ".unity3d");
				bool dlcFileExists = File.Exists(dlcPathName);
				bool validDlcFound = dlcFileExists && new FileInfo(dlcPathName).Length == (long)bundle.size;
				if (dlcFileExists && !validDlcFound)
				{
					File.Delete(dlcPathName);
				}
				if (!validDlcFound)
				{
					byte[] results = null;
					string filePath = Path.Combine(StreamingAssetsDLCPath, bundle.originalName);
					logger.Debug("[Manifest] Copying: {0}", filePath);
					if (filePath.Contains("://"))
					{
						WWW www = new WWW(filePath);
						yield return www;
						if (www.error == null)
						{
							results = www.bytes;
						}
						else
						{
							logger.Info("[Manifest] Error copying {0} from WWW", filePath);
						}
					}
					else if (File.Exists(filePath))
					{
						results = File.ReadAllBytes(filePath);
					}
					if (results != null)
					{
						logger.Info("[Manifest] Saving {0} to {1} ({2} bytes)", bundle.originalName, dlcPathName, results.Length);
						using (FileStream file = new FileStream(dlcPathName, FileMode.Create, FileAccess.Write))
						{
							using (MemoryStream ms = new MemoryStream(results))
							{
								Stream inputStream = ((!bundle.isZipped) ? ((Stream)ms) : ((Stream)new GZipInputStream(ms)));
								try
								{
									int totalBytesRead = 0;
									for (int bytesRead = inputStream.Read(buffer, 0, buffer.Length); bytesRead > 0; bytesRead = inputStream.Read(buffer, 0, buffer.Length))
									{
										totalBytesRead += bytesRead;
										logger.Info("[Manifest] {0} - {1}", bundle.originalName, totalBytesRead);
										file.Write(buffer, 0, bytesRead);
									}
								}
								finally
								{
									if (bundle.isZipped)
									{
										inputStream.Dispose();
									}
								}
							}
						}
						yield return null;
					}
					else
					{
						logger.Info("[Manifest] No data for {0}", bundle.originalName);
					}
				}
				else
				{
					logger.Info("[Manifest] {0} already exists.", bundle.name);
				}
			}
			CompleteManifestDownload();
		}
	}
}
