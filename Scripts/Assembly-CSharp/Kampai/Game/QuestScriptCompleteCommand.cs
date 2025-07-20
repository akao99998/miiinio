using Elevation.Logging;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class QuestScriptCompleteCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("QuestScriptCompleteCommand") as IKampaiLogger;

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public QuestScriptInstance questScriptInstance { get; set; }

		public override void Execute()
		{
			IQuestController questControllerByDefinitionID = questService.GetQuestControllerByDefinitionID(questScriptInstance.QuestID);
			if (questControllerByDefinitionID != null)
			{
				questControllerByDefinitionID.OnQuestScriptComplete(questScriptInstance);
				return;
			}
			logger.Error("Quest Controller with definition Id {0} doesn't exist in quest map", questScriptInstance.QuestID);
		}
	}
}
