using Kampai.Game;
using Kampai.Main;
using Kampai.UI.View;
using strange.extensions.command.impl;

public class OpenVillainLairResourcePlotBuildingCommand : Command
{
	[Inject]
	public VillainLairResourcePlot resourcePlot { get; set; }

	[Inject]
	public PlayGlobalSoundFXSignal sfxSignal { get; set; }

	[Inject]
	public IGUIService guiService { get; set; }

	[Inject]
	public CloseAllMessageDialogs closeAllMessageDialogsSignal { get; set; }

	[Inject]
	public VillainLairModel villainLairModel { get; set; }

	public override void Execute()
	{
		if (!villainLairModel.leavingLair)
		{
			if (resourcePlot.State == BuildingState.Inaccessible)
			{
				OpenModal("screen_Resource_Lair_Locked");
			}
			else
			{
				OpenModal("screen_Resource_Lair_Unlocked");
			}
		}
	}

	private void OpenModal(string prefabName)
	{
		closeAllMessageDialogsSignal.Dispatch();
		sfxSignal.Dispatch("Play_menu_popUp_01");
		IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Load, prefabName);
		iGUICommand.skrimScreen = "VillainLairResourceSkrim";
		GUIArguments args = iGUICommand.Args;
		args.Add(resourcePlot);
		args.Add(RushDialogView.RushDialogType.VILLAIN_LAIR_RESOURCE_PLOT);
		guiService.Execute(iGUICommand);
	}
}
