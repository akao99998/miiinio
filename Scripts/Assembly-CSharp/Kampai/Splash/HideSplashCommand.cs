using Elevation.Logging;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Splash
{
	public class HideSplashCommand : Command
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("HideSplashCommand") as IKampaiLogger;

		[Inject(ContextKeys.CONTEXT_VIEW)]
		public GameObject contextView { get; set; }

		[Inject]
		public AppStartCompleteSignal appStartCompleteSignal { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		public override void Execute()
		{
			logger.EventStart("HideSplashCommand.Execute");
			Native.LogInfo(string.Format("HideSplash, realtimeSinceStartup: {0}", timeService.RealtimeSinceStartup()));
			for (int i = 0; i < contextView.transform.childCount; i++)
			{
				Object.Destroy(contextView.transform.GetChild(i).gameObject);
			}
			logger.EventStop("HideSplashCommand.Execute");
			TimeProfiler.EndSection("main");
			appStartCompleteSignal.Dispatch();
			TimeProfiler.StopMonoProfiler();
		}
	}
}
