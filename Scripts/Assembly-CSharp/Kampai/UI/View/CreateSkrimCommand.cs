using Elevation.Logging;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class CreateSkrimCommand : Command
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("CreateSkrimCommand") as IKampaiLogger;

		[Inject]
		public string Name { get; set; }

		[Inject]
		public Signal<KampaiDisposable> Callback { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		public override void Execute()
		{
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Queue, "Skrim", Name);
			GUIArguments args = new GUIArguments(logger).Add(new SkrimCallback(Callback));
			iGUICommand.Args = args;
			guiService.Execute(iGUICommand);
		}
	}
}
