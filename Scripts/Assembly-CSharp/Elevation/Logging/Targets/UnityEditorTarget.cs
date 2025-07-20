using System.Collections.Generic;

namespace Elevation.Logging.Targets
{
	public class UnityEditorTarget : BaseLoggingTarget
	{
		public UnityEditorTarget(LogLevel level)
			: base("UnityEngine.Debug", level)
		{
		}

		public override void WriteLogEvent(LogEvent logEvent)
		{
		}

		public static UnityEditorTarget Build(Dictionary<string, object> config)
		{
			UnityEditorTarget unityEditorTarget = new UnityEditorTarget(LogLevel.None);
			unityEditorTarget.UpdateConfig(config);
			return unityEditorTarget;
		}
	}
}
