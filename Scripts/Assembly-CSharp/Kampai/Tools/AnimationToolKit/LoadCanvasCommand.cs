using UnityEngine;
using UnityEngine.UI;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Tools.AnimationToolKit
{
	public class LoadCanvasCommand : Command
	{
		[Inject(ContextKeys.CONTEXT_VIEW)]
		public GameObject ContextView { get; set; }

		public override void Execute()
		{
			GameObject gameObject = new GameObject("Canvas");
			gameObject.transform.SetParent(ContextView.transform, false);
			Canvas canvas = gameObject.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			base.injectionBinder.Bind<Canvas>().ToValue(canvas);
			gameObject.AddComponent<GraphicRaycaster>();
		}
	}
}
