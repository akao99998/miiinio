using Elevation.Logging;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class LoadGUICommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("LoadGUICommand") as IKampaiLogger;

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public CreateFunMeterSignal createXPBar { get; set; }

		[Inject]
		public CreatePartyMeterSignal createPartyMeter { get; set; }

		public override void Execute()
		{
			logger.EventStart("LoadGUICommand.Execute");
			GameObject o = guiService.Execute(GUIOperation.LoadStatic, "screen_HUD");
			base.injectionBinder.Bind<GameObject>().ToValue(o).ToName(UIElement.HUD);
			createXPBar.Dispatch();
			createPartyMeter.Dispatch();
			logger.EventStart("LoadGUICommand.Execute");
		}
	}
}
