using Kampai.Game;
using Kampai.Game.View;
using Kampai.Main;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class ShowBuildingDetailMenuCommand : Command
	{
		private bool isMignetteCooldown;

		[Inject]
		public Building building { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		[Inject]
		public StartMignetteSignal startMignetteSignal { get; set; }

		[Inject]
		public ShowBridgeUISignal bridgeSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public OpenOrderBoardSignal openOrderBoardSignal { get; set; }

		[Inject]
		public HideCharactersSignal hideCharactersSignal { get; set; }

		[Inject]
		public CloseAllOtherMenuSignal closeSignal { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject]
		public OpenVillainLairPortalBuildingSignal openVillainLairPortalSignal { get; set; }

		[Inject]
		public IPositionService positionService { get; set; }

		public override void Execute()
		{
			BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
			BuildingObject buildingObject = component.GetBuildingObject(building.ID);
			VillainLairEntranceBuilding villainLairEntranceBuilding = building as VillainLairEntranceBuilding;
			if (villainLairEntranceBuilding != null)
			{
				openVillainLairPortalSignal.Dispatch(villainLairEntranceBuilding, buildingObject as VillainLairEntranceBuildingObject);
				return;
			}
			IGUICommand command = GetCommand();
			if (command != null)
			{
				Vector2 uIAnchorRatioPosition = positionService.GetUIAnchorRatioPosition(buildingObject.ZoomCenter);
				ScreenPosition screenPosition = building.Definition.ScreenPosition;
				screenPosition = screenPosition ?? new ScreenPosition();
				Vector2 endPosition = new Vector2(screenPosition.x, screenPosition.z);
				if (isMignetteCooldown)
				{
					playSFXSignal.Dispatch("Play_menu_popUp_02");
				}
				else
				{
					playSFXSignal.Dispatch("Play_menu_popUp_01");
				}
				closeSignal.Dispatch(null);
				command.Args.Add(building);
				command.Args.Add(new BuildingPopupPositionData(uIAnchorRatioPosition, endPosition));
				guiService.Execute(command);
			}
		}

		private IGUICommand GetCommand()
		{
			string skrimName = null;
			bool darkSkrim = false;
			IGUICommand command = null;
			if (building is CraftingBuilding)
			{
				darkSkrim = false;
				skrimName = "CraftingSkrim";
				command = guiService.BuildCommand(GUIOperation.Queue, "screen_CraftingMenu");
				command.Args.Add(building.ID);
			}
			else if (building is OrderBoard)
			{
				OrderBoard orderBoard = building as OrderBoard;
				if (orderBoard != null && orderBoard.menuEnabled)
				{
					openOrderBoardSignal.Dispatch(orderBoard);
					return null;
				}
			}
			else
			{
				if (building is BridgeBuilding)
				{
					bridgeSignal.Dispatch(building.ID);
					return null;
				}
				if (building is ResourceBuilding)
				{
					getResourceCommand(building as ResourceBuilding, out command, out darkSkrim, out skrimName);
					if (command.prefab.Equals("screen_BaseResource"))
					{
						hideCharactersSignal.Dispatch();
					}
				}
				else if (building is MignetteBuilding)
				{
					MignetteBuilding mignetteBuilding = (MignetteBuilding)building;
					if (MignetteIsRunning(mignetteBuilding))
					{
						return null;
					}
					darkSkrim = false;
					skrimName = "MignetteSkrim";
					bool flag = playerService.HasPurchasedMinigamePack();
					command = guiService.BuildCommand(GUIOperation.Queue, mignetteBuilding.SelectMenuToLoad(flag));
					if (mignetteBuilding.State == BuildingState.Cooldown && !flag)
					{
						isMignetteCooldown = true;
					}
				}
				else if (building is DebrisBuilding)
				{
					if (building.State == BuildingState.Idle)
					{
						darkSkrim = false;
						skrimName = "DebrisSkrim";
						command = guiService.BuildCommand(GUIOperation.Queue, "screen_ClearDebris");
						command.Args.Add(RushDialogView.RushDialogType.DEBRIS);
					}
				}
				else if (building.HasDetailMenuToShow())
				{
					darkSkrim = false;
					skrimName = "BuildingSkrim";
					command = guiService.BuildCommand(GUIOperation.Queue, building.Definition.MenuPrefab);
				}
			}
			if (!string.IsNullOrEmpty(skrimName))
			{
				command.skrimScreen = skrimName;
				command.darkSkrim = darkSkrim;
			}
			return command;
		}

		private bool MignetteIsRunning(MignetteBuilding mignetteBuilding)
		{
			if (mignetteBuilding.AreAllMinionSlotsFilled() && !mignetteBuilding.Definition.ShowPlayConfirmMenu)
			{
				startMignetteSignal.Dispatch(mignetteBuilding.ID);
				return true;
			}
			return false;
		}

		private void getResourceCommand(ResourceBuilding resourceBuilding, out IGUICommand command, out bool darkSkrim, out string skrimName)
		{
			StorageBuilding firstInstanceByDefintion = playerService.GetFirstInstanceByDefintion<StorageBuilding, StorageBuildingDefinition>();
			if (playerService.isStorageFull() && resourceBuilding.AvailableHarvest > 0 && firstInstanceByDefintion != null)
			{
				command = guiService.BuildCommand(GUIOperation.Queue, "screen_StorageBuilding");
				skrimName = "StorageSkrim";
				darkSkrim = true;
				command.Args.Add(firstInstanceByDefintion);
			}
			else
			{
				darkSkrim = false;
				skrimName = "BuildingSkrim";
				command = guiService.BuildCommand(GUIOperation.Queue, "screen_BaseResource");
			}
		}
	}
}
