using System.Collections;
using System.Collections.Generic;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Common.Service.HealthMetrics;
using Kampai.Game;
using Kampai.Splash;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace Kampai.Common.Controller
{
	public class LogTapEventMetricsCommand : Command
	{
		private const int MAX_INTERVALS = 5;

		private const int INTERVAL_LENGTH_SECONDS = 30;

		private const string TIMER_METRICS_RESOURCE = "/rest/healthMetrics/timers";

		private IKampaiLogger logger = LogManager.GetClassLogger("LogTapEventMetricsCommand") as IKampaiLogger;

		[Inject]
		public ITapEventMetricsService tapEventMetricsService { get; set; }

		[Inject]
		public IDownloadService downloadService { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public IConfigurationsService configService { get; set; }

		[Inject]
		public IUserSessionService userService { get; set; }

		[Inject]
		public IRequestFactory requestFactory { get; set; }

		public override void Execute()
		{
			if (ClientHealthUtil.isHealthMetricsEnabled(configService, userService))
			{
				routineRunner.StartCoroutine(PeriodicSendTapEventMetrics());
			}
		}

		private IEnumerator PeriodicSendTapEventMetrics()
		{
			for (int intervalCount = 1; intervalCount <= 5; intervalCount++)
			{
				yield return new WaitForSeconds(30f);
				SendTapEventMetrics(intervalCount);
			}
		}

		private void SendTapEventMetrics(int intervalCount)
		{
			Signal<IResponse> signal = new Signal<IResponse>();
			signal.AddOnce(SendTapEventMetricsComplete);
			string key = string.Format("AppFlow.Tap.Interval{0}", intervalCount);
			int count = tapEventMetricsService.Count;
			Dictionary<string, float> dictionary = new Dictionary<string, float>();
			dictionary.Add(key, count);
			Dictionary<string, float> timerEvents = dictionary;
			ClientTimerMetricsDTO clientTimerMetricsDTO = new ClientTimerMetricsDTO();
			clientTimerMetricsDTO.timerEvents = timerEvents;
			ClientTimerMetricsDTO entity = clientTimerMetricsDTO;
			downloadService.Perform(requestFactory.Resource(GameConstants.Server.SERVER_URL + "/rest/healthMetrics/timers").WithContentType("application/json").WithMethod("POST")
				.WithEntity(entity)
				.WithResponseSignal(signal));
		}

		private void SendTapEventMetricsComplete(IResponse response)
		{
			logger.Log(KampaiLogLevel.Info, "Post Tap Event Metrics response: " + response.Code);
			if (response.Success)
			{
				tapEventMetricsService.Clear();
			}
			else
			{
				logger.Warning("SendTapEventMetricsComplete response failed");
			}
		}
	}
}
