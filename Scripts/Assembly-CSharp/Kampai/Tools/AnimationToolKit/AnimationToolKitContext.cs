using System.Collections.Generic;
using Kampai.Common;
using Kampai.Common.Service.Audio;
using Kampai.Game;
using Kampai.Game.Controller.Audio;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.Splash;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.api;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using strange.extensions.context.impl;
using strange.extensions.injector.api;

namespace Kampai.Tools.AnimationToolKit
{
	public class AnimationToolKitContext : MVCSContext
	{
		public AnimationToolKitContext()
		{
		}

		public AnimationToolKitContext(MonoBehaviour view, bool autoStartup)
			: base(view, autoStartup)
		{
		}

		public override void Launch()
		{
			base.Launch();
			injectionBinder.GetInstance<StartSignal>().Dispatch();
		}

		protected override void addCoreComponents()
		{
			base.addCoreComponents();
			injectionBinder.Unbind<ICommandBinder>();
			injectionBinder.Bind<ICommandBinder>().To<SignalCommandBinder>().ToSingleton();
		}

		protected override void mapBindings()
		{
			base.mapBindings();
			injectionBinder.Bind<ICrossContextCapable>().ToValue(this).ToName(BaseElement.CONTEXT);
			injectionBinder.Bind<ICrossContextCapable>().ToValue(this).ToName(AnimationToolKitElement.CONTEXT);
			mapGameBindings();
			mapAnimationToolKitBindings();
		}

		private void mapGameBindings()
		{
			ICrossContextInjectionBinder crossContextInjectionBinder = injectionBinder;
			crossContextInjectionBinder.Bind<ILocalizationService>().To<HALService>();
			crossContextInjectionBinder.Bind<IDefinitionService>().To<DefinitionService>().ToSingleton();
			crossContextInjectionBinder.Bind<IRoutineRunner>().To<RoutineRunner>().ToSingleton();
			crossContextInjectionBinder.Bind<IInvokerService>().To<InvokerService>().ToSingleton();
			crossContextInjectionBinder.Bind<IMinionBuilder>().To<MinionBuilder>().ToSingleton();
			crossContextInjectionBinder.Bind<INamedCharacterBuilder>().To<NamedCharacterBuilder>().ToSingleton();
			crossContextInjectionBinder.Bind<IPlayerService>().To<AnimationToolKitPlayerService>().ToSingleton();
			crossContextInjectionBinder.Bind<IManifestService>().To<ManifestService>().ToSingleton();
			crossContextInjectionBinder.Bind<IAssetBundlesService>().To<AssetBundlesService>().ToSingleton();
			crossContextInjectionBinder.Bind<ILocalContentService>().To<LocalContentService>().ToSingleton();
			crossContextInjectionBinder.Bind<MinionIdleNotifier>().To<AnimationToolKitMinionManagerView>().ToSingleton();
			crossContextInjectionBinder.Bind<ILocalPersistanceService>().To<LocalPersistanceService>().ToSingleton();
			crossContextInjectionBinder.Bind<ICoroutineProgressMonitor>().To<CoroutineProgressMonitor>().ToSingleton();
			crossContextInjectionBinder.Bind<IUpdateRunner>().To<UpdateRunner>().ToSingleton();
			crossContextInjectionBinder.Bind<IEncryptionService>().To<EncryptionService>().ToSingleton();
			crossContextInjectionBinder.Bind<IDownloadService>().To<AnimationToolKitDownloadService>().ToSingleton();
			crossContextInjectionBinder.Bind<IDLCService>().To<AnimationToolKitDLCService>().ToSingleton();
			crossContextInjectionBinder.Bind<ITelemetryService>().To<TelemetryService>().ToSingleton();
			crossContextInjectionBinder.Bind<ICoppaService>().To<CoppaService>().ToSingleton();
			crossContextInjectionBinder.Bind<IRandomService>().ToValue(new RandomService(0L));
			crossContextInjectionBinder.Bind<ITimeService>().To<TimeService>().ToSingleton();
			injectionBinder.Bind<string>().ToValue(GameConstants.Server.SERVER_ENVIRONMENT).ToName("game.server.environment");
			crossContextInjectionBinder.Bind<IFMODService>().To<FMODService>().ToSingleton();
			crossContextInjectionBinder.Bind<PlayGlobalSoundFXSignal>().ToSingleton();
			crossContextInjectionBinder.Bind<PlayGlobalSoundFXOneShotSignal>().ToSingleton();
			crossContextInjectionBinder.Bind<SplashProgressUpdateSignal>().ToSingleton();
			crossContextInjectionBinder.Bind<LogToScreenSignal>().ToSingleton();
			crossContextInjectionBinder.Bind<DefinitionsChangedSignal>().ToSingleton();
			crossContextInjectionBinder.Bind<SetMinionCountSignal>().ToSingleton();
			crossContextInjectionBinder.Bind<PlayMinionAnimationSignal>().ToSingleton();
			crossContextInjectionBinder.Bind<EnableInterfaceSignal>().ToSingleton();
			crossContextInjectionBinder.Bind<MainCompleteSignal>().ToSingleton();
			crossContextInjectionBinder.Bind<LoginUserSignal>().ToSingleton();
			crossContextInjectionBinder.Bind<MinionCreatedSignal>().ToSingleton();
			crossContextInjectionBinder.Bind<VillainCreatedSignal>().ToSingleton();
			crossContextInjectionBinder.Bind<CharacterCreatedSignal>().ToSingleton();
			crossContextInjectionBinder.Bind<TeleportCharacterToTikiBarSignal>().ToSingleton();
			crossContextInjectionBinder.Bind<MinionPartyAnimationSignal>().ToSingleton();
			crossContextInjectionBinder.Bind<PlayLocalAudioSignal>().ToSingleton();
			crossContextInjectionBinder.Bind<StopLocalAudioSignal>().ToSingleton();
			crossContextInjectionBinder.Bind<PlayLocalAudioOneShotSignal>().ToSingleton();
			crossContextInjectionBinder.Bind<PlayMinionStateAudioSignal>().ToSingleton();
			crossContextInjectionBinder.Bind<QueueLocalAudioCommandSignal>().ToSingleton();
			crossContextInjectionBinder.Bind<StartLoopingAudioSignal>().ToSingleton();
			crossContextInjectionBinder.Bind<PlayLocalAudioCommand>().ToSingleton();
			crossContextInjectionBinder.Bind<StopLocalAudioCommand>().ToSingleton();
			crossContextInjectionBinder.Bind<PlayLocalAudioOneShotCommand>().ToSingleton();
			crossContextInjectionBinder.Bind<PlayMinionStateAudioCommand>().ToSingleton();
			crossContextInjectionBinder.Bind<QueueLocalAudioCommand>().ToSingleton();
			crossContextInjectionBinder.Bind<StartLoopingAudioCommand>().ToSingleton();
			crossContextInjectionBinder.Bind<QuestScriptKernel>().ToSingleton();
			base.commandBinder.Bind<LoadDefinitionsSignal>().To<LoadDefinitionsCommand>();
			base.commandBinder.Bind<SetupManifestSignal>().To<LoadAnimationToolKitManifestCommand>();
			base.commandBinder.Bind<ReloadGameSignal>().To<ReloadGameCommand>();
			PlayLocalAudioCommand playLocalAudioCommand = injectionBinder.GetInstance<PlayLocalAudioCommand>();
			PlayLocalAudioSignal instance = injectionBinder.GetInstance<PlayLocalAudioSignal>();
			instance.AddListener(delegate(CustomFMOD_StudioEventEmitter emitter, string name, Dictionary<string, float> evtParams)
			{
				playLocalAudioCommand.Execute(emitter, name, evtParams);
			});
			StopLocalAudioCommand stopLocalAudioCommand = injectionBinder.GetInstance<StopLocalAudioCommand>();
			StopLocalAudioSignal instance2 = injectionBinder.GetInstance<StopLocalAudioSignal>();
			instance2.AddListener(delegate(CustomFMOD_StudioEventEmitter emitter)
			{
				stopLocalAudioCommand.Execute(emitter);
			});
			PlayMinionStateAudioCommand playMinionStateAudioCommand = injectionBinder.GetInstance<PlayMinionStateAudioCommand>();
			PlayMinionStateAudioSignal instance3 = injectionBinder.GetInstance<PlayMinionStateAudioSignal>();
			instance3.AddListener(delegate(MinionStateAudioArgs args)
			{
				playMinionStateAudioCommand.Execute(args);
			});
			PlayLocalAudioOneShotCommand playLocalAudioOneShotCommand = injectionBinder.GetInstance<PlayLocalAudioOneShotCommand>();
			PlayLocalAudioOneShotSignal instance4 = injectionBinder.GetInstance<PlayLocalAudioOneShotSignal>();
			instance4.AddListener(delegate(CustomFMOD_StudioEventEmitter emitter, string audioClip)
			{
				playLocalAudioOneShotCommand.Execute(emitter, audioClip);
			});
			QueueLocalAudioCommandSignal instance5 = injectionBinder.GetInstance<QueueLocalAudioCommandSignal>();
			QueueLocalAudioCommand queueLocalAudioCommand = injectionBinder.GetInstance<QueueLocalAudioCommand>();
			instance5.AddListener(delegate(CustomFMOD_StudioEventEmitter emitter, string audioClip)
			{
				queueLocalAudioCommand.Execute(emitter, audioClip);
			});
			StartLoopingAudioCommand startLoopingAudioCommand = injectionBinder.GetInstance<StartLoopingAudioCommand>();
			StartLoopingAudioSignal instance6 = injectionBinder.GetInstance<StartLoopingAudioSignal>();
			instance6.AddListener(delegate(CustomFMOD_StudioEventEmitter emitter, string name, Dictionary<string, float> evtParams)
			{
				startLoopingAudioCommand.Execute(emitter, name, evtParams);
			});
		}

		private void mapAnimationToolKitBindings()
		{
			ICrossContextInjectionBinder crossContextInjectionBinder = injectionBinder;
			crossContextInjectionBinder.Bind<AnimationToolKit>().ToSingleton();
			crossContextInjectionBinder.Bind<InitToggleSignal>().ToSingleton();
			crossContextInjectionBinder.Bind<AnimationToolkitModel>().ToSingleton();
			base.commandBinder.Bind<StartSignal>().To<LoadAnimationToolKitCommand>();
			base.commandBinder.Bind<LoadCameraSignal>().To<LoadCameraCommand>();
			base.commandBinder.Bind<LoadCanvasSignal>().To<LoadCanvasCommand>();
			base.commandBinder.Bind<LoadEventSystemSignal>().To<LoadEventSystemCommand>();
			base.commandBinder.Bind<LoadInterfaceSignal>().To<LoadInterfaceCommand>();
			base.commandBinder.Bind<LoadToggleGroupSignal>().To<LoadToggleGroupCommand>();
			base.commandBinder.Bind<LoadToggleSignal>().To<LoadToggleCommand>();
			base.commandBinder.Bind<GenerateMinionSignal>().To<GenerateMinionCommand>();
			base.commandBinder.Bind<GenerateVillainSignal>().To<GenerateVillainCommand>();
			base.commandBinder.Bind<GenerateCharacterSignal>().To<GenerateCharacterCommand>();
			base.commandBinder.Bind<AddMinionSignal>().To<AddMinionCommand>();
			base.commandBinder.Bind<AddCharacterSignal>().To<AddCharacterCommand>();
			base.commandBinder.Bind<RemoveMinionSignal>().To<RemoveMinionCommand>();
			base.commandBinder.Bind<RemoveCharacterSignal>().To<RemoveCharacterCommand>();
			base.commandBinder.Bind<ToggleInterfaceSignal>().To<ToggleInterfaceCommand>();
			base.commandBinder.Bind<ToggleMeshSignal>().To<ToggleMeshCommand>();
			base.mediationBinder.Bind<ToggleView>().To<ToggleMediator>();
			base.mediationBinder.Bind<AnimationToolKitButtonView>().To<AnimationToolKitButtonMediator>();
			base.mediationBinder.Bind<GachaButtonPanelView>().To<GachaButtonPanelMediator>();
		}
	}
}
