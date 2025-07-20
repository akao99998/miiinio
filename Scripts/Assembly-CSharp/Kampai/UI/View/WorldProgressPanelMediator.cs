using Elevation.Logging;
using Kampai.Util;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class WorldProgressPanelMediator : Mediator
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("WorldProgressPanelMediator") as IKampaiLogger;

		[Inject]
		public WorldProgressPanelView view { get; set; }

		[Inject]
		public UIModel model { get; set; }

		[Inject]
		public DisplayWorldProgressSignal displayWorldProgressSignal { get; set; }

		[Inject]
		public RemoveWorldProgressSignal removeWorldProgressSignal { get; set; }

		[Inject]
		public ShowMoveBuildingMenuSignal showMoveBuildingMenuSignal { get; set; }

		[Inject]
		public HideAllWayFindersSignal hideAllWayFindersSignal { get; set; }

		[Inject]
		public ShowAllWayFindersSignal showAllWayFindersSignal { get; set; }

		[Inject]
		public CloseAllOtherMenuSignal closeAllMenuSignal { get; set; }

		[Inject]
		public CreateWayFinderSignal createWayFinderSignal { get; set; }

		[Inject]
		public RemoveWayFinderSignal removeWayFinderSignal { get; set; }

		public override void OnRegister()
		{
			view.Init(logger);
			displayWorldProgressSignal.AddListener(DisplayWorldProgress);
			removeWorldProgressSignal.AddListener(RemoveWorldProgress);
			showMoveBuildingMenuSignal.AddListener(ShowMoveBuildingMenu);
		}

		public override void OnRemove()
		{
			view.Cleanup();
			displayWorldProgressSignal.RemoveListener(DisplayWorldProgress);
			removeWorldProgressSignal.RemoveListener(RemoveWorldProgress);
			showMoveBuildingMenuSignal.RemoveListener(ShowMoveBuildingMenu);
		}

		private void ShowMoveBuildingMenu(bool show, MoveBuildingSetting setting)
		{
			if (show)
			{
				createWayFinderSignal.Dispatch(new WayFinderSettings(setting.TrackedId));
				GameObject type = view.CreateOrUpdateMoveBuildingMenu(setting);
				closeAllMenuSignal.Dispatch(type);
				hideAllWayFindersSignal.Dispatch();
				model.BuildingDragMode = true;
			}
			else
			{
				removeWayFinderSignal.Dispatch(setting.TrackedId);
				showAllWayFindersSignal.Dispatch();
				view.RemoveMoveBuildingMenu();
				model.BuildingDragMode = false;
			}
		}

		private void DisplayWorldProgress(ProgressBarSettings settings)
		{
			view.CreateProgressBar(settings);
		}

		private void RemoveWorldProgress(int trackedId)
		{
			view.RemoveProgressBar(trackedId);
		}
	}
}
