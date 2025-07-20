using Kampai.Util;
using strange.extensions.context.api;

namespace Kampai.Game.MasterPlanQuest
{
	public class MiniGameScoreQuestStepController : BuildingTaskQuestStepController
	{
		protected override string BuildingLocName
		{
			get
			{
				return "MiniGames";
			}
		}

		protected override string DescriptionLocKey
		{
			get
			{
				return "MasterPlanTaskMiniGameScore";
			}
		}

		public MiniGameScoreQuestStepController(Quest quest, int stepIndex, IDefinitionService definitionService, IPlayerService playerService, ICrossContextCapable gameContext, IKampaiLogger logger)
			: base(quest, stepIndex, definitionService, playerService, gameContext, logger)
		{
		}

		public override void SetupTracking()
		{
			int itemDefinitionID = base.questStepDefinition.ItemDefinitionID;
			if (itemDefinitionID != 0)
			{
				MignetteBuilding firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<MignetteBuilding>(itemDefinitionID);
				if (firstInstanceByDefinitionId == null)
				{
					logger.Fatal(FatalCode.PS_MISSING_MIGNETTE, "Mignette instance not found!");
				}
				else
				{
					base.questStep.TrackedID = firstInstanceByDefinitionId.ID;
				}
			}
		}
	}
}
