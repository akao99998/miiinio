using System.Collections;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game.View
{
	public class RestoreResourcePlotBuildingCommand : Command
	{
		[Inject]
		public VillainLairResourcePlot resourcePlot { get; set; }

		[Inject]
		public HarvestReadySignal harvestSignal { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public AwardLairBonusDropsThenSetHarvestReadySignal awardDropThenHarvestReadySignal { get; set; }

		public override void Execute()
		{
			routineRunner.StartCoroutine(RestoreBuilding());
		}

		private IEnumerator RestoreBuilding()
		{
			yield return new WaitForEndOfFrame();
			int plotID = resourcePlot.ID;
			BuildingState oldState = resourcePlot.State;
			if (resourcePlot.harvestCount > 0 || resourcePlot.BonusMinionItems.Count > 0)
			{
				harvestSignal.Dispatch(plotID);
			}
			if (oldState == BuildingState.Working)
			{
				int secondsToHarvest = resourcePlot.parentLair.Definition.SecondsToHarvest;
				timeEventService.AddEvent(plotID, resourcePlot.UTCLastTaskingTimeStarted, secondsToHarvest, awardDropThenHarvestReadySignal, TimeEventType.ProductionBuff);
			}
		}
	}
}
