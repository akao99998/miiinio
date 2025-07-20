using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class CameraZoomBeachCommand : Command
	{
		[Inject]
		public IZoomCameraModel zoomCameraModel { get; set; }

		public override void Execute()
		{
			if (!zoomCameraModel.ZoomedIn && !zoomCameraModel.ZoomInProgress)
			{
				Vector3 type = new Vector3(131.1315f, 15.4357f, 162.0232f);
				CameraMovementSettings type2 = new CameraMovementSettings(CameraMovementSettings.Settings.Default, null, null);
				ScreenPosition screenPosition = new ScreenPosition();
				screenPosition.zoom = 0.8567233f;
				base.injectionBinder.GetInstance<CameraAutoMoveSignal>().Dispatch(type, new Boxed<ScreenPosition>(screenPosition), type2, true);
			}
		}
	}
}
