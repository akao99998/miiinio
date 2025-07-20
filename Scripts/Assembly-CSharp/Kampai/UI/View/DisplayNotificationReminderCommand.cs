using Kampai.Common;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class DisplayNotificationReminderCommand : Command
	{
		[Inject]
		public string message { get; set; }

		[Inject]
		public bool autoClose { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public ICoppaService coppaService { get; set; }

		public override void Execute()
		{
			if (!coppaService.Restricted())
			{
				IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Load, "popup_Notification");
				iGUICommand.Args.Add(message);
				iGUICommand.Args.Add(autoClose);
				iGUICommand.skrimScreen = "NotificationsSkrim";
				iGUICommand.darkSkrim = true;
				guiService.Execute(iGUICommand);
			}
		}
	}
}
