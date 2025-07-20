using Kampai.Common;
using Kampai.Game.View;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class SocialOrderBoardCompleteCommand : Command
	{
		private StageBuildingObject sbo;

		[Inject]
		public PickControllerModel pickControllerModel { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public AddStuartToStageSignal addStuartToStageSignal { get; set; }

		[Inject]
		public StuartStartPerformingSignal startPerformingSignal { get; set; }

		[Inject]
		public ReleaseMinionFromTikiBarSignal releaseMinionFromTikiBarSignal { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject(UIElement.CONTEXT)]
		public ICrossContextCapable uiContext { get; set; }

		[Inject]
		public StuartShowCompleteSignal showCompleteSignal { get; set; }

		[Inject]
		public StageService stageService { get; set; }

		[Inject]
		public ITimedSocialEventService timedSocialEventService { get; set; }

		[Inject]
		public CloseConfirmationSignal closeConfirmationSignal { get; set; }

		[Inject]
		public ShowHUDSignal showHUDSignal { get; set; }

		[Inject]
		public ShowStoreSignal showStoreSignal { get; set; }

		[Inject]
		public StuartShowStartSignal stuartShowStartSignal { get; set; }

		[Inject]
		public IZoomCameraModel zoomCameraModel { get; set; }

		[Inject]
		public HideAllWayFindersSignal hideAllWayFindersSignal { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		public override void Execute()
		{
			gameContext.injectionBinder.GetInstance<BuildingZoomSignal>().Dispatch(new BuildingZoomSettings(ZoomType.IN, BuildingZoomType.STAGE, ZoomCompleted));
			Signal signal = new Signal();
			SignalCallback<Signal> signalCallback = new SignalCallback<Signal>(signal);
			signal.AddListener(HandleShowFinished);
			StuartCharacter firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<StuartCharacter>(70001);
			releaseMinionFromTikiBarSignal.Dispatch(firstInstanceByDefinitionId, true);
			addStuartToStageSignal.Dispatch(StuartStageAnimationType.IDLEOFFSTAGE);
			startPerformingSignal.Dispatch(signalCallback);
			BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
			BuildingObject buildingObject = component.GetBuildingObject(370);
			sbo = buildingObject as StageBuildingObject;
			if (sbo != null)
			{
				sbo.PerformanceStarts();
			}
			if (!signalCallback.WillDispatch)
			{
				HandleShowFinished();
			}
			else
			{
				TurnOffUI();
			}
			timedSocialEventService.setRewardCutscene(true);
		}

		private void HandleShowFinished()
		{
			if (zoomCameraModel.ZoomedIn)
			{
				gameContext.injectionBinder.GetInstance<BuildingZoomSignal>().Dispatch(new BuildingZoomSettings(ZoomType.OUT, zoomCameraModel.LastZoomBuildingType));
			}
			closeConfirmationSignal.Dispatch();
			timedSocialEventService.setRewardCutscene(false);
			uiContext.injectionBinder.GetInstance<CheckIfShouldStartPartySignal>().Dispatch();
			if (sbo != null)
			{
				sbo.UpdateStageState(BuildingState.Idle);
			}
			showCompleteSignal.Dispatch();
		}

		public void ZoomCompleted()
		{
			stuartShowStartSignal.Dispatch();
			stageService.ShowStageBackdrop();
			pickControllerModel.CurrentMode = PickControllerModel.Mode.StageView;
		}

		private void TurnOffUI()
		{
			showHUDSignal.Dispatch(false);
			showStoreSignal.Dispatch(false);
			hideAllWayFindersSignal.Dispatch();
		}
	}
}
