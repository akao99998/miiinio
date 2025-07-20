using strange.extensions.command.impl;

namespace Kampai.Common
{
	public class LogTelemetryCommand : Command
	{
		[Inject]
		public TelemetryEvent gameEvent { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		public override void Execute()
		{
			telemetryService.LogGameEvent(gameEvent);
		}
	}
}
