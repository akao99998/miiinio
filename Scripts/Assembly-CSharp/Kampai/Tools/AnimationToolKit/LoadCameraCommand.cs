using Kampai.Game.View;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Tools.AnimationToolKit
{
	public class LoadCameraCommand : Command
	{
		public override void Execute()
		{
			Camera main = Camera.main;
			base.injectionBinder.Bind<Camera>().ToValue(main);
			CameraUtils o = main.gameObject.AddComponent<CameraUtils>();
			base.injectionBinder.Bind<CameraUtils>().ToValue(o).ToSingleton();
			main.gameObject.AddComponent<TouchPanView>();
			main.gameObject.AddComponent<TouchZoomView>();
			main.gameObject.AddComponent<TouchDragPanView>();
		}
	}
}
