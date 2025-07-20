using System;
using System.Text;
using Kampai.Util;

namespace Kampai.Common
{
	public class LoggingTelemetryService : TelemetryService
	{
		[Inject]
		public ICoppaService coppaService { get; set; }

		public override void COPPACompliance()
		{
			base.COPPACompliance();
			string text = null;
			DateTime birthdate;
			if (!coppaService.GetBirthdate(out birthdate))
			{
				birthdate = DateTime.Now;
			}
			text = DateAsString(birthdate.Year, birthdate.Month);
			logger.Info("ageGateDob " + text);
		}

		public override void LogGameEvent(TelemetryEvent gameEvent)
		{
			base.LogGameEvent(gameEvent);
			if (gameEvent == null || !logger.IsAllowedLevel(KampaiLogLevel.Info))
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(gameEvent.Type.ToString());
			stringBuilder.Append(" - ");
			foreach (TelemetryParameter parameter in gameEvent.Parameters)
			{
				stringBuilder.Append(parameter.name).Append('(').Append(parameter.keyType)
					.Append(")=")
					.Append(parameter.value)
					.Append(", ");
			}
			logger.Info("Game event: {0}", stringBuilder.ToString());
		}

		protected string DateAsString(int year, int month)
		{
			return year + "-" + ((month >= 10) ? month.ToString() : ("0" + month));
		}
	}
}
