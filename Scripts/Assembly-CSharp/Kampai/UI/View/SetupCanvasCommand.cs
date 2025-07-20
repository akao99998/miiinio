using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using strange.extensions.injector.api;

namespace Kampai.UI.View
{
	public class SetupCanvasCommand : Command
	{
		private string name;

		private MainElement mainElement;

		private Camera worldCamera;

		[Inject]
		public Tuple<string, MainElement, Camera> args { get; set; }

		public override void Execute()
		{
			name = args.Item1;
			mainElement = args.Item2;
			worldCamera = args.Item3;
			GameObject gameObject = new GameObject(name);
			switch (mainElement)
			{
			case MainElement.UI_GLASSCANVAS:
			case MainElement.UI_OVERLAY_CANVAS:
			{
				gameObject.layer = 5;
				bool flag = mainElement == MainElement.UI_OVERLAY_CANVAS;
				if (!flag)
				{
					gameObject.AddComponent<CanvasRenderer>();
				}
				AddCanvas(gameObject, (!flag) ? RenderMode.ScreenSpaceCamera : RenderMode.ScreenSpaceOverlay);
				AddCanvasScaler(gameObject);
				base.injectionBinder.injector.Inject(gameObject.AddComponent<KampaiRaycaster>());
				break;
			}
			case MainElement.UI_WORLDCANVAS:
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.AddComponent<CanvasRenderer>();
				AddCanvas(gameObject, RenderMode.WorldSpace);
				gameObject.AddComponent<KampaiWorldRaycaster>();
				break;
			}
			IInjectionBinding binding = base.injectionBinder.GetBinding<ICrossContextCapable>(UIElement.CONTEXT);
			Transform transform = gameObject.transform;
			IInjectionBinder obj;
			if (binding != null)
			{
				IInjectionBinder injectionBinder = ((ICrossContextCapable)binding.value).injectionBinder;
				obj = injectionBinder;
			}
			else
			{
				obj = base.injectionBinder;
			}
			transform.SetParent(obj.GetInstance<GameObject>(ContextKeys.CONTEXT_VIEW).transform, false);
			base.injectionBinder.Bind<GameObject>().ToValue(gameObject).ToName(mainElement)
				.CrossContext()
				.Weak();
		}

		private void SetupDooberCanvas(Canvas glassCanvas)
		{
			GameObject gameObject = new GameObject();
			gameObject.name = "DooberCanvas";
			gameObject.transform.SetParent(glassCanvas.transform, false);
			Canvas canvas = gameObject.AddComponent<Canvas>();
			canvas.overrideSorting = true;
			canvas.sortingOrder = 1;
			RectTransform rectTransform = gameObject.transform as RectTransform;
			if (rectTransform != null)
			{
				rectTransform.anchoredPosition3D = new Vector3(0f, 0f, -1000f);
				rectTransform.sizeDelta = Vector2.zero;
				rectTransform.anchorMin = Vector2.zero;
				rectTransform.anchorMax = Vector2.one;
			}
			gameObject.layer = 5;
			base.injectionBinder.Bind<GameObject>().ToValue(gameObject).ToName(MainElement.UI_DOOBER_CANVAS)
				.CrossContext()
				.Weak();
		}

		private void AddCanvas(GameObject canvasGO, RenderMode renderMode)
		{
			Canvas canvas = canvasGO.AddComponent<Canvas>();
			canvas.renderMode = renderMode;
			canvas.planeDistance = 25f;
			if (worldCamera != null)
			{
				canvas.worldCamera = worldCamera;
			}
			if (mainElement == MainElement.UI_OVERLAY_CANVAS)
			{
				canvas.sortingOrder = 32767;
			}
			if (mainElement == MainElement.UI_GLASSCANVAS)
			{
				SetupDooberCanvas(canvas);
			}
		}

		private void AddCanvasScaler(GameObject canvasGO)
		{
			CanvasScaler canvasScaler = canvasGO.AddComponent<CanvasScaler>();
			canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			canvasScaler.matchWidthOrHeight = 1f;
			canvasScaler.referenceResolution = new Vector2(960f, 640f);
		}
	}
}
