using System;
using Elevation.Logging;
using Kampai.Game;
using Kampai.Util;
using strange.extensions.context.api;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class PartyCameraControlPanelMediator : Mediator
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("PartyCameraControlPanelMediator") as IKampaiLogger;

		private CameraControlSettings cameraSettings;

		[Inject]
		public PartyCameraControlPanelView view { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			view.arrowTop.ClickedSignal.AddListener(TopButtonClicked);
			view.arrowLeft.ClickedSignal.AddListener(LeftButtonClicked);
			view.arrowRight.ClickedSignal.AddListener(RightButtonClicked);
			view.arrowBottom.ClickedSignal.AddListener(BottomButtonClicked);
			cameraSettings = definitionService.Get<MinionPartyDefinition>().cameraControlSettings;
		}

		public override void OnRemove()
		{
			base.OnRemove();
			view.arrowTop.ClickedSignal.RemoveListener(TopButtonClicked);
			view.arrowLeft.ClickedSignal.RemoveListener(LeftButtonClicked);
			view.arrowRight.ClickedSignal.RemoveListener(RightButtonClicked);
			view.arrowBottom.ClickedSignal.RemoveListener(BottomButtonClicked);
		}

		private void TopButtonClicked()
		{
			MoveCamera(cameraSettings.customCameraPosTiki);
		}

		private void LeftButtonClicked()
		{
			MoveCamera(cameraSettings.customCameraPosStage);
		}

		private void RightButtonClicked()
		{
			MoveCamera(cameraSettings.customCameraPosTownHall);
		}

		private void BottomButtonClicked()
		{
			MoveCamera(cameraSettings.customCameraPosPartyDefault);
		}

		private void MoveCamera(int customCameraPositionID)
		{
			if (customCameraPositionID != 0)
			{
				gameContext.injectionBinder.GetInstance<CameraMoveToCustomPositionSignal>().Dispatch(customCameraPositionID, new Boxed<Action>(null));
				return;
			}
			logger.Warning("Invalid camera postion ID {0}", customCameraPositionID);
		}
	}
}
