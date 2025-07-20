using Kampai.Common;
using Kampai.Game.View;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class MinionUpgradeCommand : Command
	{
		private int tokensToLevel;

		[Inject]
		public int minionInstanceID { get; set; }

		[Inject]
		public uint tokenQuantityPreLevel { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public PostMinionUpgradeSignal postUpgradeSignal { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ChangeMinionCostumeSignal changeMinionCostumeSignal { get; set; }

		[Inject(GameElement.MINION_MANAGER)]
		public GameObject minionManager { get; set; }

		[Inject]
		public CheckPopulationBenefitSignal populationBenefitSignal { get; set; }

		public override void Execute()
		{
			playerService.LevelupMinion(minionInstanceID);
			questService.UpdateAllQuestsWithQuestStepType(QuestStepType.MinionUpgrade, QuestTaskTransition.Complete);
			questService.UpdateAllQuestsWithQuestStepType(QuestStepType.HaveUpgradedMinions);
			Minion byInstanceId = playerService.GetByInstanceId<Minion>(minionInstanceID);
			if (byInstanceId != null)
			{
				MinionBenefitLevelBandDefintion minionBenefitLevelBandDefintion = definitionService.Get<MinionBenefitLevelBandDefintion>(StaticItem.MINION_BENEFITS_DEF_ID);
				tokensToLevel = minionBenefitLevelBandDefintion.minionBenefitLevelBands[byInstanceId.Level - 1].tokensToLevel;
				MinionManagerView component = minionManager.GetComponent<MinionManagerView>();
				MinionObject minionObject = component.Get(minionInstanceID);
				if (minionObject != null)
				{
					int costumeId = minionBenefitLevelBandDefintion.GetMinionBenefit(byInstanceId.Level).costumeId;
					CostumeItemDefinition costumeItemDefinition = definitionService.Get<CostumeItemDefinition>(costumeId);
					if (costumeItemDefinition != null)
					{
						changeMinionCostumeSignal.Dispatch(minionObject, costumeItemDefinition);
					}
				}
			}
			SendTelemetry();
			populationBenefitSignal.Dispatch(byInstanceId.Level);
			postUpgradeSignal.Dispatch();
		}

		private void SendTelemetry()
		{
			int newLevel = playerService.GetByInstanceId<Minion>(minionInstanceID).Level + 1;
			telemetryService.Send_Telemetry_EVT_MINION_UPGRADE(newLevel, tokensToLevel, tokenQuantityPreLevel);
		}
	}
}
