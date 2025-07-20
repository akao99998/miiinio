using Kampai.Game;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.UI.View;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Common
{
	public class ShowSocialPartyNoEventCommand : Command
	{
		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal globalSFX { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		public override void Execute()
		{
			StageBuilding firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<StageBuilding>(3054);
			if (firstInstanceByDefinitionId != null)
			{
				BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
				BuildingObject buildingObject = component.GetBuildingObject(firstInstanceByDefinitionId.ID);
				StageBuildingObject stageBuildingObject = buildingObject as StageBuildingObject;
				if (stageBuildingObject != null)
				{
					stageBuildingObject.UpdateStageState(BuildingState.Idle);
				}
				globalSFX.Dispatch("Play_menu_popUp_01");
				IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Load, "popup_SocialParty_NoEvent");
				iGUICommand.darkSkrim = false;
				iGUICommand.skrimScreen = "SocialSkrim";
				guiService.Execute(iGUICommand);
			}
		}
	}
}
