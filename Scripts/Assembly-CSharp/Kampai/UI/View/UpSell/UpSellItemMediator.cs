using Elevation.Logging;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View.UpSell
{
	public class UpSellItemMediator : Mediator
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("UpSellItemMediator") as IKampaiLogger;

		[Inject]
		public UpSellItemView view { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IFancyUIService fancyUIService { get; set; }

		[Inject]
		public MoveAudioListenerSignal moveAudioListenerSignal { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			view.Init(localizationService, fancyUIService, definitionService, logger, moveAudioListenerSignal);
		}
	}
}
