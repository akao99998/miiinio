using Elevation.Logging;
using Kampai.Common;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class CameraMoveToCustomLairPlotCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("CameraMoveToCustomLairPlotCommand") as IKampaiLogger;

		[Inject]
		public Vector3 cameraPosition { get; set; }

		[Inject(MainElement.CAMERA)]
		public Camera mainCamera { get; set; }

		[Inject]
		public DisableCameraBehaviourSignal disableCameraSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal sfxSignal { get; set; }

		[Inject]
		public PickControllerModel pickControllerModel { get; set; }

		[Inject]
		public VillainLairModel villainLairModel { get; set; }

		public override void Execute()
		{
			if (villainLairModel.cameraFlow != null)
			{
				int id = villainLairModel.cameraFlow.id;
				villainLairModel.cameraFlow.destroy();
				villainLairModel.cameraFlow = null;
				logger.Error("lairbug CameraMoveToCustomLairPlotCommand flow (id={0}) destroyed!", id);
			}
			disableCameraSignal.Dispatch(1);
			disableCameraSignal.Dispatch(2);
			PositionTweenProperty position = new PositionTweenProperty(cameraPosition);
			RotationTweenProperty rotation = new RotationTweenProperty(GameConstants.LairResourcePlotCustomUIOffsets.rotation);
			GoTweenFlow goTweenFlow = CreateFlow(position, rotation, 30f, 1f);
			pickControllerModel.PanningCameraBlocked = true;
			goTweenFlow.play();
			villainLairModel.cameraFlow = goTweenFlow;
			mainCamera.nearClipPlane = 0.3f;
			sfxSignal.Dispatch("Play_low_woosh_01");
		}

		private GoTweenFlow CreateFlow(AbstractTweenProperty position, RotationTweenProperty rotation, float fieldOfView, float duration)
		{
			GoTweenConfig config = new GoTweenConfig().addTweenProperty(position).addTweenProperty(rotation).setEaseType(GoEaseType.SineOut)
				.onComplete(delegate
				{
					pickControllerModel.PanningCameraBlocked = false;
				});
			GoTweenConfig config2 = new GoTweenConfig().floatProp("fieldOfView", fieldOfView).setEaseType(GoEaseType.SineOut);
			GoTween tween = new GoTween(mainCamera.transform, duration, config);
			GoTween tween2 = new GoTween(mainCamera, duration, config2);
			return new GoTweenFlow().insert(0f, tween).insert(0f, tween2);
		}
	}
}
