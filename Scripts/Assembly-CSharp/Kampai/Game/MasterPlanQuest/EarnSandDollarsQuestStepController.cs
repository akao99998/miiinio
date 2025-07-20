using Kampai.Util;
using strange.extensions.context.api;

namespace Kampai.Game.MasterPlanQuest
{
	public class EarnSandDollarsQuestStepController : BuildingTaskQuestStepController
	{
		protected override string BuildingLocName
		{
			get
			{
				return "Anything";
			}
		}

		protected override string DescriptionLocKey
		{
			get
			{
				return "MasterPlanTaskEarnSandDollars";
			}
		}

		public EarnSandDollarsQuestStepController(Quest quest, int stepIndex, IDefinitionService definitionService, IPlayerService playerService, ICrossContextCapable gameContext, IKampaiLogger logger)
			: base(quest, stepIndex, definitionService, playerService, gameContext, logger)
		{
		}
	}
}
