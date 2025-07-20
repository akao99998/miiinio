using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.UI.Controller
{
	public class DisplayMasterPlanSelectComponentSimpleCommand : Command
	{
		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public CloseAllMessageDialogs closeAllDialogsSignal { get; set; }

		public override void Execute()
		{
			closeAllDialogsSignal.Dispatch();
			OpenModal();
		}

		private void OpenModal()
		{
			IGUICommand command = guiService.BuildCommand(GUIOperation.Queue, "screen_MasterplanSelectComponent");
			guiService.Execute(command);
		}
	}
}
