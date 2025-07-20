using Elevation.Logging;
using UnityEngine;

namespace Kampai.Util
{
	public class FPSUtil
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("FPSUtil") as IKampaiLogger;

		private int fpsHeartbeat = 90;

		private bool subscribed;

		private int loggingLimit = 10;

		private int currentLogging;

		private int currentFpsCount;

		private float actualFPS;

		private float dt;

		[Inject]
		public IUpdateRunner updateRunner { get; set; }

		public void SetFpsHeartbeat(int heartbeat)
		{
			fpsHeartbeat = heartbeat;
			if (fpsHeartbeat > 0 && !subscribed)
			{
				updateRunner.Subscribe(Update);
				subscribed = true;
			}
			else if (subscribed)
			{
				updateRunner.Unsubscribe(Update);
				subscribed = false;
			}
		}

		private void Update()
		{
			currentFpsCount++;
			dt += Time.deltaTime;
			if (currentFpsCount >= fpsHeartbeat)
			{
				actualFPS = (float)currentFpsCount / dt;
				currentFpsCount = 0;
				dt = 0f;
				logger.Info("FPS: {0}", actualFPS.ToString());
				currentLogging++;
				if (currentLogging >= loggingLimit)
				{
					SetFpsHeartbeat(0);
				}
			}
		}
	}
}
