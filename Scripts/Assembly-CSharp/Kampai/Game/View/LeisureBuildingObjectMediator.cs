using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	public class LeisureBuildingObjectMediator : EventMediator
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("LeisureBuildingObjectMediator") as IKampaiLogger;

		private Signal<CharacterObject, int> addToBuildingSignal;

		[Inject]
		public LeisureBuildingObjectView view { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(GameElement.MINION_MANAGER)]
		public GameObject minionManager { get; set; }

		[Inject]
		public RouteMinionToLeisureSignal routeMinionToLeisureSignal { get; set; }

		[Inject]
		public TeleportMinionToLeisureSignal teleportMinionToLeisureSignal { get; set; }

		[Inject]
		public MinionStateChangeSignal minionStateChangeSignal { get; set; }

		[Inject]
		public RemoveMinionFromLeisureSignal removeMinionFromLeisureSignal { get; set; }

		[Inject]
		public PrepareLeisureMinionsForPartySignal prepareLeisureMinionsForPartySignal { get; set; }

		[Inject]
		public RestoreLeisureMinionsFromPartySignal restoreLeisureMinionsFromPartySignal { get; set; }

		[Inject]
		public StartLeisurePartyPointsSignal startLeisurePartyPointsSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		[Inject]
		public ToggleMinionRendererSignal toggleMinionSignal { get; set; }

		[Inject]
		public PickControllerModel model { get; set; }

		[Inject]
		public RelocateCharacterSignal relocateCharacterSignal { get; set; }

		public override void OnRegister()
		{
			addToBuildingSignal = new Signal<CharacterObject, int>();
			routeMinionToLeisureSignal.AddListener(RouteMinionToLeisure);
			removeMinionFromLeisureSignal.AddListener(RemoveMinionFromLeisure);
			teleportMinionToLeisureSignal.AddListener(TeleportMinionToLeisure);
			addToBuildingSignal.AddListener(AddToBuilding);
			prepareLeisureMinionsForPartySignal.AddListener(ReleaseMinionsForParty);
			restoreLeisureMinionsFromPartySignal.AddListener(RestoreMinionsFromParty);
			SetupInjections();
		}

		public override void OnRemove()
		{
			routeMinionToLeisureSignal.RemoveListener(RouteMinionToLeisure);
			removeMinionFromLeisureSignal.RemoveListener(RemoveMinionFromLeisure);
			teleportMinionToLeisureSignal.RemoveListener(TeleportMinionToLeisure);
			addToBuildingSignal.RemoveListener(AddToBuilding);
			prepareLeisureMinionsForPartySignal.RemoveListener(ReleaseMinionsForParty);
			restoreLeisureMinionsFromPartySignal.RemoveListener(RestoreMinionsFromParty);
		}

		private void SetupInjections()
		{
			view.SetupInjections(minionStateChangeSignal, startLeisurePartyPointsSignal, relocateCharacterSignal);
		}

		private void RouteMinionToLeisure(MinionObject minionObject, RouteInstructions routeInfo, int routeIndex)
		{
			if (view.leisureBuilding.ID == routeInfo.TargetBuilding.ID)
			{
				playSFXSignal.Dispatch("Play_whistle_call_01");
				view.PathMinionToLeisureBuilding(minionObject, routeInfo.Path, routeInfo.Rotation, routeIndex, addToBuildingSignal);
			}
		}

		private void AddToBuilding(CharacterObject characterObject, int routeIndex)
		{
			view.AddCharacterToBuildingActions(characterObject, routeIndex);
			bool flag = false;
			if (model.SelectedBuilding.HasValue && view.leisureBuilding.ID == model.SelectedBuilding.Value)
			{
				flag = true;
			}
			if (view.IsGFXFaded() || flag)
			{
				view.FadeMinions(toggleMinionSignal, false);
			}
		}

		private void TeleportMinionToLeisure(Minion minion)
		{
			if (minion.BuildingID == view.leisureBuilding.ID)
			{
				MinionManagerView component = minionManager.GetComponent<MinionManagerView>();
				MinionObject mo = component.Get(minion.ID);
				int minionRouteIndex = view.leisureBuilding.GetMinionRouteIndex(minion.ID);
				if (minionRouteIndex == -1)
				{
					logger.Error("Minion {0} doesn't exist on this building {1}, but you are still trying to add him.", minion.ID, view.leisureBuilding.ID);
				}
				view.AddCharacterToBuildingActions(mo, minionRouteIndex);
			}
		}

		private void RemoveMinionFromLeisure(int buildingInstanceID)
		{
			if (view.leisureBuilding.ID == buildingInstanceID)
			{
				CleanBuildingState(true);
				view.FreeAllMinions();
			}
		}

		private void ReleaseMinionsForParty()
		{
			timeEventService.RemoveEvent(view.leisureBuilding.ID);
			CleanBuildingState(false);
			view.FreeAllMinions();
		}

		private void RestoreMinionsFromParty()
		{
			IList<int> minionList = view.leisureBuilding.MinionList;
			foreach (int item in minionList)
			{
				Minion byInstanceId = playerService.GetByInstanceId<Minion>(item);
				byInstanceId.IsInMinionParty = false;
				MinionManagerView component = minionManager.GetComponent<MinionManagerView>();
				MinionObject minionObject = component.Get(byInstanceId.ID);
				minionObject.ClearActionQueue();
				TeleportMinionToLeisure(byInstanceId);
				minionStateChangeSignal.Dispatch(item, MinionState.Leisure);
			}
		}

		private void CleanBuildingState(bool isTaskComplete)
		{
			foreach (int minion in view.leisureBuilding.MinionList)
			{
				if (!view.IsMinionInBuilding(minion))
				{
					MinionManagerView component = minionManager.GetComponent<MinionManagerView>();
					MinionObject minionObject = component.Get(minion);
					minionObject.ClearActionQueue();
				}
				Minion byInstanceId = playerService.GetByInstanceId<Minion>(minion);
				if (isTaskComplete)
				{
					byInstanceId.BuildingID = -1;
				}
				minionStateChangeSignal.Dispatch(minion, MinionState.Idle);
				toggleMinionSignal.Dispatch(minion, true);
			}
			if (isTaskComplete)
			{
				view.leisureBuilding.CleanMinionQueue();
			}
		}
	}
}
