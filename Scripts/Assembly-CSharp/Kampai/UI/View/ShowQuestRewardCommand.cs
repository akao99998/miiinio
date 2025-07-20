using Kampai.Game;
using Kampai.Main;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.UI.View
{
	public class ShowQuestRewardCommand : Command
	{
		[Inject]
		public int questInstanceID { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		[Inject]
		public CloseAllOtherMenuSignal closeSignal { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public ShowHUDSignal showHUDSignal { get; set; }

		[Inject]
		public ShowStoreSignal showStoreSignal { get; set; }

		[Inject]
		public IMasterPlanQuestService masterPlanQuestService { get; set; }

		public override void Execute()
		{
			closeSignal.Dispatch(null);
			Quest questByInstanceId = masterPlanQuestService.GetQuestByInstanceId(questInstanceID);
			if (questByInstanceId != null)
			{
				IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Queue, "popup_QuestReward");
				iGUICommand.skrimScreen = "QuestRewardSkrim";
				iGUICommand.darkSkrim = true;
				iGUICommand.Args.Add(questInstanceID);
				guiService.Execute(iGUICommand);
				playSFXSignal.Dispatch("Play_menu_popUp_01");
				showHUDSignal.Dispatch(true);
				showStoreSignal.Dispatch(true);
				if ((questByInstanceId != null && questByInstanceId.state == QuestState.Complete) || questByInstanceId.state == QuestState.Harvestable)
				{
					gameContext.injectionBinder.GetInstance<RemoveQuestWorldIconSignal>().Dispatch(questByInstanceId);
				}
			}
		}
	}
}
