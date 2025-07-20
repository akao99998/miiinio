using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class ProgressBarSettings : WorldToGlassUISettings
	{
		public int StartTime { get; set; }

		public int Duration { get; set; }

		public Signal<int> ConstructionCompleteSignal { get; set; }

		public ProgressBarSettings(int trackedId, Signal<int> constructionCompleteSignal, int startTime, int duration)
			: base(trackedId)
		{
			StartTime = startTime;
			Duration = duration;
			ConstructionCompleteSignal = constructionCompleteSignal;
		}
	}
}
