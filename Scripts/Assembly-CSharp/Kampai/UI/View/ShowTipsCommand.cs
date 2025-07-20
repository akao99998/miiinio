using Kampai.Main;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class ShowTipsCommand : Command
	{
		[Inject]
		public string localizedKey { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistService { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		public override void Execute()
		{
			if (!localPersistService.HasKey(localizedKey))
			{
				IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Queue, "popup_Tip");
				iGUICommand.skrimScreen = "DidYouKnowSkrim";
				iGUICommand.darkSkrim = false;
				string @string = localService.GetString(localizedKey);
				GUIArguments args = iGUICommand.Args;
				args.Add(@string);
				guiService.Execute(iGUICommand);
			}
		}
	}
}
