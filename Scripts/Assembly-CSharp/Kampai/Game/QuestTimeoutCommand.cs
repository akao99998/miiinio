using Kampai.Main;
using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class QuestTimeoutCommand : Command
	{
		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public UpdateQuestBookBadgeSignal updateBadgeSignal { get; set; }

		[Inject]
		public PopupMessageSignal messageSignal { get; set; }

		[Inject]
		public RemoveQuestWorldIconSignal removeQuestIconSignal { get; set; }

		[Inject]
		public UpdateQuestWorldIconsSignal updateQuestWorldIconSignal { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public int questId { get; set; }

		public override void Execute()
		{
			guiService.Execute(GUIOperation.Unload, "screen_QuestPanel");
			updateBadgeSignal.Dispatch();
			string @string = localService.GetString("QuestTimeout");
			messageSignal.Dispatch(@string, PopupMessageType.NORMAL);
			Quest byInstanceId = playerService.GetByInstanceId<Quest>(questId);
			if (byInstanceId == null)
			{
				return;
			}
			TimedQuestDefinition timedQuestDefinition = byInstanceId.GetActiveDefinition() as TimedQuestDefinition;
			if (timedQuestDefinition != null)
			{
				if (timedQuestDefinition.Repeat)
				{
					byInstanceId.Clear();
					updateQuestWorldIconSignal.Dispatch(byInstanceId);
				}
				else
				{
					IQuestController questControllerByDefinitionID = questService.GetQuestControllerByDefinitionID(byInstanceId.GetActiveDefinition().ID);
					if (questControllerByDefinitionID != null)
					{
						questControllerByDefinitionID.GoToQuestState(QuestState.Complete);
					}
					removeQuestIconSignal.Dispatch(byInstanceId);
				}
			}
			LimitedQuestDefinition limitedQuestDefinition = byInstanceId.GetActiveDefinition() as LimitedQuestDefinition;
			if (limitedQuestDefinition != null)
			{
				questService.RemoveQuest(byInstanceId.GetActiveDefinition().ID);
				removeQuestIconSignal.Dispatch(byInstanceId);
			}
		}
	}
}
