using System.Collections.Generic;
using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class RemoveMinionFromTikiBarCommand : Command
	{
		[Inject]
		public Prestige prestige { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		[Inject]
		public MinionStateChangeSignal minionStateChangeSignal { get; set; }

		[Inject]
		public ReleaseMinionFromTikiBarSignal releaseMinionSignal { get; set; }

		[Inject]
		public MinionPrestigeCompleteSignal minionPrestigeCompleteSignal { get; set; }

		[Inject]
		public GUIServiceQueueEmptySignal guiServiceQueueEmptySignal { get; set; }

		public override void Execute()
		{
			IList<Instance> instancesByDefinition = playerService.GetInstancesByDefinition<TikiBarBuildingDefinition>();
			if (instancesByDefinition == null || instancesByDefinition.Count == 0)
			{
				return;
			}
			TikiBarBuilding tikiBar = instancesByDefinition[0] as TikiBarBuilding;
			int minionSlotIndex = tikiBar.GetMinionSlotIndex(prestige.Definition.ID);
			if (minionSlotIndex >= 0 && minionSlotIndex < tikiBar.minionQueue.Count)
			{
				tikiBar.minionQueue[minionSlotIndex] = -1;
			}
			Character byInstanceId = playerService.GetByInstanceId<Character>(prestige.trackedInstanceId);
			releaseMinionSignal.Dispatch(byInstanceId, false);
			if (byInstanceId is Minion)
			{
				minionStateChangeSignal.Dispatch(byInstanceId.ID, MinionState.Idle);
			}
			if (prestige.state != PrestigeState.TaskableWhileQuesting)
			{
				prestigeService.ChangeToPrestigeState(prestige, PrestigeState.Taskable);
			}
			if (tikiBar.minionQueue.Count > 3)
			{
				int waitingNewPrestigeDefId = tikiBar.minionQueue[3];
				guiServiceQueueEmptySignal.AddOnce(delegate
				{
					Prestige type = prestigeService.GetPrestige(waitingNewPrestigeDefId);
					tikiBar.minionQueue.Remove(waitingNewPrestigeDefId);
					minionPrestigeCompleteSignal.Dispatch(type);
				});
			}
		}
	}
}
