using System;
using System.Collections.Generic;
using System.Threading;
using Elevation.Logging;
using Kampai.Util;

namespace Kampai.Common
{
	public class NimbleTelemetrySender : ITelemetrySender
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("NimbleTelemetrySender") as IKampaiLogger;

		[Inject]
		public ICoppaService coppaService { get; set; }

		public NimbleTelemetrySender()
		{
			NimbleBridge_Log.GetComponent();
		}

		public virtual void COPPACompliance()
		{
			string text = null;
			DateTime birthdate;
			if (!coppaService.GetBirthdate(out birthdate))
			{
				birthdate = DateTime.Now;
			}
			text = DateAsString(birthdate.Year, birthdate.Month);
			NimbleBridge_Tracking.GetComponent().AddCustomSessionValue("ageGateDob", text);
		}

		public void SharingUsage(bool enabled)
		{
			logger.Info("=======================================================================================================================================================================================");
			logger.Info("=======================================================================================================================================================================================");
			logger.Info("#                                                                                                                                                                                     #");
			logger.Info("#                  setting Nimble sharingusage to {0}                                                                                                                             #", enabled);
			logger.Info("#                                                                                                                                                                                     #");
			logger.Info("=======================================================================================================================================================================================");
			ThreadPool.QueueUserWorkItem(delegate
			{
				NimbleBridge_Tracking.GetComponent().SetEnabled(enabled);
			}, null);
		}

		protected string DateAsString(int year, int month)
		{
			return year + "-" + ((month >= 10) ? month.ToString() : ("0" + month));
		}

		public virtual void SendEvent(TelemetryEvent gameEvent)
		{
			NimbleBridge_Tracking.GetComponent().LogEvent("SYNERGYTRACKING::CUSTOM", getNimbleParameters(gameEvent));
		}

		public static Dictionary<string, string> getNimbleParameters(TelemetryEvent telemetryEvent)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("eventType", ((int)telemetryEvent.Type).ToString());
			for (int i = 0; i < telemetryEvent.Parameters.Count; i++)
			{
				TelemetryParameter telemetryParameter = telemetryEvent.Parameters[i];
				if (telemetryParameter.keyType.Length > 0)
				{
					dictionary.Add("keyType" + (i + 1).ToString().PadLeft(2, '0'), ((int)Enum.Parse(typeof(SynergyTrackingEventKey), telemetryParameter.keyType)).ToString());
					dictionary.Add("keyValue" + (i + 1).ToString().PadLeft(2, '0'), (telemetryParameter.value != null) ? telemetryParameter.value.ToString() : string.Empty);
				}
			}
			return dictionary;
		}
	}
}
