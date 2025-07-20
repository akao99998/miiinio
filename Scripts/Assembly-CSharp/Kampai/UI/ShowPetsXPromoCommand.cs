using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.UI
{
	internal sealed class ShowPetsXPromoCommand : Command
	{
		[Inject]
		public IGUIService guiService { get; set; }

		public override void Execute()
		{
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Load, "popup_Pets");
			iGUICommand.skrimScreen = "PetsXPromo";
			iGUICommand.darkSkrim = true;
			guiService.Execute(iGUICommand);
		}
	}
}
