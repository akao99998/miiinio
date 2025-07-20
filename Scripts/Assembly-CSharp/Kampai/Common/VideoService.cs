using System;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Game;
using Kampai.Main;
using Kampai.Splash;
using Kampai.Util;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.Common
{
	public class VideoService : IVideoService
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("VideoService") as IKampaiLogger;

		private string locale;

		private VideoRequest request;

		[Inject]
		public SplashProgressUpdateSignal splashProgressUpdateSignal { get; set; }

		[Inject]
		public SetSplashProgressSignal setSplashProgressSignal { get; set; }

		[Inject]
		public DLCModel dlcModel { get; set; }

		[Inject]
		public ShowNoWiFiPanelSignal showNoWiFiPanelSignal { get; set; }

		[Inject]
		public NetworkModel networkModel { get; set; }

		[Inject]
		public IDownloadService downloadService { get; set; }

		[Inject]
		public IDLCService dlcService { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public IRequestFactory requestFactory { get; set; }

		[PostConstruct]
		public void PostConstruct()
		{
			locale = HALService.GetResourcePath(Native.GetDeviceLanguage());
			if (string.IsNullOrEmpty(locale))
			{
				locale = HALService.GetResourcePath("en");
			}
			if (string.IsNullOrEmpty(locale))
			{
				locale = "EN-US";
			}
		}

		public void playVideo(string urlOrFilename, bool showControls, bool closeOnTouch)
		{
			logger.Info("[Video] Playing {0}", urlOrFilename);
			FullScreenMovieControlMode controlMode = FullScreenMovieControlMode.Hidden;
			if (showControls)
			{
				controlMode = FullScreenMovieControlMode.Minimal;
			}
			else if (closeOnTouch)
			{
				controlMode = FullScreenMovieControlMode.CancelOnInput;
			}
			Handheld.PlayFullScreenMovie(urlOrFilename, Color.black, controlMode, FullScreenMovieScalingMode.AspectFit);
		}

		public void playIntro(bool showControls, bool closeOnTouch, Action videoPlayingCallback = null, string videoUriTemplate = null)
		{
			if (request == null)
			{
				LoadState.Set(LoadStateType.INTRO);
				request = new VideoRequest();
				request.showControls = showControls;
				request.closeOnTouch = closeOnTouch;
				request.callback = videoPlayingCallback;
				request.videoUriTemplate = videoUriTemplate;
				fetchAndPlayIntroVideo();
			}
			else
			{
				logger.Error("[Video] Intro already playing");
			}
		}

		private string GetIntroVideoUri(string template = null)
		{
			return string.Format(template ?? "https://eaassets-a.akamaihd.net/cdn-kampai/videos/intro_{0}.mp4", locale);
		}

		private void fetchAndPlayIntroVideo()
		{
			if (request == null)
			{
				logger.Error("[Video] Null request for intro");
			}
			else if (IsIntroCached(request.videoUriTemplate))
			{
				logger.Info("[Video] Cached: {0}", GetIntroVideoUri(request.videoUriTemplate));
				if (request.callback != null)
				{
					logger.Info("[Video] CALLBACK");
					request.callback();
				}
				bool showControls = request.showControls;
				bool closeOnTouch = request.closeOnTouch;
				request = null;
				PlayerPrefs.SetInt("intro_video_played", 1);
				playVideo(GameConstants.VIDEO_PATH, showControls, closeOnTouch);
			}
			else
			{
				BeginVideoDownload();
			}
		}

		private void BeginVideoDownload()
		{
			string introVideoUri = GetIntroVideoUri(request.videoUriTemplate);
			logger.Info("[Video] Requesting: {0}", introVideoUri);
			Signal<DownloadProgress, IRequest> signal = new Signal<DownloadProgress, IRequest>();
			signal.AddListener(UpdateProgressBar);
			Signal<IResponse> signal2 = new Signal<IResponse>();
			signal2.AddListener(RequestCallback);
			VideoRequest videoRequest = request;
			IRequest obj = requestFactory.Resource(introVideoUri).WithOutputFile(GameConstants.VIDEO_PATH).WithProgressSignal(signal)
				.WithResponseSignal(signal2);
			int retries = request.retries;
			videoRequest.networkRequest = obj.WithRetry(true, retries).WithResume(true).WithAvoidBackup(true);
			if (networkModel.reachability == NetworkReachability.ReachableViaCarrierDataNetwork && !dlcModel.AllowDownloadOnMobileNetwork)
			{
				request.networkRequest.Restart();
				showNoWiFiPanelSignal.Dispatch(true);
			}
			request.progressBarStart = request.progressBarNow;
			downloadService.Perform(request.networkRequest);
		}

		public bool IsIntroCached(string videoUriTemplate = null)
		{
			return PlayerPrefs.HasKey("VideoCache") && GetIntroVideoUri(videoUriTemplate) == PlayerPrefs.GetString("VideoCache");
		}

		private void SetIntroCached(string videoUriTemplate = null)
		{
			PlayerPrefs.SetString("VideoCache", GetIntroVideoUri(videoUriTemplate));
		}

		private void RequestCallback(IResponse response)
		{
			if (response.Success)
			{
				telemetryService.Send_Telemetry_EVT_USER_GAME_DOWNLOAD_FUNNEL(response.Request.Uri, response.DownloadTime, response.ContentLength, NetworkUtil.IsNetworkWiFi());
				telemetryService.Send_Telemetry_EVT_USER_GAME_LOAD_FUNNEL("40 - Downloaded Intro Video", "anyVariant", dlcService.GetDownloadQualityLevel());
				SetIntroCached(request.videoUriTemplate);
			}
			else
			{
				logger.Error("[Video] Error fetching video {0}", response.Code);
			}
			setSplashProgressSignal.Dispatch(30f);
			fetchAndPlayIntroVideo();
		}

		private void UpdateProgressBar(DownloadProgress progress, IRequest networkRequest)
		{
			long totalBytes = progress.TotalBytes;
			if (totalBytes > 0)
			{
				int num = request.progressBarStart + (int)((100f - (float)request.progressBarStart) * progress.GetProgress());
				int num2 = num - request.progressBarNow;
				logger.Info("[Video] Progress: {0}/{1} {2}", progress.CompletedBytes, totalBytes, num2);
				if (num2 > 0)
				{
					splashProgressUpdateSignal.Dispatch(num2, 1f);
					request.progressBarNow = num;
				}
			}
			else
			{
				logger.Warning("[Video] No progress bar with unknown length");
			}
		}
	}
}
