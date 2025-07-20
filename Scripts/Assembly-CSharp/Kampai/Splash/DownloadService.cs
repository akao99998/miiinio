using System.Collections.Generic;
using System.Net;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Ea.Sharkbite.HttpPlugin.Http.Impl;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.Splash
{
	public class DownloadService : IDownloadService
	{
		public const int MAX_CONCURRENT_REQUESTS = 5;

		private Queue<IRequest> requestQueue;

		private LinkedList<IRequest> runningRequests;

		private bool logInfo;

		private LinkedList<Signal<IResponse>> globalResponseSignals;

		public IKampaiLogger logger = LogManager.GetClassLogger("DownloadService") as IKampaiLogger;

		private TimeService timeService;

		private string deviceTypeUrlEscaped;

		[Inject]
		public IInvokerService invoker { get; set; }

		[Inject]
		public NetworkModel networkModel { get; set; }

		[Inject]
		public NetworkConnectionLostSignal networkConnectionLostSignal { get; set; }

		[Inject]
		public ResumeNetworkOperationSignal resumeNetworkOperationSignal { get; set; }

		[Inject]
		public IClientVersion clientVersion { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject("game.server.host")]
		public string ServerUrl { get; set; }

		[Inject]
		public ITimeService timeServiceInstance { get; set; }

		public bool IsRunning { get; set; }

		private int requestCounter { get; set; }

		public DownloadService()
		{
			ServicePointManager.DefaultConnectionLimit = 5;
			IsRunning = true;
			requestQueue = new Queue<IRequest>();
			runningRequests = new LinkedList<IRequest>();
			globalResponseSignals = new LinkedList<Signal<IResponse>>();
			ConnectionSettings.ConnectionLimit = 5;
		}

		[PostConstruct]
		public void PostConstruct()
		{
			deviceTypeUrlEscaped = WWW.EscapeURL(clientVersion.GetClientDeviceType());
			timeService = timeServiceInstance as TimeService;
			resumeNetworkOperationSignal.AddListener(ProcessQueue);
		}

		public void Perform(IRequest request, bool forceRequest = false)
		{
			if (forceRequest)
			{
				DoPerform(request.WithRunInBackground(true));
				return;
			}
			invoker.Add(delegate
			{
				DoPerform(request);
			});
		}

		private void DoPerform(IRequest request)
		{
			logInfo = logger.IsAllowedLevel(KampaiLogLevel.Info);
			if (request.ProgressSignal != null)
			{
				if (!string.IsNullOrEmpty(request.FilePath))
				{
					request.RegisterNotifiable(ProgressCallback);
				}
				else
				{
					logger.Warning("Unable to notify if request is not notifiable");
				}
			}
			requestQueue.Enqueue(request.WithHeaderParam("K-Platform", clientVersion.GetClientPlatform()).WithHeaderParam("K-Device", deviceTypeUrlEscaped).WithHeaderParam("K-Version", clientVersion.GetClientVersion()));
			if (!request.IsRestarted())
			{
				ProcessQueue();
			}
		}

		public void AddGlobalResponseListener(Signal<IResponse> signal)
		{
			globalResponseSignals.AddLast(signal);
		}

		public void ProcessQueue()
		{
			if (!IsRunning || requestQueue.Count <= 0)
			{
				return;
			}
			if (!networkModel.isConnectionLost)
			{
				if (NetworkUtil.IsConnected())
				{
					if (runningRequests.Count < 5)
					{
						DoDownload(requestQueue.Dequeue());
					}
				}
				else
				{
					NetworkLost();
				}
				return;
			}
			IRequest request = null;
			int count = requestQueue.Count;
			while (count-- != 0)
			{
				IRequest request2 = requestQueue.Dequeue();
				if (request == null && request2.IsAborted() && !request2.IsRestarted())
				{
					request = request2;
				}
				else
				{
					requestQueue.Enqueue(request2);
				}
			}
			if (request != null)
			{
				DoDownload(request);
			}
		}

		private void NetworkLost()
		{
			if (!networkModel.isConnectionLost)
			{
				networkConnectionLostSignal.Dispatch();
			}
		}

		private void RetryDownload(IRequest request)
		{
			if (IsRunning)
			{
				logger.Warning("Failed to download {0}, {1} retries left, trying again...", request.Uri, request.RetryCount);
				request.RetryCount--;
				if (IsRunning && runningRequests.Count < 5)
				{
					DoDownload(request);
					return;
				}
				requestQueue.Enqueue(request);
				ProcessQueue();
			}
		}

		private void DoDownload(IRequest request)
		{
			int requestCount = request.requestCount;
			if (requestCount > 0)
			{
				if (requestCount < requestCounter)
				{
					logger.Warning("HTTP START ABORTED [Attempting to save old save:{0} over new save:{1}] {2}: {3}", requestCount.ToString(), requestCounter.ToString(), request.Method, request.Uri);
					return;
				}
				logger.Debug("Save Counter {0}", requestCount.ToString());
				requestCounter = requestCount;
			}
			else
			{
				logger.Debug("Save Counter Untracked");
			}
			runningRequests.AddLast(request);
			if (logInfo)
			{
				logger.Info("HTTP START [{0}] {1}: {2}", runningRequests.Count, request.Method, request.Uri);
			}
			request.Execute(RequestCallback);
		}

		private void ProgressCallbackProxy(DownloadProgress progress, IRequest request)
		{
			invoker.Add(delegate
			{
				ProgressCallback(progress, request);
			});
		}

		private void ProgressCallback(DownloadProgress progress, IRequest request)
		{
			if (!IsRunning)
			{
				Native.LogError("Ignoring HTTP progress (shutting down)");
			}
			else
			{
				NotifyProgress(progress, request);
			}
		}

		private void RequestCallbackProxy(IResponse response)
		{
			invoker.Add(delegate
			{
				RequestCallback(response);
			});
		}

		private void RequestCallback(IResponse response)
		{
			if (!IsRunning)
			{
				logger.Error("Ignoring HTTP response (shutting down)");
				return;
			}
			bool flag = false;
			string text = "unknown";
			bool flag2 = false;
			if (response != null)
			{
				flag = response.Success;
				if (response.IsConnectionLost)
				{
					NetworkLost();
				}
				IRequest request = response.Request;
				if (request != null)
				{
					runningRequests.Remove(request);
					text = request.Uri;
					flag2 = request.IsAborted();
					if (!flag)
					{
						logger.Warning("Error downloading {0} HTTP RESPONSE => {1} Error => {2}", text, response.Code, response.Error);
						if ((networkModel.isConnectionLost && !flag2) || request.IsRestarted())
						{
							requestQueue.Enqueue(request);
							return;
						}
						if (request.CanRetry && request.RetryCount > 0 && !flag2)
						{
							RetryDownload(request);
							return;
						}
					}
					else
					{
						if (text.Contains(ServerUrl))
						{
							timeService.SyncServerTime(response);
							localizationService.RetrieveCultureInfo(response);
						}
						if (logInfo)
						{
							logger.Info("Successfully downloaded " + text);
						}
					}
				}
				else
				{
					logger.Error("Null request on response");
				}
			}
			else
			{
				logger.Error("Null response");
				response = new DefaultResponse().WithCode(500).WithBody("Null response");
			}
			if (flag || !networkModel.isConnectionLost || flag2)
			{
				NotifyResponse(response);
			}
			if (logInfo)
			{
				logger.Info("HTTP END [" + runningRequests.Count + "] " + text);
			}
			ProcessQueue();
		}

		private void NotifyProgress(DownloadProgress progress, IRequest request)
		{
			if (IsRunning)
			{
				Signal<DownloadProgress, IRequest> progressSignal = request.ProgressSignal;
				if (progressSignal != null)
				{
					progressSignal.Dispatch(progress, request);
				}
			}
		}

		private void NotifyResponse(IResponse response)
		{
			if (!IsRunning)
			{
				return;
			}
			IRequest request = response.Request;
			if (request != null)
			{
				Signal<IResponse> responseSignal = request.ResponseSignal;
				if (responseSignal != null)
				{
					responseSignal.Dispatch(response);
				}
			}
			else
			{
				logger.Error("Null request on response");
			}
			foreach (Signal<IResponse> globalResponseSignal in globalResponseSignals)
			{
				globalResponseSignal.Dispatch(response);
			}
		}

		public void Shutdown()
		{
			IsRunning = false;
			ClearQueueAndAbortRunning();
			runningRequests.Clear();
			resumeNetworkOperationSignal.RemoveListener(ProcessQueue);
		}

		public void Abort()
		{
			foreach (IRequest item in requestQueue)
			{
				item.Abort();
				NotifyResponse(new DefaultResponse().WithCode(500).WithRequest(item));
			}
			ClearQueueAndAbortRunning();
		}

		private void ClearQueueAndAbortRunning()
		{
			requestQueue.Clear();
			foreach (IRequest runningRequest in runningRequests)
			{
				runningRequest.Abort();
			}
		}

		public void Restart()
		{
			foreach (IRequest runningRequest in runningRequests)
			{
				if (!runningRequest.IsAborted())
				{
					runningRequest.Restart();
				}
			}
		}
	}
}
