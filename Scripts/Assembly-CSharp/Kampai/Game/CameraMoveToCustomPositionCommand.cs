using System;
using Kampai.Common;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class CameraMoveToCustomPositionCommand : Command
	{
		[Inject]
		public int definitionID { get; set; }

		[Inject]
		public Boxed<Action> callback { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject(MainElement.CAMERA)]
		public Camera mainCamera { get; set; }

		[Inject]
		public DisableCameraBehaviourSignal disableCameraSignal { get; set; }

		[Inject]
		public EnableCameraBehaviourSignal enableCameraSignal { get; set; }

		[Inject]
		public PickControllerModel pickControllerModel { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playGlobalSoundFXSignal { get; set; }

		[Inject]
		public VillainLairModel villainLairModel { get; set; }

		public override void Execute()
		{
			bool restoreCameraControl = false;
			if (villainLairModel.cameraFlow != null)
			{
				villainLairModel.cameraFlow.destroy();
				villainLairModel.cameraFlow = null;
			}
			disableCameraSignal.Dispatch(1);
			disableCameraSignal.Dispatch(2);
			CustomCameraPositionDefinition customCameraPositionDefinition = definitionService.Get<CustomCameraPositionDefinition>(definitionID);
			restoreCameraControl = customCameraPositionDefinition.enableCameraControl;
			Vector3 endValue = new Vector3(customCameraPositionDefinition.xPos, customCameraPositionDefinition.yPos, customCameraPositionDefinition.zPos);
			Vector3 endValue2 = new Vector3(customCameraPositionDefinition.xRotation, customCameraPositionDefinition.yRotation, customCameraPositionDefinition.zRotation);
			float fOV = customCameraPositionDefinition.FOV;
			PositionTweenProperty position = new PositionTweenProperty(endValue);
			RotationTweenProperty rotation = new RotationTweenProperty(endValue2);
			if (!string.IsNullOrEmpty(customCameraPositionDefinition.panSound))
			{
				playGlobalSoundFXSignal.Dispatch(customCameraPositionDefinition.panSound);
			}
			GoTweenFlow goTweenFlow = CreateFlow(position, rotation, fOV, customCameraPositionDefinition.duration);
			goTweenFlow.setOnCompleteHandler(delegate
			{
				if (callback.Value != null)
				{
					callback.Value();
				}
				if (restoreCameraControl)
				{
					enableCameraSignal.Dispatch(1);
					enableCameraSignal.Dispatch(2);
				}
				pickControllerModel.PanningCameraBlocked = false;
			});
			pickControllerModel.PanningCameraBlocked = true;
			goTweenFlow.play();
			villainLairModel.cameraFlow = goTweenFlow;
			if (customCameraPositionDefinition.nearClip > 0f)
			{
				mainCamera.nearClipPlane = customCameraPositionDefinition.nearClip;
			}
			if (customCameraPositionDefinition.farClip > 0f)
			{
				mainCamera.farClipPlane = customCameraPositionDefinition.farClip;
			}
		}

		private GoTweenFlow CreateFlow(AbstractTweenProperty position, RotationTweenProperty rotation, float fieldOfView, float duration)
		{
			GoTweenConfig config = new GoTweenConfig().addTweenProperty(position).addTweenProperty(rotation).setEaseType(GoEaseType.SineOut);
			GoTweenConfig config2 = new GoTweenConfig().floatProp("fieldOfView", fieldOfView).setEaseType(GoEaseType.SineOut);
			GoTween tween = new GoTween(mainCamera.transform, duration, config);
			GoTween tween2 = new GoTween(mainCamera, duration, config2);
			return new GoTweenFlow().insert(0f, tween).insert(0f, tween2);
		}
	}
}
