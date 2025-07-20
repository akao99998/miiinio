using Kampai.Game;
using Kampai.Game.View;
using Kampai.Main;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class ShowBridgeBuildingUICommand : Command
	{
		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public int BridgeBuildingId { get; set; }

		[Inject]
		public PopupMessageSignal popupMessageSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ILandExpansionConfigService landExpansionConfigService { get; set; }

		[Inject]
		public IPositionService positionService { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal sfxSignal { get; set; }

		public override void Execute()
		{
			BridgeBuilding byInstanceId = playerService.GetByInstanceId<BridgeBuilding>(BridgeBuildingId);
			uint quantity = playerService.GetQuantity(StaticItem.LEVEL_ID);
			if (byInstanceId.UnlockLevel > quantity)
			{
				ShowLockMessage(byInstanceId.Definition.AspirationalMessage, byInstanceId.UnlockLevel);
			}
			else
			{
				if (byInstanceId.BridgeId == 0)
				{
					return;
				}
				BridgeDefinition bridgeDefinition = definitionService.Get(byInstanceId.BridgeId) as BridgeDefinition;
				if (bridgeDefinition == null)
				{
					return;
				}
				LandExpansionConfig expansionConfig = landExpansionConfigService.GetExpansionConfig(bridgeDefinition.LandExpansionID);
				if (expansionConfig == null)
				{
					return;
				}
				bool flag = false;
				foreach (int adjacentExpansionId in expansionConfig.adjacentExpansionIds)
				{
					if (playerService.IsExpansionPurchased(adjacentExpansionId))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					ShowLockMessage("BridgeNotAvailable");
					return;
				}
				if (byInstanceId.State == BuildingState.Idle)
				{
					ShowLockMessage("MustPrestigeBobKey");
					return;
				}
				sfxSignal.Dispatch("Play_menu_popUp_01");
				BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
				BridgeBuildingObject bridgeBuildingObject = component.GetBuildingObject(BridgeBuildingId) as BridgeBuildingObject;
				Vector2 uIAnchorRatioPosition = positionService.GetUIAnchorRatioPosition(bridgeBuildingObject.Center);
				ScreenPosition screenPosition = byInstanceId.Definition.ScreenPosition;
				screenPosition = screenPosition ?? new ScreenPosition();
				Vector2 endPosition = new Vector2(screenPosition.x, screenPosition.z);
				BuildingPopupPositionData buildingPopupPositionData = new BuildingPopupPositionData(uIAnchorRatioPosition, endPosition);
				OpenUI(buildingPopupPositionData);
			}
		}

		private void OpenUI(BuildingPopupPositionData buildingPopupPositionData)
		{
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Load, "popup_Confirmation_Expansion");
			iGUICommand.skrimScreen = "BridgeSkrim";
			iGUICommand.darkSkrim = true;
			GUIArguments args = iGUICommand.Args;
			args.Add(BridgeBuildingId);
			args.Add(RushDialogView.RushDialogType.BRIDGE_QUEST);
			args.Add(buildingPopupPositionData);
			guiService.Execute(iGUICommand);
		}

		private void ShowLockMessage(string key, int unlockLevel = -1)
		{
			string type = ((unlockLevel == -1) ? localService.GetString(key) : localService.GetString(key, unlockLevel));
			popupMessageSignal.Dispatch(type, PopupMessageType.NORMAL);
			sfxSignal.Dispatch("Play_action_locked_01");
		}
	}
}
