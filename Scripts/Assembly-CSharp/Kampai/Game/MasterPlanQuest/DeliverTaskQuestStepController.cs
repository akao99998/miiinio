using Kampai.Util;
using strange.extensions.context.api;

namespace Kampai.Game.MasterPlanQuest
{
	public class DeliverTaskQuestStepController : ItemTaskQuestStepController
	{
		protected override string DescriptionLocKey
		{
			get
			{
				return "MasterPlanTaskDeliver";
			}
		}

		public DeliverTaskQuestStepController(Quest quest, int stepIndex, IDefinitionService definitionService, IPlayerService playerService, ICrossContextCapable gameContext, IKampaiLogger logger)
			: base(quest, stepIndex, definitionService, playerService, gameContext, logger)
		{
		}
	}
}
