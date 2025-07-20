using System.Collections.Generic;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Common.Service.HealthMetrics;
using Kampai.Game;
using Kampai.Splash;
using Kampai.Util;
using Newtonsoft.Json;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace Kampai.Common.Controller
{
	public class LogClientMetricsCommand : Command
	{
		private sealed class ClientMeterMetricsDTO : IFastJSONSerializable
		{
			public Dictionary<string, int> meterEvents { get; set; }

			public void Serialize(JsonWriter writer)
			{
				writer.WriteStartObject();
				writer.WritePropertyName("meterEvents");
				writer.WriteStartObject();
				Dictionary<string, int>.Enumerator enumerator = meterEvents.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<string, int> current = enumerator.Current;
						writer.WritePropertyName(current.Key);
						writer.WriteValue(current.Value);
					}
				}
				finally
				{
					enumerator.Dispose();
				}
				writer.WriteEndObject();
				writer.WriteEndObject();
			}
		}

		private const string METER_METRICS_RESOURCE = "/rest/healthMetrics/meters";

		private const string TIMER_METRICS_RESOURCE = "/rest/healthMetrics/timers";

		private IKampaiLogger logger = LogManager.GetClassLogger("LogClientMetricsCommand") as IKampaiLogger;

		[Inject]
		public bool forceRequest { get; set; }

		[Inject]
		public IClientHealthService clientHealthService { get; set; }

		[Inject]
		public IDownloadService downloadService { get; set; }

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
				SendClientMeterMetrics();
				SendClientTimerMetrics();
			}
		}

		private void SendClientMeterMetrics()
		{
			Signal<IResponse> signal = new Signal<IResponse>();
			signal.AddListener(MeterMetricPOSTComplete);
			Dictionary<string, int> meterEvents = clientHealthService.MeterEvents;
			if (meterEvents != null && meterEvents.Count > 0)
			{
				ClientMeterMetricsDTO clientMeterMetricsDTO = new ClientMeterMetricsDTO();
				clientMeterMetricsDTO.meterEvents = meterEvents;
				downloadService.Perform(requestFactory.Resource(GameConstants.Server.SERVER_URL + "/rest/healthMetrics/meters").WithContentType("application/json").WithMethod("POST")
					.WithEntity(clientMeterMetricsDTO)
					.WithResponseSignal(signal), forceRequest);
			}
		}

		private void MeterMetricPOSTComplete(IResponse response)
		{
			logger.Log(KampaiLogLevel.Info, "Post Health Metrics response: " + response.Code);
			if (response.Success)
			{
				clientHealthService.ClearMeterEvents();
			}
			else
			{
				logger.Warning("MeterMetricPOSTComplete failed response");
			}
		}

		private void SendClientTimerMetrics()
		{
			Signal<IResponse> signal = new Signal<IResponse>();
			signal.AddOnce(SendClientTimerMetricsComplete);
			Dictionary<string, float> timerEvents = clientHealthService.TimerEvents;
			if (timerEvents != null && timerEvents.Count > 0)
			{
				ClientTimerMetricsDTO clientTimerMetricsDTO = new ClientTimerMetricsDTO();
				clientTimerMetricsDTO.timerEvents = timerEvents;
				ClientTimerMetricsDTO entity = clientTimerMetricsDTO;
				downloadService.Perform(requestFactory.Resource(GameConstants.Server.SERVER_URL + "/rest/healthMetrics/timers").WithContentType("application/json").WithMethod("POST")
					.WithEntity(entity)
					.WithResponseSignal(signal), forceRequest);
			}
		}

		private void SendClientTimerMetricsComplete(IResponse response)
		{
			logger.Log(KampaiLogLevel.Info, "Post Health Metrics response: " + response.Code);
			if (response.Success)
			{
				clientHealthService.ClearTimerEvents();
			}
			else
			{
				logger.Warning("SendClientTimerMetricsComplete failed response");
			}
		}
	}
}
