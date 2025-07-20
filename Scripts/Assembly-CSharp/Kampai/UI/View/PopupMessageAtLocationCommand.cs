using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class PopupMessageAtLocationCommand : Command
	{
		[Inject]
		public string localizedText { get; set; }

		[Inject]
		public MessagePopUpAnchor anchor { get; set; }

		[Inject]
		public Vector2 anchorPosition { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		public override void Execute()
		{
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.LoadStatic, "popup_MessageBox");
			GUIArguments args = iGUICommand.Args;
			args.Add(localizedText);
			args.Add(anchor);
			args.Add(anchorPosition);
			guiService.Execute(iGUICommand);
		}
	}
}
