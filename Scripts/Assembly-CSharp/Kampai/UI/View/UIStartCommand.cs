using Elevation.Logging;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.UI.View
{
	public class UIStartCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("UIStartCommand") as IKampaiLogger;

		[Inject(ContextKeys.CONTEXT_VIEW)]
		public GameObject contextView { get; set; }

		[Inject]
		public SetupCanvasSignal canvasSignal { get; set; }

		[Inject(MainElement.CAMERA)]
		public Camera mainCamera { get; set; }

		[Inject(UIElement.CAMERA)]
		public Camera uiCamera { get; set; }

		[Inject]
		public LoadGUISignal loadGUISignal { get; set; }

		[Inject]
		public LoadUICompleteSignal loadUICompleteSignal { get; set; }

		[Inject]
		public SetupDeepLinkSignal setupDeepLinkSignal { get; set; }

		[Inject]
		public IRewardedAdService rewardedAdService { get; set; }

		public override void Execute()
		{
			logger.EventStart("UIStartCommand.Execute");
			TimeProfiler.StartSection("ui");
			TimeProfiler.StartSection("canvas");
			canvasSignal.Dispatch(Tuple.Create("GlassCanvas", MainElement.UI_GLASSCANVAS, uiCamera));
			canvasSignal.Dispatch(Tuple.Create("WorldCanvas", MainElement.UI_WORLDCANVAS, mainCamera));
			TimeProfiler.EndSection("canvas");
			TimeProfiler.StartSection("gui");
			loadGUISignal.Dispatch();
			TimeProfiler.EndSection("gui");
			TimeProfiler.StartSection("complete");
			loadUICompleteSignal.Dispatch(contextView);
			TimeProfiler.EndSection("complete");
			setupDeepLinkSignal.Dispatch();
			rewardedAdService.Initialize();
			TimeProfiler.EndSection("ui");
			logger.EventStop("UIStartCommand.Execute");
		}
	}
}
