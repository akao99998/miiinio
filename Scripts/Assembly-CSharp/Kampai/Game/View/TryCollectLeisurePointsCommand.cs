using Elevation.Logging;
using Kampai.Common;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game.View
{
	public class TryCollectLeisurePointsCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("TryCollectLeisurePointsCommand") as IKampaiLogger;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public BuildingChangeStateSignal buildingChangeStateSignal { get; set; }

		[Inject]
		public UpdateResourceIconCountSignal updateResourceIconCountSignal { get; set; }

		[Inject]
		public SpawnDooberSignal spawnDooberSignal { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public IMasterPlanService masterPlanService { get; set; }

		[Inject]
		public HideAllWayFindersSignal hideAllWayFindersSignal { get; set; }

		[Inject]
		public UpdateAdHUDSignal updateAdHUDSignal { get; set; }

		[Inject]
		public LeisureBuilding building { get; set; }

		public override void Execute()
		{
			if (building == null || building.UTCLastTaskingTimeStarted == 0)
			{
				logger.Warning("Can not collect points for Leisure building");
				return;
			}
			int partyPointsReward = building.Definition.PartyPointsReward;
			playerService.AddXP(partyPointsReward);
			questService.UpdateAllQuestsWithQuestStepType(QuestStepType.HarvestAnyLeisure);
			masterPlanService.ProcessActiveComponent(MasterPlanComponentTaskType.EarnPartyPoints, (uint)partyPointsReward, building.Definition.ID);
			masterPlanService.ProcessActiveComponent(MasterPlanComponentTaskType.EarnLeisurePartyPoints, (uint)partyPointsReward, building.Definition.ID);
			building.UTCLastTaskingTimeStarted = 0;
			telemetryService.Send_Telemetry_EVT_PARTY_POINTS_EARNED(partyPointsReward, building.Definition.LocalizedKey);
			buildingChangeStateSignal.Dispatch(building.ID, BuildingState.Idle);
			SpawnDoober();
			if (playerService.GetMinionPartyInstance().IsPartyReady)
			{
				hideAllWayFindersSignal.Dispatch();
			}
			updateAdHUDSignal.Dispatch();
		}

		private void SpawnDoober()
		{
			updateResourceIconCountSignal.Dispatch(new Tuple<int, int>(building.ID, 2), 0);
			Vector3 type = new Vector3(building.Location.x, 0f, building.Location.y);
			spawnDooberSignal.Dispatch(type, DestinationType.XP, 2, true);
		}
	}
}
