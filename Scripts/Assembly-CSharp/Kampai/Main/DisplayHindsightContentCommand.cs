using Elevation.Logging;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Main
{
	public class DisplayHindsightContentCommand : Command
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("DisplayHindsightContentCommand") as IKampaiLogger;

		[Inject]
		public HindsightCampaign.Scope scope { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public IHindsightService hindsightService { get; set; }

		public override void Execute()
		{
			HindsightCampaign cachedContent = hindsightService.GetCachedContent(scope);
			if (cachedContent == null)
			{
				logger.Info("There is no campaign to display right now");
				return;
			}
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Load, "HindsightContentView");
			iGUICommand.Args.Add(cachedContent);
			iGUICommand.darkSkrim = true;
			iGUICommand.skrimScreen = "HindsightContentSkrim";
			guiService.Execute(iGUICommand);
			cachedContent.ViewCount++;
		}
	}
}
