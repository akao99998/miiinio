using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.injector.api;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public abstract class AbstractWayFinderMediator : Mediator
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("AbstractWayFinderMediator") as IKampaiLogger;

		private OpenBuildingMenuSignal openBuildingMenuSignal;

		private BuildingManagerView buildingManagerView;

		private ButtonView goToButton;

		private int trackedId;

		public abstract IWayFinderView View { get; }

		[Inject]
		public IPositionService PositionService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable GameContext { get; set; }

		[Inject]
		public IPlayerService PlayerService { get; set; }

		[Inject]
		public CameraAutoMoveToInstanceSignal CameraAutoMoveToInstanceSignal { get; set; }

		[Inject]
		public ILocalizationService LocalizationService { get; set; }

		[Inject]
		public RemoveWayFinderSignal RemoveWayFinderSignal { get; set; }

		[Inject]
		public ITikiBarService TikiBarService { get; set; }

		[Inject]
		public IZoomCameraModel ZoomCameraModel { get; set; }

		[Inject]
		public UpdateWayFinderPrioritySignal UpdateWayFinderPrioritySignal { get; set; }

		[Inject]
		public IDefinitionService DefinitionService { get; set; }

		[Inject]
		public TryHarvestBuildingSignal tryHarvestBuildingSignal { get; set; }

		[Inject]
		public TryCollectLeisurePointsSignal tryCollectLeisurePoints { get; set; }

		[Inject]
		public PickControllerModel pickModel { get; set; }

		[Inject]
		public VillainLairModel lairModel { get; set; }

		[Inject]
		public ShowActivitySpinnerSignal showActivitySpinnerSignal { get; set; }

		public override void OnRegister()
		{
			ICrossContextInjectionBinder injectionBinder = GameContext.injectionBinder;
			CameraAutoMoveToInstanceSignal = injectionBinder.GetInstance<CameraAutoMoveToInstanceSignal>();
			openBuildingMenuSignal = injectionBinder.GetInstance<OpenBuildingMenuSignal>();
			buildingManagerView = injectionBinder.GetInstance<GameObject>(GameElement.BUILDING_MANAGER).GetComponent<BuildingManagerView>();
			WayFinderModal component = GetComponent<WayFinderModal>();
			goToButton = component.GoToButton;
			trackedId = component.Settings.TrackedId;
			AbstractWayFinderView abstractWayFinderView = View as AbstractWayFinderView;
			goToButton.ClickedSignal.AddListener(OnGoTo);
			abstractWayFinderView.UpdateWayFinderPrioritySignal.AddListener(UpdateWayFinderPriority);
			abstractWayFinderView.RemoveWayFinderSignal.AddListener(RemoveWayFinder);
			abstractWayFinderView.SimulateClickSignal.AddListener(OnGoTo);
			abstractWayFinderView.Init(PositionService, GameContext, logger, ZoomCameraModel, TikiBarService, PlayerService, LocalizationService, DefinitionService);
		}

		public override void OnRemove()
		{
			AbstractWayFinderView abstractWayFinderView = View as AbstractWayFinderView;
			abstractWayFinderView.Clear();
			goToButton.ClickedSignal.RemoveListener(OnGoTo);
			abstractWayFinderView.UpdateWayFinderPrioritySignal.RemoveListener(UpdateWayFinderPriority);
			abstractWayFinderView.RemoveWayFinderSignal.RemoveListener(RemoveWayFinder);
			abstractWayFinderView.SimulateClickSignal.RemoveListener(OnGoTo);
		}

		private void RemoveWayFinder()
		{
			RemoveWayFinderSignal.Dispatch(trackedId);
		}

		private void UpdateWayFinderPriority()
		{
			UpdateWayFinderPrioritySignal.Dispatch();
		}

		private void OnGoTo()
		{
			if (pickModel.activitySpinnerExists)
			{
				showActivitySpinnerSignal.Dispatch(false, Vector3.zero);
				pickModel.activitySpinnerExists = false;
			}
			GoToClicked();
		}

		protected virtual void GoToClicked()
		{
			if (pickModel.PanningCameraBlocked || lairModel.goingToLair)
			{
				return;
			}
			Building byInstanceId = PlayerService.GetByInstanceId<Building>(trackedId);
			if (byInstanceId != null && (View.IsTargetObjectVisible() || byInstanceId.Definition.ID == 3022))
			{
				ScaffoldingBuildingObject scaffoldingBuildingObject = buildingManagerView.GetScaffoldingBuildingObject(trackedId);
				if (scaffoldingBuildingObject != null)
				{
					GameContext.injectionBinder.GetInstance<RevealBuildingSignal>().Dispatch(byInstanceId);
					return;
				}
				BuildingObject buildingObject = buildingManagerView.GetBuildingObject(trackedId);
				if (!(buildingObject != null))
				{
					return;
				}
				BuildingState state = byInstanceId.State;
				if (state == BuildingState.Harvestable || state == BuildingState.HarvestableAndWorking)
				{
					tryHarvestBuildingSignal.Dispatch(buildingObject, delegate
					{
					}, false);
					LeisureBuilding leisureBuilding = byInstanceId as LeisureBuilding;
					if (leisureBuilding != null)
					{
						tryCollectLeisurePoints.Dispatch(leisureBuilding);
					}
				}
				else
				{
					openBuildingMenuSignal.Dispatch(buildingObject, byInstanceId);
				}
			}
			else
			{
				PanToInstance();
			}
		}

		protected virtual void PanToInstance()
		{
			Instance byInstanceId = PlayerService.GetByInstanceId<Instance>(trackedId);
			if (byInstanceId != null && !ZoomCameraModel.ZoomedIn)
			{
				CameraAutoMoveToInstanceSignal.Dispatch(new PanInstructions(byInstanceId), new Boxed<ScreenPosition>(new ScreenPosition()));
			}
		}

		protected int GetTrackedId()
		{
			return trackedId;
		}
	}
}
