using Kampai.Game;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.UI;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

public class OpenVillainLairPortalBuildingCommand : Command
{
	[Inject]
	public VillainLairEntranceBuilding lairPortal { get; set; }

	[Inject]
	public VillainLairEntranceBuildingObject lairPortalObject { get; set; }

	[Inject]
	public IPlayerService playerService { get; set; }

	[Inject]
	public PopupMessageSignal popupMessageSignal { get; set; }

	[Inject]
	public CameraAutoMoveSignal AutoMoveSignal { get; set; }

	[Inject]
	public ILocalizationService localService { get; set; }

	[Inject]
	public IGUIService guiService { get; set; }

	[Inject]
	public PlayGlobalSoundFXSignal sfxSignal { get; set; }

	[Inject]
	public IQuestService questService { get; set; }

	[Inject]
	public UnlockVillainLairSignal unlockVillainLairSignal { get; set; }

	[Inject]
	public IPositionService positionService { get; set; }

	public override void Execute()
	{
		if (lairPortal.State == BuildingState.Inaccessible)
		{
			IQuestController questControllerByDefinitionID = questService.GetQuestControllerByDefinitionID(3849292);
			if (questControllerByDefinitionID == null || questControllerByDefinitionID.State == QuestState.Notstarted)
			{
				popupMessageSignal.Dispatch(localService.GetString(lairPortal.Definition.AspirationalMessage_NeedKevinsQuest), PopupMessageType.NORMAL);
			}
			else
			{
				OpenModal("screen_UnlockLair", true);
			}
			return;
		}
		if (!lairPortal.IsUnlocked)
		{
			unlockVillainLairSignal.Dispatch(lairPortal, 3137);
		}
		VillainLair byInstanceId = playerService.GetByInstanceId<VillainLair>(lairPortal.VillainLairInstanceID);
		string prefabName = ((!byInstanceId.hasVisited) ? "screen_EnterLair" : "screen_Resource_LairPortal");
		OpenModal(prefabName, false);
	}

	private void OpenModal(string prefabName, bool isBuildModal)
	{
		Pan();
		sfxSignal.Dispatch("Play_menu_popUp_01");
		IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Load, prefabName);
		GUIArguments args = iGUICommand.Args;
		iGUICommand.skrimScreen = "VillainLairPortalSkrim";
		args.Add(lairPortal);
		if (isBuildModal)
		{
			iGUICommand.darkSkrim = true;
			args.Add(lairPortal.ID);
			args.Add(RushDialogView.RushDialogType.VILLAIN_LAIR_PORTAL_REPAIR);
		}
		else
		{
			args.Add(3137);
		}
		Vector2 uIAnchorRatioPosition = positionService.GetUIAnchorRatioPosition(lairPortalObject.transform.position);
		ScreenPosition screenPosition = lairPortal.Definition.ScreenPosition;
		screenPosition = screenPosition ?? new ScreenPosition();
		Vector2 endPosition = new Vector2(screenPosition.x, screenPosition.z);
		args.Add(new BuildingPopupPositionData(uIAnchorRatioPosition, endPosition));
		guiService.Execute(iGUICommand);
	}

	private void Pan()
	{
		Vector3 position = lairPortalObject.transform.position;
		ScreenPosition screenPosition = lairPortal.Definition.ScreenPosition;
		CameraMovementSettings cameraMovementSettings = new CameraMovementSettings(CameraMovementSettings.Settings.KeepUIOpen, lairPortal, null);
		cameraMovementSettings.cameraSpeed = 0.4f;
		AutoMoveSignal.Dispatch(position, new Boxed<ScreenPosition>(screenPosition), cameraMovementSettings, false);
	}
}
