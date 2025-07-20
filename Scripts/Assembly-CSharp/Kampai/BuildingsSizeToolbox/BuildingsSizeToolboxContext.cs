using System.IO;
using Kampai.Common;
using Kampai.Common.Service.Audio;
using Kampai.Game;
using Kampai.Main;
using Kampai.Splash;
using Kampai.Tools.AnimationToolKit;
using Kampai.UI;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.api;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using strange.extensions.context.impl;
using strange.extensions.injector.api;

namespace Kampai.BuildingsSizeToolbox
{
	public class BuildingsSizeToolboxContext : MVCSContext
	{
		public BuildingsSizeToolboxContext()
		{
		}

		public BuildingsSizeToolboxContext(MonoBehaviour view, bool autoStartup)
			: base(view, autoStartup)
		{
		}

		protected override void addCoreComponents()
		{
			base.addCoreComponents();
			injectionBinder.Unbind<ICommandBinder>();
			injectionBinder.Bind<ICommandBinder>().To<SignalCommandBinder>().ToSingleton();
		}

		public override void Launch()
		{
			base.Launch();
			injectionBinder.GetInstance<ILocalizationService>().Initialize("EN-US");
			TextAsset textAsset = Resources.Load("dev_definitions") as TextAsset;
			using (StringReader textReader = new StringReader(textAsset.text))
			{
				injectionBinder.GetInstance<IDefinitionService>().DeserializeJson(textReader);
			}
		}

		protected override void mapBindings()
		{
			ICrossContextInjectionBinder crossContextInjectionBinder = injectionBinder;
			crossContextInjectionBinder.Bind<ICrossContextCapable>().ToValue(this).ToName(BaseElement.CONTEXT);
			crossContextInjectionBinder.Bind<ILocalizationService>().To<HALService>().ToSingleton();
			crossContextInjectionBinder.Bind<ILocalPersistanceService>().To<LocalPersistanceService>();
			crossContextInjectionBinder.Bind<IDefinitionService>().To<DefinitionService>().ToSingleton();
			crossContextInjectionBinder.Bind<string>().ToValue(GameConstants.Server.SERVER_ENVIRONMENT).ToName("game.server.environment");
			crossContextInjectionBinder.Bind<IPlayerService>().To<AnimationToolKitPlayerService>().ToSingleton();
			crossContextInjectionBinder.Bind<IFancyUIService>().To<FancyUIService>().ToSingleton();
			crossContextInjectionBinder.Bind<IGhostComponentService>().To<GhostComponentService>().ToSingleton();
			crossContextInjectionBinder.Bind<IDLCService>().To<AnimationToolKitDLCService>().ToSingleton();
			crossContextInjectionBinder.Bind<IDownloadService>().To<AnimationToolKitDownloadService>().ToSingleton();
			crossContextInjectionBinder.Bind<IMinionBuilder>().To<MinionBuilder>().ToSingleton();
			crossContextInjectionBinder.Bind<IRandomService>().ToValue(new RandomService(0L));
			crossContextInjectionBinder.Bind<IPrestigeService>().To<BuildingsSizeToolboxPrestigeService>().ToSingleton();
			crossContextInjectionBinder.Bind<IDummyCharacterBuilder>().To<DummyCharacterBuilder>().ToSingleton();
			crossContextInjectionBinder.Bind<IFMODService>().To<BuildingsSizeToolboxFMODService>().ToSingleton();
			bindSignals(crossContextInjectionBinder);
		}

		private void bindSignals(ICrossContextInjectionBinder localInjectionBinder)
		{
			localInjectionBinder.Bind<LogToScreenSignal>().ToSingleton();
			localInjectionBinder.Bind<PlayGlobalSoundFXSignal>().ToSingleton();
			localInjectionBinder.Bind<NewUpsellScreenSelectedSignal>().ToSingleton();
			localInjectionBinder.Bind<PlayLocalAudioSignal>().ToSingleton();
			localInjectionBinder.Bind<StartLoopingAudioSignal>().ToSingleton();
			localInjectionBinder.Bind<StopLocalAudioSignal>().ToSingleton();
			localInjectionBinder.Bind<PlayMinionStateAudioSignal>().ToSingleton();
			localInjectionBinder.Bind<MoveAudioListenerSignal>().ToSingleton();
			localInjectionBinder.Bind<BuildingSelectedSignal>().ToSingleton();
			localInjectionBinder.Bind<BuildingModifiedSignal>().ToSingleton();
			localInjectionBinder.Bind<BuildingsStateSavedSignal>().ToSingleton();
		}
	}
}
