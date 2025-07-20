using System;
using System.Collections.Generic;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class PopulateBuildingCommand : Command
	{
		[Inject]
		public PlaceBuildingSignal placeBuildingSignal { get; set; }

		[Inject]
		public DebugUpdateGridSignal DebugUpdateGridSignal { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		public override void Execute()
		{
			IPlayerService instance = base.injectionBinder.GetInstance<IPlayerService>();
			foreach (Building item in instance.GetInstancesByType<Building>())
			{
				if (item.State != BuildingState.Inventory)
				{
					if (item.Location == null)
					{
						TryPatchBuildingLocation(item);
					}
					placeBuildingSignal.Dispatch(item.ID, item.Location);
				}
			}
			DebugUpdateGridSignal.Dispatch();
		}

		private void TryPatchBuildingLocation(Building building)
		{
			if (building == null)
			{
				return;
			}
			Definition definition = building.Definition;
			if (definition == null || building.Definition.Movable)
			{
				return;
			}
			string initialPlayer = definitionService.GetInitialPlayer();
			if (string.IsNullOrEmpty(initialPlayer))
			{
				return;
			}
			try
			{
				Player player = playerService.LoadPlayerData(initialPlayer);
				List<Instance> instancesByDefinitionID = player.GetInstancesByDefinitionID(definition.ID);
				if (instancesByDefinitionID.Count > 0 && instancesByDefinitionID[0] != null)
				{
					Locatable locatable = instancesByDefinitionID[0] as Locatable;
					if (locatable != null)
					{
						building.Location = locatable.Location;
					}
				}
			}
			catch (Exception)
			{
			}
		}
	}
}
