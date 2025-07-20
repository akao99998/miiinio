using Elevation.Logging;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class SendMinionToLairResourcePlotCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("SendMinionToLairResourcePlotCommand") as IKampaiLogger;

		[Inject]
		public int buildingId { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(GameElement.MINION_MANAGER)]
		public GameObject minionManager { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public MinionStateChangeSignal minionStateChangeSignal { get; set; }

		[Inject]
		public BuildingChangeStateSignal buildingStateChangeSignal { get; set; }

		[Inject]
		public RouteMinionToLairResourcePlotSignal routeMinionToPlotSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playMinionNoAnimAudioSignal { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public AwardLairBonusDropsThenSetHarvestReadySignal awardDropsThenHarvestReadySignal { get; set; }

		public override void Execute()
		{
			MinionManagerView component = minionManager.GetComponent<MinionManagerView>();
			BuildingManagerView component2 = buildingManager.GetComponent<BuildingManagerView>();
			VillainLairResourcePlotObjectView villainLairResourcePlotObjectView = component2.GetBuildingObject(buildingId) as VillainLairResourcePlotObjectView;
			VillainLairResourcePlot resourcePlot = villainLairResourcePlotObjectView.resourcePlot;
			if (resourcePlot != null && !(villainLairResourcePlotObjectView == null))
			{
				Minion untaskedMinionWithHighestLevel = playerService.GetUntaskedMinionWithHighestLevel();
				MinionObject minionObject = component.Get(untaskedMinionWithHighestLevel.ID);
				playMinionNoAnimAudioSignal.Dispatch("Play_minion_confirm_pathToBldg_01");
				HandlePathing(minionObject, resourcePlot);
				buildingStateChangeSignal.Dispatch(resourcePlot.ID, BuildingState.Working);
			}
		}

		private void HandlePathing(MinionObject minionObject, VillainLairResourcePlot resourcePlot)
		{
			Minion byInstanceId = playerService.GetByInstanceId<Minion>(minionObject.ID);
			if (byInstanceId == null)
			{
				logger.Fatal(FatalCode.CMD_NO_SUCH_MINION, "{0}", minionObject.ID);
			}
			byInstanceId.BuildingID = buildingId;
			minionStateChangeSignal.Dispatch(byInstanceId.ID, MinionState.Tasking);
			minionObject.transform.position = (Vector3)resourcePlot.parentLair.Definition.MinionArrivalOffset + (Vector3)resourcePlot.parentLair.Definition.Location;
			routeMinionToPlotSignal.Dispatch(minionObject, resourcePlot.ID);
			resourcePlot.AddMinion(minionObject.ID, timeService.CurrentTime());
			timeEventService.AddEvent(resourcePlot.ID, resourcePlot.UTCLastTaskingTimeStarted, resourcePlot.parentLair.Definition.SecondsToHarvest, awardDropsThenHarvestReadySignal, TimeEventType.ProductionBuff);
		}
	}
}
