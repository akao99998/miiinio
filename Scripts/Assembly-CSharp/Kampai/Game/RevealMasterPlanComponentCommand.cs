using System.Collections.Generic;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class RevealMasterPlanComponentCommand : Command
	{
		[Inject]
		public int buildingDefinitionId { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		public override void Execute()
		{
			IList<MasterPlanComponent> instancesByType = playerService.GetInstancesByType<MasterPlanComponent>();
			foreach (MasterPlanComponent item in instancesByType)
			{
				if (item.buildingDefID == buildingDefinitionId)
				{
					item.State = MasterPlanComponentState.Built;
				}
			}
		}
	}
}
