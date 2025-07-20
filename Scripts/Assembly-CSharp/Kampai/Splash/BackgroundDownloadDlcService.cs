using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Ea.Sharkbite.HttpPlugin.Http.Impl;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Common.Service.HealthMetrics;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Splash
{
	public class BackgroundDownloadDlcService : IBackgroundDownloadDlcService
	{
		private sealed class OnRequestListener : AndroidJavaProxy
		{
			private static readonly string NATIVE_INTERFACE = string.Format("{0}${1}", NATIVE_CLASS, typeof(OnRequestListener).Name);

			private RequestBundleCallback responseCallback;

			public OnRequestListener(RequestBundleCallback callback)
				: base(NATIVE_INTERFACE)
			{
				responseCallback = callback;
			}

			public void onResponseCallback(string url, string filePath, bool isGZipped, long downloadedContentLength, long expectedContentLength, int statusCode, string error)
			{
				if (responseCallback != null)
				{
					responseCallback(url, filePath, isGZipped, downloadedContentLength, expectedContentLength, statusCode, error);
				}
			}
		}

		private sealed class Invoker : IInvokerService
		{
			private Queue<Action> work = new Queue<Action>();

			private Mutex mutex = new Mutex(false);

			public void Add(Action a)
			{
				try
				{
					mutex.WaitOne();
					work.Enqueue(a);
				}
				finally
				{
					mutex.ReleaseMutex();
				}
			}

			public bool Update()
			{
				if (work.Count > 0)
				{
					try
					{
						mutex.WaitOne();
						if (work.Count > 0)
						{
							Action action = work.Dequeue();
							action();
							return true;
						}
					}
					finally
					{
						mutex.ReleaseMutex();
					}
				}
				return false;
			}
		}

		private delegate void RequestBundleCallback(string url, string filePath, bool isGZipped, long downloadedContentLength, long expectedContentLength, int statusCode, string error);

		private const int MAX_CONCURRENT_REQUESTS = 5;

		private static readonly string NATIVE_CLASS = string.Format("com.ea.gp.minions.nimble.{0}", typeof(BackgroundDownloadDlcService).Name);

		public IKampaiLogger logger = LogManager.GetClassLogger("BackgroundDownloadDlcService") as IKampaiLogger;

		private Queue<IRequest> pendingRequests;

		private List<IRequest> runningRequests = new List<IRequest>();

		private bool isNetworkWifi;

		private bool stopped = true;

		private bool isRunning;

		private Invoker invoker = new Invoker();

		private bool udpEnabled;

		private long downloadTotalSize;

		private DateTime downloadStartTime;

		private IEnumerator startDownloadCoroutine;

		private string deviceTypeUrlEscaped;

		private AndroidJavaClass nativeService;

		private AndroidJavaObject requestHeaders;

		private OnRequestListener onRequestListener;

		[Inject]
		public DLCModel dlcModel { get; set; }

		[Inject]
		public IManifestService manifestService { get; set; }

		[Inject]
		public IConfigurationsService configurationsService { get; set; }

		[Inject]
		public IClientHealthService clientHealthService { get; set; }

		[Inject]
		public IDLCService dlcService { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public IRequestFactory requestFactory { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistanceService { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public AppPauseSignal pauseSignal { get; set; }

		public bool Stopped
		{
			get
			{
				return stopped;
			}
		}

		[Inject]
		public IClientVersion clientVersion { get; set; }

		[Inject]
		public IInvokerService invokerService { get; set; }

		[PostConstruct]
		public void PostConstruct()
		{
			deviceTypeUrlEscaped = WWW.EscapeURL(clientVersion.GetClientDeviceType());
		}

		public void Start()
		{
			logger.Info("[BDLC] Start");
			startDownloadCoroutine = StartDownload();
			routineRunner.StartCoroutine(startDownloadCoroutine);
		}

		private IEnumerator StartDownload()
		{
			pauseSignal.AddOnce(OnPause);
			if (!stopped)
			{
				isRunning = false;
				do
				{
					logger.Info("[BDLC] Waiting for service to stop before starting it again...");
					yield return new WaitForSeconds(0.1f);
				}
				while (!stopped);
			}
			pauseSignal.RemoveListener(OnPause);
			startDownloadCoroutine = null;
			Init();
			ThreadPool.QueueUserWorkItem(delegate
			{
				Run();
			});
		}

		private void OnPause()
		{
			if (startDownloadCoroutine != null)
			{
				routineRunner.StopCoroutine(startDownloadCoroutine);
				startDownloadCoroutine = null;
			}
		}

		public void Stop()
		{
			logger.Info("[BDLC] Stop");
			isRunning = false;
		}

		private void Init()
		{
			downloadTotalSize = 0L;
			downloadStartTime = DateTime.Now;
			pendingRequests = CreateNetworkRequests(dlcModel.NeededBundles, manifestService.GetDLCURL());
			isNetworkWifi = NetworkUtil.IsNetworkWiFi();
			isRunning = pendingRequests.Count != 0;
			stopped = !isRunning;
			logger.Info("[BDLC] Init :: pendingRequests.Count = {0}", pendingRequests.Count);
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("K-Platform", clientVersion.GetClientPlatform());
			dictionary.Add("K-Device", deviceTypeUrlEscaped);
			dictionary.Add("K-Version", clientVersion.GetClientVersion());
			Dictionary<string, string> dictionary2 = dictionary;
			nativeService = new AndroidJavaClass("com.ea.gp.minions.nimble.BackgroundDownloadDlcService");
			requestHeaders = new AndroidJavaObject("java.util.HashMap");
			foreach (KeyValuePair<string, string> item in dictionary2)
			{
				requestHeaders.Call<string>("put", new object[2] { item.Key, item.Value });
			}
			if (onRequestListener == null)
			{
				onRequestListener = new OnRequestListener(OnRequestBundleCallbackProxy);
			}
			string dLC_PATH = GameConstants.DLC_PATH;
			if (!Directory.Exists(dLC_PATH))
			{
				Directory.CreateDirectory(dLC_PATH);
			}
		}

		private void Run()
		{
			AndroidJNI.AttachCurrentThread();
			while (isRunning)
			{
				if (!ProcessQueue())
				{
					Thread.Sleep(100);
				}
				bool flag = invoker.Update();
				if (pendingRequests.Count == 0 && runningRequests.Count == 0)
				{
					logger.Info("[BDLC] nothing to do, reconciling tier");
					int playerDLCTier = DLCService.GetPlayerDLCTier(localPersistanceService);
					if (playerDLCTier > dlcModel.HighestTierDownloaded)
					{
						logger.Info("[BDLC] setting tier {0} -> {1}", dlcModel.HighestTierDownloaded, playerDLCTier);
						telemetryService.Send_Telemetry_EVT_USER_GAME_LOAD_FUNNEL("60 - Downloaded DLC", "anyVariant", dlcService.GetDownloadQualityLevel());
						dlcModel.HighestTierDownloaded = playerDLCTier;
					}
					break;
				}
				if (flag && LoadState.Get() == LoadStateType.STARTED)
				{
					Thread.Sleep(2000);
				}
			}
			logger.Info("[BDLC] requesting stop");
			if (runningRequests.Count != 0)
			{
				AbortRunning();
				if (runningRequests.Count != 0)
				{
					runningRequests.Clear();
				}
			}
			pendingRequests.Clear();
			long num = (long)(DateTime.Now - downloadStartTime).TotalMilliseconds;
			logger.Info("DLC Download Speed Stats : DownloadedTotalTime: {0} , DownloadedTotalSize : {1}  ", num, downloadTotalSize);
			if (num > 0)
			{
				string text = ((!udpEnabled) ? "Download.Http" : "Download.Udp");
				float num2 = (float)downloadTotalSize / (float)num;
				clientHealthService.MarkTimerEvent(text, num2);
				logger.Info("DLC Download Speed Stats : eventname: {0} , downloadSpeed : {1} ", text, num2);
			}
			AndroidJNI.DetachCurrentThread();
			invokerService.Add(delegate
			{
				requestHeaders.Dispose();
				requestHeaders = null;
				nativeService.Dispose();
				nativeService = null;
				stopped = true;
			});
			logger.Info("[BDLC] Stopped");
		}

		private bool ProcessQueue()
		{
			if (isRunning && pendingRequests.Count > 0 && runningRequests.Count < 5 && (NetworkUtil.IsNetworkWiFi() || dlcModel.AllowDownloadOnMobileNetwork))
			{
				IRequest request = pendingRequests.Dequeue();
				runningRequests.Add(request);
				logger.Info("[BDLC] request: " + request.Uri);
				PrepareDirectory(request);
				nativeService.CallStatic("requestBundle", request.Uri, request.GetTempFilePath(), requestHeaders, request.UseGZip, onRequestListener);
				return true;
			}
			return false;
		}

		private void AbortRunning()
		{
			foreach (IRequest runningRequest in runningRequests)
			{
				runningRequest.Abort();
			}
			nativeService.CallStatic("abortRequest", string.Empty);
			logger.Info("[BDLC] finalizing running requests");
			int num = 100;
			while (runningRequests.Count > 0 && num-- > 0)
			{
				logger.Info("[BDLC] exiting {0} request(s) [time left: {1:0.##} s]", runningRequests.Count, (float)num / 10f);
				do
				{
					Thread.Sleep(100);
				}
				while (invoker.Update());
			}
			if (runningRequests.Count != 0)
			{
				logger.Error("[BDLC] unstopped requests: {0}", runningRequests.Count);
			}
		}

		private void OnRequestBundleCallbackProxy(string url, string tempFilePath, bool isGZipped, long downloadedContentLength, long expectedContentLength, int statusCode, string error)
		{
			invoker.Add(delegate
			{
				OnRequestBundleCallback(url, tempFilePath, isGZipped, downloadedContentLength, expectedContentLength, statusCode, error);
			});
		}

		private void OnRequestBundleCallback(string url, string tempFilePath, bool isGZipped, long downloadedContentLength, long expectedContentLength, int statusCode, string error)
		{
			IRequest request = null;
			foreach (IRequest runningRequest in runningRequests)
			{
				if (runningRequest.Uri == url)
				{
					request = runningRequest;
					break;
				}
			}
			if (request == null)
			{
				logger.Error("[BDLC] OnRequestBundleCallback(): Unable to find original request for URL = {0}", url);
				return;
			}
			bool flag = request.IsAborted();
			bool flag2 = !string.IsNullOrEmpty(error);
			if (File.Exists(tempFilePath) && !flag2)
			{
				error = (flag ? "Aborting file download." : DownloadUtil.UnpackFile(tempFilePath, request.FilePath, request.Md5, request.AvoidBackup));
				if (!string.IsNullOrEmpty(error))
				{
					statusCode = 418;
				}
			}
			else if (!flag2)
			{
				error = (flag ? "Aborting file download." : "Temp file doesn't exist upon download finish.");
				statusCode = 418;
			}
			if (File.Exists(tempFilePath))
			{
				File.Delete(tempFilePath);
			}
			HandleResponse(new FileDownloadResponse().WithError(error).WithCode((statusCode == 0) ? 408 : statusCode).WithRequest(request)
				.WithContentLength(expectedContentLength));
		}

		private void PrepareDirectory(IRequest request)
		{
			string tempFilePath = request.GetTempFilePath();
			if (File.Exists(tempFilePath))
			{
				File.Delete(tempFilePath);
			}
			string filePath = request.FilePath;
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}
			string directoryName = Path.GetDirectoryName(filePath);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
		}

		private void HandleResponse(IResponse response)
		{
			IRequest request = response.Request;
			runningRequests.Remove(request);
			string uri = request.Uri;
			long contentLength = response.ContentLength;
			logger.Info("[BDLC] url = {0}", uri);
			if (request.IsAborted())
			{
				logger.Info("[BDLC] aborted, url = {0}", uri);
				return;
			}
			if (!response.Success)
			{
				logger.Info("[BDLC] failure: code = {0}, error = {1}, url = {2}, enqueue request", response.Code, response.Error, uri);
				pendingRequests.Enqueue(request);
			}
			else
			{
				string bundleNameFromUrl = DownloadUtil.GetBundleNameFromUrl(request.Uri);
				if (!configurationsService.isKillSwitchOn(KillSwitch.DLC_TELEMETRY))
				{
					telemetryService.Send_Telemetry_EVT_USER_GAME_DOWNLOAD_FUNNEL(bundleNameFromUrl, response.DownloadTime, contentLength, isNetworkWifi);
				}
				logger.Info("[BDLC]: success: url = {0}", uri);
			}
			if (contentLength > 0)
			{
				downloadTotalSize += contentLength;
			}
		}

		private Queue<IRequest> CreateNetworkRequests(IList<BundleInfo> bundles, string baseDlcUrl)
		{
			string userId = UnityEngine.Random.Range(0, 100).ToString();
			udpEnabled = FeatureAccessUtil.isAccessible(AccessControlledFeature.AKAMAI_UDP, configurationsService.GetConfigurations(), userId, logger);
			Queue<IRequest> queue = new Queue<IRequest>(bundles.Count);
			foreach (BundleInfo bundle in bundles)
			{
				IRequest item = CreateRequest(bundle, baseDlcUrl, udpEnabled);
				queue.Enqueue(item);
			}
			return queue;
		}

		private IRequest CreateRequest(BundleInfo bundleInfo, string baseDlcUrl, bool udpEnabled)
		{
			string name = bundleInfo.name;
			string uri = DownloadUtil.CreateBundleURL(baseDlcUrl, name);
			string filePath = DownloadUtil.CreateBundlePath(GameConstants.DLC_PATH, name);
			return requestFactory.Resource(uri).WithOutputFile(filePath).WithMd5(bundleInfo.sum)
				.WithGZip(true)
				.WithUdp(udpEnabled);
		}
	}
}
