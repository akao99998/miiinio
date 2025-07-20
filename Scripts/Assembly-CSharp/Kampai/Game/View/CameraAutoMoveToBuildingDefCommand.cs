using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game.View
{
	public class CameraAutoMoveToBuildingDefCommand : Command
	{
		[Inject]
		public BuildingDefinition def { get; set; }

		[Inject]
		public Vector3 position { get; set; }

		[Inject]
		public PanInstructions panInstructions { get; set; }

		[Inject]
		public CameraAutoMoveSignal autoMoveSignal { get; set; }

		public override void Execute()
		{
			ScreenPosition screenPosition = new ScreenPosition();
			if (def.ScreenPosition != null)
			{
				screenPosition = screenPosition.Clone(def.ScreenPosition);
			}
			Boxed<Vector3> offset = panInstructions.Offset;
			Boxed<float> zoomDistance = panInstructions.ZoomDistance;
			if (zoomDistance != null)
			{
				if (screenPosition == null)
				{
					screenPosition = new ScreenPosition();
				}
				screenPosition.zoom = zoomDistance.Value;
			}
			Vector3 type = ((offset != null) ? (offset.Value + position) : position);
			autoMoveSignal.Dispatch(type, new Boxed<ScreenPosition>(screenPosition), new CameraMovementSettings(CameraMovementSettings.Settings.None, null, null), false);
		}
	}
}
