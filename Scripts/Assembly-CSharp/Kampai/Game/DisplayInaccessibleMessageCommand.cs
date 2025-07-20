using Kampai.Game.View;
using Kampai.Main;
using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class DisplayInaccessibleMessageCommand : Command
	{
		[Inject]
		public Building hitBuilding { get; set; }

		[Inject]
		public BuildingObject hitBuildingObject { get; set; }

		[Inject]
		public PopupMessageSignal popupMessageSignal { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal globalSFXSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public OpenBuildingMenuSignal openBuildingMenuSignal { get; set; }

		[Inject]
		public OpenVillainLairResourcePlotBuildingSignal openResourcePlotBuildingSignal { get; set; }

		public override void Execute()
		{
			VillainLairEntranceBuilding villainLairEntranceBuilding = hitBuilding as VillainLairEntranceBuilding;
			if (villainLairEntranceBuilding != null)
			{
				if (playerService.GetQuantity(StaticItem.LEVEL_ID) < villainLairEntranceBuilding.Definition.UnlockAtLevel)
				{
					DisplayMessage(localizationService.GetString(villainLairEntranceBuilding.Definition.AspirationalMessage_NeedLevel, villainLairEntranceBuilding.Definition.UnlockAtLevel));
				}
				else
				{
					openBuildingMenuSignal.Dispatch(hitBuildingObject, hitBuilding);
				}
			}
			MinionUpgradeBuilding minionUpgradeBuilding = hitBuilding as MinionUpgradeBuilding;
			if (minionUpgradeBuilding != null)
			{
				if (playerService.GetQuantity(StaticItem.LEVEL_ID) < minionUpgradeBuilding.Definition.UnlockAtLevel)
				{
					DisplayMessage(localizationService.GetString(minionUpgradeBuilding.Definition.AspirationalMessage_NeedLevel, minionUpgradeBuilding.Definition.UnlockAtLevel));
				}
				else
				{
					DisplayMessage(localizationService.GetString(minionUpgradeBuilding.Definition.AspirationalMessage_NeedQuest));
				}
				return;
			}
			StageBuilding stageBuilding = hitBuilding as StageBuilding;
			if (stageBuilding != null)
			{
				DisplayMessage(localizationService.GetString(stageBuilding.Definition.AspirationalMessage));
				return;
			}
			VillainLairResourcePlot villainLairResourcePlot = hitBuilding as VillainLairResourcePlot;
			if (villainLairResourcePlot != null)
			{
				openResourcePlotBuildingSignal.Dispatch(villainLairResourcePlot);
				return;
			}
			FountainBuilding fountainBuilding = hitBuilding as FountainBuilding;
			if (fountainBuilding != null)
			{
				DisplayMessage(localizationService.GetString(fountainBuilding.Definition.AspirationalMessage));
			}
		}

		private void DisplayMessage(string message)
		{
			globalSFXSignal.Dispatch("Play_action_locked_01");
			popupMessageSignal.Dispatch(message, PopupMessageType.NORMAL);
		}
	}
}
