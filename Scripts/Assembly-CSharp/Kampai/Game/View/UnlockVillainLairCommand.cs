using System.Collections;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game.View
{
	public class UnlockVillainLairCommand : Command
	{
		[Inject]
		public VillainLairEntranceBuilding portal { get; set; }

		[Inject]
		public int villainLairDefinitionID { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public LoadVillainLairAssetsSignal loadVillainLairAssetsSignal { get; set; }

		public override void Execute()
		{
			if (!portal.IsUnlocked)
			{
				VillainLairDefinition villainLairDefinition = definitionService.Get<VillainLairDefinition>(villainLairDefinitionID);
				VillainLair villainLair = (VillainLair)villainLairDefinition.Build();
				playerService.Add(villainLair);
				portal.unlockVillainLair(villainLair.ID);
				CreateResourcePlots(villainLairDefinition, villainLair);
				routineRunner.StartCoroutine(LoadLairAssets());
			}
		}

		private IEnumerator LoadLairAssets()
		{
			yield return null;
			loadVillainLairAssetsSignal.Dispatch(portal.VillainLairInstanceID);
		}

		private void CreateResourcePlots(VillainLairDefinition lairDefinition, VillainLair lair)
		{
			for (int i = 0; i < lairDefinition.ResourcePlots.Count; i++)
			{
				ResourcePlotDefinition resourcePlotDefinition = lairDefinition.ResourcePlots[i];
				VillainLairResourcePlotDefinition villainLairResourcePlotDefinition = definitionService.Get<VillainLairResourcePlotDefinition>(lair.Definition.ResourceBuildingDefID);
				VillainLairResourcePlot villainLairResourcePlot = villainLairResourcePlotDefinition.Build() as VillainLairResourcePlot;
				villainLairResourcePlot.Location = new Location(resourcePlotDefinition.location.x + lair.Definition.Location.y, resourcePlotDefinition.location.y + lair.Definition.Location.y);
				villainLairResourcePlot.rotation = resourcePlotDefinition.rotation;
				villainLairResourcePlot.parentLair = lair;
				villainLairResourcePlot.indexInLairResourcePlots = i;
				villainLairResourcePlot.State = ((!resourcePlotDefinition.isAutomaticallyUnlocked) ? BuildingState.Inaccessible : BuildingState.Idle);
				villainLairResourcePlot.unlockTransactionID = resourcePlotDefinition.unlockTransactionID;
				playerService.Add(villainLairResourcePlot);
				lair.resourcePlotInstanceIDs.Add(villainLairResourcePlot.ID);
				lair.portalInstanceID = portal.ID;
			}
		}
	}
}
