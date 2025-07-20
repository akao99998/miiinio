using Elevation.Logging;
using Kampai.Common;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;

namespace Kampai.UI.View
{
	public class HindsightContentMediator : UIStackMediator<HindsightContentView>
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("HindsightContentMediator") as IKampaiLogger;

		[Inject(MainElement.UI_GLASSCANVAS)]
		public GameObject glassCanvas { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public HindsightContentDismissSignal dismissSignal { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrimSignal { get; set; }

		public override void Initialize(GUIArguments args)
		{
			HindsightCampaign hindsightCampaign = args.Get<HindsightCampaign>();
			if (hindsightCampaign == null)
			{
				logger.Error("Campaign is null");
				return;
			}
			base.view.definition = hindsightCampaign.Definition;
			base.view.dismissSignal = dismissSignal;
			base.view.hideSkrimSignal = hideSkrimSignal;
			base.view.guiService = guiService;
			base.view.telemetryService = telemetryService;
			base.view.Open(glassCanvas, hindsightCampaign, localizationService.GetLanguageKey());
		}

		protected override void Close()
		{
			base.view.Close(HindsightCampaign.DismissType.DECLINED);
		}
	}
}
