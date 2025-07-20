using Kampai.Game;
using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Main
{
	public class VillainIslandMessageController : Command
	{
		[Inject]
		public bool showMessage { get; set; }

		[Inject]
		public UIModel model { get; set; }

		[Inject]
		public PopupMessageSignal popupMessageSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal sfxSignal { get; set; }

		[Inject]
		public EnableVillainIslandCollidersSignal enableVillainIslandCollidersSignal { get; set; }

		public override void Execute()
		{
			int quantity = (int)playerService.GetQuantity(StaticItem.LEVEL_ID);
			int num = 0;
			QuestDefinition questDefinition = definitionService.Get<QuestDefinition>(3849290);
			if (questDefinition != null)
			{
				num = questDefinition.UnlockLevel;
			}
			IQuestController questControllerByDefinitionID = questService.GetQuestControllerByDefinitionID(3849290);
			if (quantity < num && showMessage)
			{
				PopupMessage(localService.GetString("AspirationalMessageVillainIsland", num));
			}
			else if ((questControllerByDefinitionID != null && questControllerByDefinitionID.State != 0) || questService.IsQuestCompleted(3849290))
			{
				enableVillainIslandCollidersSignal.Dispatch(false);
			}
			else if (showMessage)
			{
				PopupMessage(localService.GetString("UnlockKevinForVillainIsland", num));
			}
		}

		private void PopupMessage(string message)
		{
			if (!model.BuildingDragMode)
			{
				popupMessageSignal.Dispatch(message, PopupMessageType.NORMAL);
				sfxSignal.Dispatch("Play_action_locked_01");
			}
		}
	}
}
