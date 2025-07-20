using System.Collections.Generic;
using Kampai.Common;
using Kampai.UI.View;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class CheckPopulationBenefitCommand : Command
	{
		[Inject]
		public int currentLevel { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public SpawnDooberModel dooberModel { get; set; }

		[Inject]
		public DisplayPlayerTrainingSignal playerTrainingSignal { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		public override void Execute()
		{
			List<PopulationBenefitDefinition> all = definitionService.GetAll<PopulationBenefitDefinition>();
			MinionUpgradeBuilding byInstanceId = playerService.GetByInstanceId<MinionUpgradeBuilding>(375);
			for (int i = 0; i < all.Count; i++)
			{
				PopulationBenefitDefinition populationBenefitDefinition = all[i];
				if (populationBenefitDefinition.minionLevelRequired == currentLevel)
				{
					int iD = populationBenefitDefinition.ID;
					if (!byInstanceId.processedPopulationBenefitDefinitionIDs.Contains(iD) && playerService.GetMinionCountAtOrAboveLevel(currentLevel) >= populationBenefitDefinition.numMinionsRequired)
					{
						playerService.RunEntireTransaction(populationBenefitDefinition.transactionDefinitionID, TransactionTarget.NO_VISUAL, null);
						byInstanceId.processedPopulationBenefitDefinitionIDs.Add(iD);
						dooberModel.PendingPopulationDoober = iD;
						playerTrainingSignal.Dispatch(19000029, false, new Signal<bool>());
						telemetryService.Send_Telemetry_EVT_MINION_POPULATION_BENEFIT(populationBenefitDefinition.LocalizedKey);
					}
				}
			}
		}
	}
}
