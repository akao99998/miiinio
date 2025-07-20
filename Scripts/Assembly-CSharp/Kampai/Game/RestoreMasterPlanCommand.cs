using System.Collections.Generic;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class RestoreMasterPlanCommand : Command
	{
		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public MasterPlanCooldownCompleteSignal cooldownCompleteSignal { get; set; }

		public override void Execute()
		{
			List<MasterPlan> instancesByType = playerService.GetInstancesByType<MasterPlan>();
			for (int i = 0; i < instancesByType.Count; i++)
			{
				MasterPlan masterPlan = instancesByType[i];
				int cooldownUTCStartTime = masterPlan.cooldownUTCStartTime;
				if (masterPlan.cooldownUTCStartTime > 0)
				{
					int num = timeService.CurrentTime() - masterPlan.cooldownUTCStartTime;
					if (num >= masterPlan.Definition.CooldownDuration)
					{
						cooldownCompleteSignal.Dispatch(masterPlan.ID);
					}
					else
					{
						timeEventService.AddEvent(masterPlan.ID, cooldownUTCStartTime, masterPlan.Definition.CooldownDuration, cooldownCompleteSignal);
					}
				}
			}
		}
	}
}
