using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class CameraAutoMoveToPositionCommand : Command
	{
		[Inject]
		public Vector3 position { get; set; }

		[Inject]
		public float zoom { get; set; }

		[Inject]
		public bool useOffset { get; set; }

		[Inject]
		public CameraAutoMoveSignal autoMoveSignal { get; set; }

		public override void Execute()
		{
			Vector3 type = position;
			if (useOffset)
			{
				type += GameConstants.CAMERA_OFFSET_ACTIONABLE_OBJECT;
			}
			ScreenPosition screenPosition = new ScreenPosition();
			screenPosition.x = -1f;
			screenPosition.z = -1f;
			screenPosition.zoom = zoom;
			autoMoveSignal.Dispatch(type, new Boxed<ScreenPosition>(screenPosition), new CameraMovementSettings(CameraMovementSettings.Settings.None, null, null), true);
		}
	}
}
