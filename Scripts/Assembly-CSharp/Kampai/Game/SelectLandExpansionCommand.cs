using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.UI;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class SelectLandExpansionCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("SelectLandExpansionCommand") as IKampaiLogger;

		private PurchasedLandExpansion purchasedLandExpansion;

		private LandExpansionDefinition definition;

		private BuildingDefinition buildingDef;

		[Inject]
		public int buildingID { get; set; }

		[Inject]
		public ILandExpansionService landExpansionService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public IPositionService positionService { get; set; }

		[Inject]
		public CameraAutoMoveSignal AutoMoveSignal { get; set; }

		[Inject]
		public PopupMessageSignal popupMessageSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		[Inject]
		public HighlightLandExpansionSignal highlightSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal sfxSignal { get; set; }

		[Inject]
		public PromptReceivedSignal promptReceivedSignal { get; set; }

		[Inject]
		public ShowDialogSignal showDialog { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		public override void Execute()
		{
			BobCharacter firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<BobCharacter>(70002);
			int expansionByItemID = landExpansionService.GetExpansionByItemID(buildingID);
			purchasedLandExpansion = playerService.GetByInstanceId<PurchasedLandExpansion>(354);
			definition = FindLandExpansion(expansionByItemID);
			if (ExpansionIsPurchaseable(expansionByItemID))
			{
				int minimumLevel = definition.MinimumLevel;
				if (minimumLevel > playerService.GetQuantity(StaticItem.LEVEL_ID))
				{
					Error(localService.GetString("LandLockedByLevel", minimumLevel));
					return;
				}
				GameObject forSaleSign = landExpansionService.GetForSaleSign(expansionByItemID);
				Vector3 position = forSaleSign.transform.position;
				highlightSignal.Dispatch(expansionByItemID, true);
				buildingDef = definitionService.Get(definition.BuildingDefinitionID) as BuildingDefinition;
				AutoMoveSignal.Dispatch(position, new Boxed<ScreenPosition>(buildingDef.ScreenPosition), new CameraMovementSettings(CameraMovementSettings.Settings.None, null, null), false);
				if (expansionByItemID == playerService.GetTargetExpansion() && firstInstanceByDefinitionId != null && !firstInstanceByDefinitionId.HasShownExpansionNarrative)
				{
					QuestDialogSetting type = new QuestDialogSetting();
					Tuple<int, int> type2 = new Tuple<int, int>(0, 0);
					promptReceivedSignal.AddOnce(DialogDismissed);
					showDialog.Dispatch("BobExpansionNarrative", type, type2);
					firstInstanceByDefinitionId.HasShownExpansionNarrative = true;
				}
				else
				{
					ShowMenu(expansionByItemID);
				}
			}
			else if (!HasPurchased(expansionByItemID))
			{
				Error(localService.GetString("LandLockedByOtherLand"));
			}
		}

		private void DialogDismissed(int param1, int param2)
		{
			if (param1 == 0 && param2 == 0)
			{
				int expansionByItemID = landExpansionService.GetExpansionByItemID(buildingID);
				ShowMenu(expansionByItemID);
			}
		}

		private void Error(string message)
		{
			sfxSignal.Dispatch("Play_action_locked_01");
			popupMessageSignal.Dispatch(message, PopupMessageType.NORMAL);
		}

		private bool HasPurchased(int expansionID)
		{
			return purchasedLandExpansion.HasPurchased(expansionID);
		}

		private bool ExpansionIsPurchaseable(int expansionID)
		{
			return purchasedLandExpansion.IsUnpurchasedAdjacentExpansion(expansionID);
		}

		private void ShowMenu(int expansionID)
		{
			playSFXSignal.Dispatch("Play_menu_popUp_01");
			BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
			LandExpansionBuildingObject landExpansionBuildingObject = component.GetBuildingObject(buildingID) as LandExpansionBuildingObject;
			Vector2 uIAnchorRatioPosition = positionService.GetUIAnchorRatioPosition(landExpansionBuildingObject.ZoomCenter);
			ScreenPosition screenPosition = ((buildingDef == null) ? new ScreenPosition() : buildingDef.ScreenPosition);
			screenPosition = screenPosition ?? new ScreenPosition();
			Vector2 endPosition = new Vector2(screenPosition.x, screenPosition.z);
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Load, "popup_Confirmation_Expansion");
			iGUICommand.skrimScreen = "LandExpansionSkrim";
			iGUICommand.darkSkrim = false;
			GUIArguments args = iGUICommand.Args;
			args.Add(expansionID);
			args.Add(RushDialogView.RushDialogType.LAND_EXPANSION);
			args.Add(new BuildingPopupPositionData(uIAnchorRatioPosition, endPosition));
			guiService.Execute(iGUICommand);
		}

		private LandExpansionDefinition FindLandExpansion(int expansionId)
		{
			List<LandExpansionDefinition> all = definitionService.GetAll<LandExpansionDefinition>();
			for (int i = 0; i < all.Count; i++)
			{
				if (all[i].ExpansionID == expansionId)
				{
					return all[i];
				}
			}
			logger.Fatal(FatalCode.DS_NO_SUCH_LAND_EXPANSION, expansionId);
			return null;
		}
	}
}
