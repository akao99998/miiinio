using Kampai.Common;
using Kampai.Download.View;
using Kampai.Main;
using Kampai.Splash.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.Splash
{
	public class SplashContext : BaseContext
	{
		public SplashContext()
		{
		}

		public SplashContext(MonoBehaviour view, bool autoStartup)
			: base(view, autoStartup)
		{
		}

		protected override void MapBindings()
		{
			MapInjections();
			MapCommands();
			MapMediations();
		}

		private void MapInjections()
		{
			injectionBinder.Bind<ICrossContextCapable>().ToValue(this).ToName(SplashElement.CONTEXT)
				.CrossContext();
			injectionBinder.Bind<LaunchDownloadSignal>().ToSingleton().CrossContext();
			injectionBinder.Bind<ReconcileDLCSignal>().ToSingleton().CrossContext();
			injectionBinder.Bind<DownloadInitializeSignal>().ToSingleton();
			injectionBinder.Bind<DownloadProgressSignal>().ToSingleton();
			injectionBinder.Bind<DLCLoadScreenModel>().ToSingleton();
			injectionBinder.Bind<PlayGlobalSoundFXSignal>().ToSingleton();
			injectionBinder.Bind<ShowNoWiFiPanelSignal>().ToSingleton().CrossContext()
				.Weak();
		}

		private void MapCommands()
		{
			base.commandBinder.Bind<StartSignal>().To<SplashStartCommand>();
			base.commandBinder.Bind<HideSplashSignal>().To<HideSplashCommand>();
			base.commandBinder.Bind<AppPauseSignal>().To<DownloadPauseCommand>();
			base.commandBinder.Bind<AppResumeSignal>().To<DownloadResumeCommand>();
			base.commandBinder.Bind<LaunchDownloadSignal>().To<LaunchDownloadCommand>();
			base.commandBinder.Bind<DownloadResponseSignal>().To<DownloadResponseCommand>();
			base.commandBinder.Bind<ReconcileDLCSignal>().To<ReconcileDLCCommand>();
			base.commandBinder.Bind<DLCDownloadFinishedSignal>().To<DLCDownloadFinishedCommand>();
		}

		private void MapMediations()
		{
			base.mediationBinder.Bind<LoadInTipView>().To<LoadInTipMediator>();
			base.mediationBinder.Bind<LoadingBarView>().To<LoadingBarMediator>();
			base.mediationBinder.Bind<NoWiFiView>().To<NoWiFiMediator>();
			base.mediationBinder.Bind<LogoPanelView>().To<LogoPanelMediator>();
		}
	}
}
