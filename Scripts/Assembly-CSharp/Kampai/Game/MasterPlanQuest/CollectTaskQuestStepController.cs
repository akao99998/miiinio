using Kampai.Util;
using strange.extensions.context.api;

namespace Kampai.Game.MasterPlanQuest
{
	public class CollectTaskQuestStepController : ItemTaskQuestStepController
	{
		protected override string DescriptionLocKey
		{
			get
			{
				return "MasterPlanTaskCollect";
			}
		}

		public CollectTaskQuestStepController(Quest quest, int stepIndex, IDefinitionService definitionService, IPlayerService playerService, ICrossContextCapable gameContext, IKampaiLogger logger)
			: base(quest, stepIndex, definitionService, playerService, gameContext, logger)
		{
		}
	}
}
