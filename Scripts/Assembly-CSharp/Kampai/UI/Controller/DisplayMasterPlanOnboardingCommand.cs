using System;
using Kampai.Game;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.UI.Controller
{
	public class DisplayMasterPlanOnboardingCommand : Command
	{
		[Inject]
		public int onboardDefinitionId { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public CameraMoveToCustomPositionSignal customCameraPositionSignal { get; set; }

		[Inject]
		public IGhostComponentService ghostService { get; set; }

		public override void Execute()
		{
			MasterPlanOnboardDefinition masterPlanOnboardDefinition = definitionService.Get<MasterPlanOnboardDefinition>(onboardDefinitionId);
			customCameraPositionSignal.Dispatch(masterPlanOnboardDefinition.CustomCameraPosID, new Boxed<Action>(null));
			ghostService.RunBeginGhostComponentFunctionFromDefinition(masterPlanOnboardDefinition.ghostFunction.startType, masterPlanOnboardDefinition.ghostFunction.componentBuildingDefID);
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Queue, "screen_MasterPlanOnboarding");
			iGUICommand.Args.Add(masterPlanOnboardDefinition);
			iGUICommand.skrimScreen = "MasterPlanOnboarding";
			iGUICommand.darkSkrim = false;
			iGUICommand.disableSkrimButton = true;
			guiService.Execute(iGUICommand);
		}
	}
}
