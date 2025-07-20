using Elevation.Logging;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using strange.extensions.command.impl;

namespace Kampai.Splash
{
	public class SplashStartCommand : Command
	{
		[Inject]
		public SplashProgressUpdateSignal splashProgressUpdateSignal { get; set; }

		[Inject(MainElement.MANAGER_PARENT)]
		public GameObject managers { get; set; }

		public override void Execute()
		{
			LogManager.RegisterLogger(KampaiLoggerV2.BuildingKampaiLogger);
			IKampaiLogger kampaiLogger = LogManager.GetClassLogger("SplashStartCommand") as IKampaiLogger;
			TimeProfiler.InitializeLogger(kampaiLogger);
			kampaiLogger.Debug("Loggly test: This log should have a user ID attached in Loggly!");
			GameObject gameObject = GameObject.Find("SplashRoot");
			GameObject gameObject2 = gameObject.FindChild("ToolTip");
			gameObject2.AddComponent<LoadInTipView>();
			GameObject gameObject3 = gameObject.FindChild("meter_bar");
			gameObject3.AddComponent<LoadingBarView>();
			SetupBindings();
			splashProgressUpdateSignal.Dispatch(10, 10f);
			SceneManager.LoadScene("Main", LoadSceneMode.Additive);
			GameObject gameObject4 = gameObject.FindChild("LogoPrefab");
			LogoPanelView logoPanelView = gameObject4.AddComponent<LogoPanelView>();
			logoPanelView.SetNoWifiPanel(gameObject.FindChild("popup_NoWiFi"));
		}

		public void SetupBindings()
		{
			GameObject gameObject = new GameObject("AppTracker");
			gameObject.transform.parent = managers.transform;
			gameObject.AddComponent<AppTrackerView>();
		}
	}
}
