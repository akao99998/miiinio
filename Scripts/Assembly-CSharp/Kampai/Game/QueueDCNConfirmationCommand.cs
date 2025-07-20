using Kampai.Common;
using Kampai.UI.View;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class QueueDCNConfirmationCommand : Command
	{
		[Inject]
		public Signal<bool> callback { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistanceService { get; set; }

		[Inject]
		public IDCNService dcnService { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		public override void Execute()
		{
			if (localPersistanceService.HasKey("DCNStoreDoNotShow"))
			{
				callback.Dispatch(true);
				telemetryService.Send_Telemetry_EVT_DCN("Yes", dcnService.GetLaunchURL(), dcnService.GetFeaturedContentId().ToString());
				return;
			}
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Queue, "popup_DCNconfirmation");
			iGUICommand.Args.Add(callback);
			iGUICommand.skrimScreen = "ConfirmationSkrim";
			iGUICommand.darkSkrim = true;
			guiService.Execute(iGUICommand);
		}
	}
}
