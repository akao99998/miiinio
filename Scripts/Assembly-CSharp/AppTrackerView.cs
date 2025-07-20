using Kampai.Common;
using Kampai.Util;
using strange.extensions.mediation.impl;

public class AppTrackerView : View
{
	private bool isInitialized;

	public AppPauseSignal pauseSignal { get; set; }

	public AppEarlyPauseSignal earlyPauseSignal { get; set; }

	public AppResumeSignal resumeSignal { get; set; }

	public AppQuitSignal quitSignal { get; set; }

	public AppFocusGainedSignal focusGainedSignal { get; set; }

	public IKampaiLogger logger { get; set; }

	public void SetIsInitialized(bool isInitialized)
	{
		this.isInitialized = isInitialized;
	}

	public void OnApplicationPause(bool isPausing)
	{
		if (isPausing)
		{
			if (isInitialized)
			{
				pauseSignal.Dispatch();
			}
			earlyPauseSignal.Dispatch();
		}
		else if (isInitialized)
		{
			resumeSignal.Dispatch();
		}
		TimeProfiler.Flush();
	}

	public void OnApplicationQuit()
	{
		if (isInitialized)
		{
			quitSignal.Dispatch();
		}
		TimeProfiler.Flush();
	}

	public void OnApplicationFocus(bool hasFocus)
	{
		if (hasFocus && isInitialized)
		{
			focusGainedSignal.Dispatch();
		}
	}
}
