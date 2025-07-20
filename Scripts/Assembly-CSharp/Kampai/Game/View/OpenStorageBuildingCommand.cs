using System.Collections;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game.View
{
	public class OpenStorageBuildingCommand : Command
	{
		[Inject]
		public StorageBuilding building { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public BuildingChangeStateSignal stateChangeSignal { get; set; }

		[Inject]
		public bool directOpenMenu { get; set; }

		public override void Execute()
		{
			stateChangeSignal.Dispatch(building.ID, BuildingState.Working);
			building.MenuOpening = true;
			if (directOpenMenu)
			{
				routineRunner.StartCoroutine(WaitAFrame());
			}
			else
			{
				routineRunner.StartCoroutine(DelayOpenGUI());
			}
		}

		private void OpenGUI()
		{
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Load, "screen_StorageBuilding");
			iGUICommand.skrimScreen = "StorageSkrim";
			iGUICommand.darkSkrim = true;
			iGUICommand.Args.Add(building);
			iGUICommand.Args.Add(StorageBuildingModalTypes.STORAGE);
			guiService.Execute(iGUICommand);
		}

		private IEnumerator DelayOpenGUI()
		{
			yield return new WaitForSeconds(0f);
			OpenGUI();
		}

		private IEnumerator WaitAFrame()
		{
			yield return null;
			OpenGUI();
		}
	}
}
