using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class RestoreStuartCommand : Command
	{
		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public SocialEventAvailableSignal socialEventAvailableSignal { get; set; }

		[Inject]
		public FrolicSignal frolicSignal { get; set; }

		[Inject]
		public AddMinionToTikiBarSignal tikiSignal { get; set; }

		public override void Execute()
		{
			StuartCharacter firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<StuartCharacter>(70001);
			if (firstInstanceByDefinitionId == null)
			{
				return;
			}
			Prestige firstInstanceByDefinitionId2 = playerService.GetFirstInstanceByDefinitionId<Prestige>(40003);
			if (firstInstanceByDefinitionId2.state != PrestigeState.Questing)
			{
				frolicSignal.Dispatch(firstInstanceByDefinitionId.ID);
			}
			else
			{
				TikiBarBuilding byInstanceId = playerService.GetByInstanceId<TikiBarBuilding>(313);
				if (byInstanceId != null)
				{
					int minionSlotIndex = byInstanceId.GetMinionSlotIndex(firstInstanceByDefinitionId2.Definition.ID);
					if (minionSlotIndex != -1)
					{
						tikiSignal.Dispatch(byInstanceId, firstInstanceByDefinitionId, firstInstanceByDefinitionId2, minionSlotIndex);
					}
				}
			}
			if (firstInstanceByDefinitionId2.CurrentPrestigeLevel >= 1)
			{
				socialEventAvailableSignal.Dispatch();
			}
		}
	}
}
