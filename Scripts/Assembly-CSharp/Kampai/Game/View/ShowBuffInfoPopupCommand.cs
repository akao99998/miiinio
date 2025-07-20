using Kampai.UI.View;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game.View
{
	public class ShowBuffInfoPopupCommand : Command
	{
		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public Vector3 location { get; set; }

		[Inject]
		public float offset { get; set; }

		public override void Execute()
		{
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Load, "screen_BuffPopup");
			iGUICommand.skrimScreen = "GenericPopup";
			iGUICommand.darkSkrim = false;
			iGUICommand.genericPopupSkrim = true;
			iGUICommand.Args.Add(location);
			iGUICommand.Args.Add(offset);
			guiService.Execute(iGUICommand);
		}
	}
}
