using Elevation.Logging;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class MinionPartyReadyCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("MinionPartyReadyCommand") as IKampaiLogger;

		[Inject]
		public IQuestService questService { get; set; }

		public override void Execute()
		{
			logger.Debug("Minion Party is Ready");
			questService.UpdateAllQuestsWithQuestStepType(QuestStepType.ThrowParty);
		}
	}
}
