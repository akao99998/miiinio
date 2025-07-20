using Kampai.Util;
using strange.extensions.context.api;

namespace Kampai.Game.MasterPlanQuest
{
	public class EarnMignettePartyPointsQuestStepController : BuildingTaskQuestStepController
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
				return "MasterPlanTaskEarnMignettePartyPoints";
			}
		}

		public EarnMignettePartyPointsQuestStepController(Quest quest, int stepIndex, IDefinitionService definitionService, IPlayerService playerService, ICrossContextCapable gameContext, IKampaiLogger logger)
			: base(quest, stepIndex, definitionService, playerService, gameContext, logger)
		{
		}
	}
}
