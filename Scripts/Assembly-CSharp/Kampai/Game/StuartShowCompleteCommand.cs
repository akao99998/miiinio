using System.Collections.Generic;
using Kampai.Game.View;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class StuartShowCompleteCommand : Command
	{
		[Inject(GameElement.MINION_MANAGER)]
		public GameObject minionManager { get; set; }

		[Inject]
		public TemporaryMinionsService tempMinionService { get; set; }

		[Inject]
		public MoveMinionFinishedSignal moveMinionFinishedSignal { get; set; }

		[Inject]
		public PathFinder pathFinder { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public IZoomCameraModel zoomCameraModel { get; set; }

		[Inject]
		public ShowHUDSignal showHUDSignal { get; set; }

		[Inject]
		public ShowStoreSignal showStoreSignal { get; set; }

		[Inject(UIElement.CONTEXT)]
		public ICrossContextCapable uiContext { get; set; }

		[Inject]
		public ShowAllWayFindersSignal showAllWayFindersSignal { get; set; }

		public override void Execute()
		{
			uiContext.injectionBinder.GetInstance<CheckIfShouldStartPartySignal>().Dispatch();
			showHUDSignal.Dispatch(true);
			showStoreSignal.Dispatch(true);
			showAllWayFindersSignal.Dispatch();
			IDictionary<int, MinionObject> temporaryMinions = tempMinionService.getTemporaryMinions();
			if (temporaryMinions.Count <= 0)
			{
				return;
			}
			moveMinionFinishedSignal.AddListener(TemporaryMinionFinishedMoving);
			foreach (MinionObject value in temporaryMinions.Values)
			{
				Vector3 position = value.transform.position;
				Vector3 goalPos = new Vector3(position.x + 4.8f, position.y, position.z);
				bool muteStatus = true;
				IList<Vector3> path = pathFinder.FindPath(position, goalPos, 4, true);
				MinionManagerView component = minionManager.GetComponent<MinionManagerView>();
				component.StartPathing(value, path, 4.5f, muteStatus, moveMinionFinishedSignal, 0f);
			}
		}

		private void TemporaryMinionFinishedMoving(int id)
		{
			IDictionary<int, MinionObject> temporaryMinions = tempMinionService.getTemporaryMinions();
			foreach (MinionObject value in temporaryMinions.Values)
			{
				if (value.ID > -100)
				{
					Object.Destroy(value.gameObject);
					tempMinionService.removeTemporaryMinion(value.ID);
				}
			}
			gameContext.injectionBinder.GetInstance<BuildingZoomSignal>().Dispatch(new BuildingZoomSettings(ZoomType.OUT, zoomCameraModel.LastZoomBuildingType));
			moveMinionFinishedSignal.RemoveListener(TemporaryMinionFinishedMoving);
		}
	}
}
