using System.Collections;
using System.Text;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Splash;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class NimbleOTCommand : Command
	{
		private const string NIMBLE_OT_RESOURCE = "/rest/healthMetrics/nimbleOT";

		private static int NIMBLE_POLL_INTERVAL = 120;

		public IKampaiLogger logger = LogManager.GetClassLogger("NimbleOTCommand") as IKampaiLogger;

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public IDownloadService downloadService { get; set; }

		[Inject("game.server.host")]
		public string ServerUrl { get; set; }

		[Inject]
		public IConfigurationsService configurationsService { get; set; }

		[Inject]
		public IUserSessionService userSessionService { get; set; }

		[Inject]
		public IRequestFactory requestFactory { get; set; }

		public override void Execute()
		{
			routineRunner.StartCoroutine(PeriodicReloadConfigs());
		}

		private IEnumerator PeriodicReloadConfigs()
		{
			if (ClientHealthUtil.isHealthMetricsEnabled(configurationsService, userSessionService))
			{
				while (true)
				{
					pollNimbleOT();
					yield return new WaitForSeconds(NIMBLE_POLL_INTERVAL);
				}
			}
			logger.Warning("Disabling operational telemetry due to configurations");
		}

		private void pollNimbleOT()
		{
			NimbleBridge_OperationalTelemetryEvent[] events = NimbleBridge_OperationalTelemetryDispatch.GetComponent().GetEvents("com.ea.nimble.network");
			string text = "{\"events\":[";
			int num = 0;
			NimbleBridge_OperationalTelemetryEvent[] array = events;
			foreach (NimbleBridge_OperationalTelemetryEvent nimbleBridge_OperationalTelemetryEvent in array)
			{
				text += nimbleBridge_OperationalTelemetryEvent.GetEventDictionary();
				if (num < events.Length - 1)
				{
					text += ",";
				}
				num++;
				nimbleBridge_OperationalTelemetryEvent.Dispose();
			}
			events = null;
			text += "]}";
			if (num > 0)
			{
				logger.Log(KampaiLogLevel.Info, "Sending " + num + " Nimble OT events");
				UTF8Encoding uTF8Encoding = new UTF8Encoding(false);
				byte[] bytes = uTF8Encoding.GetBytes(text.ToCharArray());
				downloadService.Perform(requestFactory.Resource(ServerUrl + "/rest/healthMetrics/nimbleOT").WithContentType("application/json").WithMethod("POST")
					.WithBody(bytes));
			}
		}
	}
}
