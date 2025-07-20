using Kampai.Game;
using Kampai.Main;
using Kampai.UI.View;
using strange.extensions.command.impl;

public class ClickedVillainLairComponentBuildingCommand : Command
{
	[Inject]
	public MasterPlanComponentDefinition componentDefinition { get; set; }

	[Inject]
	public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

	[Inject]
	public DisplayMasterPlanSignal displayMasterPlanSignal { get; set; }

	public override void Execute()
	{
		if (!string.IsNullOrEmpty(componentDefinition.OnClickSound))
		{
			playSFXSignal.Dispatch(componentDefinition.OnClickSound);
		}
		displayMasterPlanSignal.Dispatch(componentDefinition.ID);
	}
}
