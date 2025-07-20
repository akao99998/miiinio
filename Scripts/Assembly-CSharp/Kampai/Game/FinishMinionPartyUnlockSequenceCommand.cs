using Kampai.Game.View;
using Kampai.UI;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class FinishMinionPartyUnlockSequenceCommand : Command
	{
		private const string UNLOCK_FINISH_LOC_KEY = "qc2000000174_step2_intro";

		private const int PINATA_DEFINITION_ID = 3123;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public PromptReceivedSignal promptReceivedSignal { get; set; }

		[Inject]
		public ShowDialogSignal showDialog { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject]
		public OpenBuildingMenuSignal openBuildingMenuSignal { get; set; }

		[Inject]
		public SetXPSignal setXPSignal { get; set; }

		public override void Execute()
		{
			QuestDialogSetting type = new QuestDialogSetting();
			Tuple<int, int> type2 = new Tuple<int, int>(0, 0);
			setXPSignal.Dispatch();
			promptReceivedSignal.AddOnce(DialogDismissed);
			showDialog.Dispatch("qc2000000174_step2_intro", type, type2);
		}

		private void DialogDismissed(int param1, int param2)
		{
			if (param1 == 0 && param2 == 0)
			{
				guiService.AddToArguments(new ThrobCallButtons());
				Building firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<Building>(3123);
				BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
				BuildingObject buildingObject = component.GetBuildingObject(firstInstanceByDefinitionId.ID);
				openBuildingMenuSignal.Dispatch(buildingObject, firstInstanceByDefinitionId);
				guiService.RemoveFromArguments(typeof(ThrobCallButtons));
			}
		}
	}
}
