using Kampai.Common;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.Game.View
{
	public class VillainLairResourcePlotObjectMediator : EventMediator
	{
		[Inject]
		public VillainLairResourcePlotObjectView view { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IRandomService randomService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public RouteMinionToLairResourcePlotSignal routeMinionToPlotSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		[Inject]
		public MinionStateChangeSignal minionStateChangeSignal { get; set; }

		[Inject]
		public RemoveMinionFromLairResourcePlotSignal removeMinionFromLairResourcePlotSignal { get; set; }

		[Inject]
		public TeleportMinionToLeisureSignal teleportMinionToLeisureSignal { get; set; }

		[Inject]
		public PathFinder pathfinder { get; set; }

		[Inject(GameElement.MINION_MANAGER)]
		public GameObject minionManager { get; set; }

		public override void OnRegister()
		{
			routeMinionToPlotSignal.AddListener(RouteMinionToResourcePlot);
			removeMinionFromLairResourcePlotSignal.AddListener(RemoveFromBuilding);
			teleportMinionToLeisureSignal.AddListener(TeleportMinionToResourcePlot);
			view.addToBuildingSignal.AddListener(AddCharacterToBuildingActions);
			view.gagSignal.AddListener(TriggerGagAnimation);
			view.InitializeControllers(randomService, minionStateChangeSignal);
		}

		public override void OnRemove()
		{
			routeMinionToPlotSignal.RemoveListener(RouteMinionToResourcePlot);
			removeMinionFromLairResourcePlotSignal.RemoveListener(RemoveFromBuilding);
			teleportMinionToLeisureSignal.RemoveListener(TeleportMinionToResourcePlot);
			view.addToBuildingSignal.RemoveListener(AddCharacterToBuildingActions);
			view.gagSignal.RemoveListener(TriggerGagAnimation);
		}

		private void RouteMinionToResourcePlot(MinionObject minionObject, int buildingID)
		{
			if (view.resourcePlot.ID == buildingID)
			{
				InitializeGagAnimation();
				playSFXSignal.Dispatch("Play_whistle_call_01");
				view.PathMinionToPlot(minionObject, view.addToBuildingSignal);
			}
		}

		private void TeleportMinionToResourcePlot(Minion minion)
		{
			if (minion.BuildingID == view.resourcePlot.ID)
			{
				view.resourcePlot.AddMinion(minion.ID, view.resourcePlot.UTCLastTaskingTimeStarted);
				MinionManagerView component = minionManager.GetComponent<MinionManagerView>();
				MinionObject mo = component.Get(minion.ID);
				InitializeGagAnimation();
				view.AddCharacterToBuildingActions(mo, 0);
			}
		}

		private void AddCharacterToBuildingActions(CharacterObject mo, int routeIndex)
		{
			InitializeGagAnimation();
			view.AddCharacterToBuildingActions(mo, routeIndex);
		}

		private void RemoveFromBuilding(int buildingInstanceID)
		{
			if (view.resourcePlot.ID == buildingInstanceID)
			{
				CleanBuildingState();
				Vector3 newPos = pathfinder.RandomPosition(true);
				view.FreeAllMinions(newPos);
			}
		}

		private void CleanBuildingState()
		{
			int minionIDInBuilding = view.resourcePlot.MinionIDInBuilding;
			view.resourcePlot.ClearMinionInBuilding();
			Minion byInstanceId = playerService.GetByInstanceId<Minion>(minionIDInBuilding);
			if (byInstanceId != null)
			{
				byInstanceId.BuildingID = -1;
				minionStateChangeSignal.Dispatch(minionIDInBuilding, MinionState.Idle);
			}
		}

		private void InitializeGagAnimation()
		{
			if (view.isGagable && view.resourcePlot.MinionIsTaskedToBuilding())
			{
				int eventTime = randomService.NextInt(view.resourcePlot.Definition.randomGagMin, view.resourcePlot.Definition.randomGagMax);
				timeEventService.AddEvent(view.resourcePlot.MinionIDInBuilding, timeService.CurrentTime(), eventTime, view.gagSignal);
			}
		}

		private void TriggerGagAnimation(int minionId)
		{
			if (minionId == view.resourcePlot.MinionIDInBuilding)
			{
				InitializeGagAnimation();
				view.TriggerGagAnimation();
			}
		}
	}
}
