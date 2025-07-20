using Elevation.Logging;
using Kampai.Main;
using Kampai.Splash;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using strange.extensions.injector.api;
using strange.extensions.injector.impl;

namespace Kampai.UI.Controller
{
	public class ShowOfflinePopupCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("ShowOfflinePopupCommand") as IKampaiLogger;

		[Inject]
		public bool isShow { get; set; }

		[Inject]
		public SetupCanvasSignal setupCanvasSignal { get; set; }

		[Inject(SplashElement.CONTEXT)]
		public ICrossContextCapable splashContext { get; set; }

		public override void Execute()
		{
			IGUIService iGUIService = null;
			try
			{
				iGUIService = splashContext.injectionBinder.GetInstance<IGUIService>();
			}
			catch (InjectionException ex)
			{
				logger.Debug("Caught exception when attempting to Get GUIService: {0}", ex);
			}
			if (iGUIService != null)
			{
				if (isShow)
				{
					IGUICommand command = iGUIService.BuildCommand(GUIOperation.Load, "popup_Error_LostConnectivity");
					iGUIService.Execute(command);
				}
				else
				{
					iGUIService.Execute(GUIOperation.Unload, "popup_Error_LostConnectivity");
				}
				return;
			}
			Canvas overlayCanvas = GetOverlayCanvas();
			if (overlayCanvas == null && !isShow)
			{
				return;
			}
			OfflineView[] componentsInChildren = overlayCanvas.gameObject.GetComponentsInChildren<OfflineView>(true);
			if (componentsInChildren.Length == 0)
			{
				if (isShow)
				{
					string path = "UI/popup_Error_LostConnectivity";
					GameObject gameObject = Object.Instantiate(Resources.Load(path)) as GameObject;
					gameObject.transform.SetParent(overlayCanvas.transform, false);
				}
			}
			else if (!isShow)
			{
				Object.Destroy(componentsInChildren[0].gameObject);
			}
			else if (!componentsInChildren[0].gameObject.activeSelf)
			{
				componentsInChildren[0].gameObject.SetActive(true);
			}
			else if (isShow)
			{
				string path2 = "UI/popup_Error_LostConnectivity";
				GameObject gameObject2 = Object.Instantiate(Resources.Load(path2)) as GameObject;
				gameObject2.transform.SetParent(overlayCanvas.transform, false);
			}
		}

		private Canvas GetOverlayCanvas()
		{
			GameObject gameObject = null;
			IInjectionBinding binding = base.injectionBinder.GetBinding<GameObject>(MainElement.UI_OVERLAY_CANVAS);
			if (binding == null || binding.value == null || ((GameObject)binding.value).gameObject == null)
			{
				if (binding != null)
				{
					base.injectionBinder.Unbind(binding);
				}
				if (isShow)
				{
					setupCanvasSignal.Dispatch(Tuple.Create<string, MainElement, Camera>("OverlayCanvas", MainElement.UI_OVERLAY_CANVAS, null));
					gameObject = base.injectionBinder.GetInstance<GameObject>(MainElement.UI_OVERLAY_CANVAS);
				}
			}
			else
			{
				gameObject = (GameObject)binding.value;
			}
			return (!(gameObject != null)) ? null : gameObject.GetComponent<Canvas>();
		}
	}
}
