using Elevation.Logging;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class CameraAutoMoveToInstanceCommand : Command
	{
		private const float ZOOM_LEVEL_ACTIONABLE_OBJECT = 0.6f;

		public IKampaiLogger logger = LogManager.GetClassLogger("CameraAutoMoveToInstanceCommand") as IKampaiLogger;

		private Vector3 CAMERA_OFFSET_VILLAIN = new Vector3(-2f, 0f, 1.3f);

		private Vector3 CAMERA_OFFSET_TIKIBAR = new Vector3(-2.5f, 0f, 2.5f);

		[Inject]
		public PanInstructions panInstructions { get; set; }

		[Inject]
		public Boxed<ScreenPosition> boxedScreenPosition { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public CameraAutoMoveSignal autoMoveSignal { get; set; }

		[Inject]
		public ShowHiddenBuildingsSignal showHiddenBuildingsSignal { get; set; }

		[Inject]
		public CameraAutoMoveToBuildingSignal buildingMoveSignal { get; set; }

		public override void Execute()
		{
			showHiddenBuildingsSignal.Dispatch();
			Instance instance = panInstructions.Instance;
			if (instance == null)
			{
				instance = playerService.GetByInstanceId<Instance>(panInstructions.InstanceId);
			}
			Building building = instance as Building;
			if (building != null)
			{
				buildingMoveSignal.Dispatch(building, panInstructions);
				return;
			}
			ActionableObject fromAllObjects = ActionableObjectManagerView.GetFromAllObjects(instance.ID);
			if (fromAllObjects == null)
			{
				logger.Error("CameraAutoMoveToInstanceCommand: Cannot find object {0} {1}", instance, instance.GetType());
				return;
			}
			Boxed<Vector3> offset = panInstructions.Offset;
			Boxed<float> zoomDistance = panInstructions.ZoomDistance;
			Vector3 type = fromAllObjects.transform.position;
			CharacterObject characterObject = fromAllObjects as CharacterObject;
			if (characterObject != null)
			{
				if (characterObject is VillainView)
				{
					type = characterObject.GetIndicatorPosition() + CAMERA_OFFSET_VILLAIN;
				}
				else if (characterObject is PhilView)
				{
					type += CAMERA_OFFSET_TIKIBAR;
				}
				else
				{
					type = characterObject.GetIndicatorPosition();
				}
			}
			if (offset != null)
			{
				type += offset.Value;
			}
			CameraMovementSettings type2 = ((panInstructions.CameraMovementSettings != null) ? panInstructions.CameraMovementSettings : new CameraMovementSettings(CameraMovementSettings.Settings.None, null, null));
			ScreenPosition screenPosition = new ScreenPosition();
			if (boxedScreenPosition != null)
			{
				screenPosition = screenPosition.Clone(boxedScreenPosition.Value);
			}
			if (zoomDistance != null)
			{
				screenPosition.zoom = zoomDistance.Value;
			}
			autoMoveSignal.Dispatch(type, new Boxed<ScreenPosition>(screenPosition), type2, false);
		}
	}
}
