using Elevation.Logging;
using Kampai.Game;
using Kampai.Util;
using strange.extensions.context.api;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class DiscoGlobePanelMediator : Mediator
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("DiscoGlobePanelMediator") as IKampaiLogger;

		[Inject]
		public DiscoGlobePanelView view { get; set; }

		[Inject]
		public DisplayDiscoGlobeSignal displayDiscoGlobeSignal { get; set; }

		[Inject]
		public KillDiscoGlobeSignal killDiscoGlobeSignal { get; set; }

		public PreLoadPartyAssetsSignal preLoadPartyAssetsSignal { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public IPlayerService player { get; set; }

		public override void OnRegister()
		{
			preLoadPartyAssetsSignal = gameContext.injectionBinder.GetInstance<PreLoadPartyAssetsSignal>();
			displayDiscoGlobeSignal.AddListener(DisplayDiscoGlobe);
			preLoadPartyAssetsSignal.AddListener(PreloadDiscoGlobe);
			killDiscoGlobeSignal.AddListener(KillDiscoGlobe);
		}

		public override void OnRemove()
		{
			displayDiscoGlobeSignal.RemoveListener(DisplayDiscoGlobe);
			preLoadPartyAssetsSignal.RemoveListener(PreloadDiscoGlobe);
			killDiscoGlobeSignal.RemoveListener(KillDiscoGlobe);
		}

		private void KillDiscoGlobe()
		{
			view.DestroyDiscoGlobeView();
		}

		private void PreloadDiscoGlobe()
		{
			view.PreLoadDiscoGlobe();
		}

		private void DisplayDiscoGlobe(bool display)
		{
			logger.Debug("Display disco globe: " + display);
			view.DisplayDiscoGlobe(display, player.GetMinionPartyInstance());
		}
	}
}
