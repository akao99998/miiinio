using System.Collections;
using Kampai.Common;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class StartCOPPAFlowCommand : Command
	{
		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public ICoppaService coppaService { get; set; }

		[Inject]
		public CoppaCompletedSignal coppaCompletedSignal { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		public override void Execute()
		{
			if (!coppaService.IsBirthdateKnown())
			{
				IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Load, "COPPA_Age_Gate_Panel");
				iGUICommand.skrimScreen = "CoppaAgeGate";
				iGUICommand.disableSkrimButton = true;
				iGUICommand.darkSkrim = false;
				guiService.Execute(iGUICommand);
			}
			else
			{
				routineRunner.StartCoroutine(CompleteCoppa());
			}
		}

		private IEnumerator CompleteCoppa()
		{
			yield return new WaitForSeconds(2f);
			coppaCompletedSignal.Dispatch();
		}
	}
}
