using Kampai.Util;
using strange.extensions.context.api;

namespace Kampai.Game.MasterPlanQuest
{
	public class EarnLeisurePartyPointsQuestStepController : BuildingTaskQuestStepController
	{
		protected override string BuildingLocName
		{
			get
			{
				return "Distractivities";
			}
		}

		protected override string DescriptionLocKey
		{
			get
			{
				return "MasterPlanTaskEarnLeisurePartyPoints";
			}
		}

		public EarnLeisurePartyPointsQuestStepController(Quest quest, int stepIndex, IDefinitionService definitionService, IPlayerService playerService, ICrossContextCapable gameContext, IKampaiLogger logger)
			: base(quest, stepIndex, definitionService, playerService, gameContext, logger)
		{
		}
	}
}
