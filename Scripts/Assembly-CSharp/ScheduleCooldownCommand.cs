using Kampai.Game;
using Kampai.Util;
using strange.extensions.command.impl;

public class ScheduleCooldownCommand : Command
{
	[Inject]
	public Tuple<int, bool> commandParams { get; set; }

	[Inject]
	public bool triggerStateChange { get; set; }

	[Inject]
	public BuildingCooldownCompleteSignal cooldownCompleteSignal { get; set; }

	[Inject]
	public BuildingCooldownUpdateViewSignal cooldownUpdateViewSignal { get; set; }

	[Inject]
	public BuildingChangeStateSignal buildingChangeStateSignal { get; set; }

	[Inject]
	public MignetteNotificationSignal mignetteNotificationSignal { get; set; }

	[Inject]
	public ITimeEventService timeEventService { get; set; }

	[Inject]
	public IPlayerService playerService { get; set; }

	[Inject]
	public IDevicePrefsService devicePrefsService { get; set; }

	public override void Execute()
	{
		int item = commandParams.Item1;
		bool item2 = commandParams.Item2;
		IBuildingWithCooldown byInstanceId = playerService.GetByInstanceId<IBuildingWithCooldown>(item);
		if (byInstanceId == null || byInstanceId.GetCooldown() <= 0)
		{
			cooldownCompleteSignal.Dispatch(item);
			return;
		}
		if (triggerStateChange)
		{
			buildingChangeStateSignal.Dispatch(item, BuildingState.Cooldown);
		}
		timeEventService.AddEvent(item, byInstanceId.StateStartTime, byInstanceId.GetCooldown(), cooldownCompleteSignal);
		if (devicePrefsService.GetDevicePrefs().MinionsParadiseNotif)
		{
			mignetteNotificationSignal.Dispatch(false, byInstanceId.ID);
		}
		if (item2)
		{
			int num = byInstanceId.GetCooldown() / 10;
			for (int i = 0; i < 10; i++)
			{
				timeEventService.AddEvent(item, byInstanceId.StateStartTime, i * num, cooldownUpdateViewSignal);
			}
		}
	}
}
