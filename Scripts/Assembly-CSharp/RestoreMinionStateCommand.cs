using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Util;
using strange.extensions.command.impl;

public class RestoreMinionStateCommand : Command
{
	public IKampaiLogger logger = LogManager.GetClassLogger("RestoreMinionStateCommand") as IKampaiLogger;

	[Inject]
	public int minionID { get; set; }

	[Inject]
	public IPlayerService playerService { get; set; }

	[Inject]
	public IDefinitionService definitionService { get; set; }

	[Inject]
	public ITimeEventService timeEventService { get; set; }

	[Inject]
	public ITimeService timeService { get; set; }

	[Inject]
	public MinionTaskCompleteSignal taskCompleteSignal { get; set; }

	[Inject]
	public StartTeleportTaskSignal teleportTaskSignal { get; set; }

	[Inject]
	public EnableMinionRendererSignal enableRendererSignal { get; set; }

	[Inject]
	public RestoreMinionAtTikiBarSignal restoreMinionAtTikiBarSignal { get; set; }

	[Inject]
	public TeleportMinionToLeisureSignal teleportMinionToLeisureSignal { get; set; }

	public override void Execute()
	{
		Minion byInstanceId = playerService.GetByInstanceId<Minion>(minionID);
		if (byInstanceId == null)
		{
			logger.Fatal(FatalCode.CMD_RESTORE_MINION);
			return;
		}
		if (byInstanceId.IsInMinionParty)
		{
			if (byInstanceId.UTCTaskStartTime > 0 && byInstanceId.TaskDuration > 0)
			{
				byInstanceId.IsInMinionParty = false;
				byInstanceId.State = MinionState.Tasking;
			}
			else if (byInstanceId.State == MinionState.Idle && byInstanceId.BuildingID != -1)
			{
				Building byInstanceId2 = playerService.GetByInstanceId<Building>(byInstanceId.BuildingID);
				if (byInstanceId2 is LeisureBuilding)
				{
					byInstanceId.IsInMinionParty = false;
					byInstanceId.State = MinionState.Leisure;
				}
			}
		}
		if (byInstanceId.State == MinionState.Tasking)
		{
			if (!RestoreResourcePlotTaskingMinionState(byInstanceId))
			{
				RestoreTaskingMinionState(byInstanceId);
			}
		}
		else if (byInstanceId.State == MinionState.Selected || byInstanceId.State == MinionState.Selectable || byInstanceId.State == MinionState.WaitingOnMagnetFinger || byInstanceId.State == MinionState.PlayingMignette || byInstanceId.State == MinionState.Unselectable)
		{
			byInstanceId.State = MinionState.Idle;
		}
		else if (byInstanceId.State == MinionState.Questing)
		{
			if (!byInstanceId.HasPrestige)
			{
				byInstanceId.State = MinionState.Idle;
			}
			else
			{
				restoreMinionAtTikiBarSignal.Dispatch(byInstanceId);
			}
		}
		else if (byInstanceId.State == MinionState.Leisure)
		{
			RestoreLeisureMinionState(byInstanceId);
		}
		if (byInstanceId.State == MinionState.Idle || byInstanceId.State.Equals(MinionState.Uninitialized))
		{
			enableRendererSignal.Dispatch(byInstanceId.ID, true);
		}
	}

	private bool RestoreResourcePlotTaskingMinionState(Minion minion)
	{
		VillainLairResourcePlot byInstanceId = playerService.GetByInstanceId<VillainLairResourcePlot>(minion.BuildingID);
		if (byInstanceId == null)
		{
			return false;
		}
		if (byInstanceId.State == BuildingState.Working && byInstanceId.UTCLastTaskingTimeStarted > 0)
		{
			teleportMinionToLeisureSignal.Dispatch(minion);
		}
		else
		{
			minion.State = MinionState.Idle;
		}
		return true;
	}

	private void RestoreTaskingMinionState(Minion minion)
	{
		TaskableBuilding byInstanceId = playerService.GetByInstanceId<TaskableBuilding>(minion.BuildingID);
		if (minion.TaskDuration == 0 || minion.UTCTaskStartTime + minion.TaskDuration - minion.PartyTimeReduction <= timeService.CurrentTime())
		{
			if (byInstanceId != null)
			{
				if (!(byInstanceId is ResourceBuilding))
				{
					minion.AlreadyRushed = true;
					teleportTaskSignal.Dispatch(minion, byInstanceId);
					byInstanceId.AddMinion(minionID, minion.UTCTaskStartTime);
				}
			}
			else
			{
				minion.State = MinionState.Idle;
			}
			taskCompleteSignal.Dispatch(minionID);
		}
		else
		{
			teleportTaskSignal.Dispatch(minion, byInstanceId);
			int eventTime = BuildingUtil.GetHarvestTimeForTaskableBuilding(byInstanceId, definitionService) - minion.PartyTimeReduction;
			timeEventService.AddEvent(minionID, minion.UTCTaskStartTime, eventTime, taskCompleteSignal, TimeEventType.ProductionBuff);
			byInstanceId.AddMinion(minionID, minion.UTCTaskStartTime);
		}
	}

	private void RestoreLeisureMinionState(Minion minion)
	{
		LeisureBuilding byInstanceId = playerService.GetByInstanceId<LeisureBuilding>(minion.BuildingID);
		if (byInstanceId.State == BuildingState.Working && byInstanceId.UTCLastTaskingTimeStarted > 0)
		{
			byInstanceId.AddMinion(minion.ID, byInstanceId.UTCLastTaskingTimeStarted);
			teleportMinionToLeisureSignal.Dispatch(minion);
		}
		else
		{
			minion.State = MinionState.Idle;
		}
	}
}
