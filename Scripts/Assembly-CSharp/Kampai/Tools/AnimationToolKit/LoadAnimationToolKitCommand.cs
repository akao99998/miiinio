using Elevation.Logging;
using Kampai.Common;
using Kampai.Common.Service.Audio;
using Kampai.Game;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Tools.AnimationToolKit
{
	public class LoadAnimationToolKitCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("LoadAnimationToolKitCommand") as IKampaiLogger;

		[Inject(ContextKeys.CONTEXT_VIEW)]
		public GameObject ContextView { get; set; }

		[Inject]
		public IFMODService fmodService { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public LoadDefinitionsSignal LoadDefinitionsSignal { get; set; }

		[Inject]
		public LoadCameraSignal LoadCameraSignal { get; set; }

		[Inject]
		public LoadCanvasSignal LoadCanvasSignal { get; set; }

		[Inject]
		public LoadEventSystemSignal LoadEventSystemSignal { get; set; }

		[Inject]
		public SetupManifestSignal setupManifestSignal { get; set; }

		public override void Execute()
		{
			DeviceCapabilities.Initialize();
			LoadDevDefinitions();
			LoadCameraSignal.Dispatch();
			LoadCanvasSignal.Dispatch();
			LoadEventSystemSignal.Dispatch();
			setupManifestSignal.Dispatch();
			LoadFMOD();
			LoadMinionGroup();
			LoadVillainGroup();
			LoadCharacterGroup();
			base.injectionBinder.GetInstance<AnimationToolKit>();
		}

		private void LoadDevDefinitions()
		{
			TextAsset textAsset = Resources.Load("dev_definitions") as TextAsset;
			if (textAsset != null && textAsset.text != null)
			{
				LoadDefinitionsCommand.LoadDefinitionsData loadDefinitionsData = new LoadDefinitionsCommand.LoadDefinitionsData();
				loadDefinitionsData.Json = textAsset.text;
				LoadDefinitionsSignal.Dispatch(false, loadDefinitionsData);
			}
			else
			{
				logger.Debug("Unable to load dev_definitions.json from Resources.");
			}
		}

		private void LoadFMOD()
		{
			routineRunner.StartCoroutine(fmodService.InitializeSystem());
			Camera main = Camera.main;
			main.gameObject.AddComponent<FMOD_Listener>();
			GameObject gameObject = GameObject.Find("FMOD_StudioSystem");
			gameObject.transform.parent = ContextView.transform;
			GameObject gameObject2 = new GameObject("EnvironmentAudioManager");
			EnvironmentAudioManagerView environmentAudioManagerView = gameObject2.AddComponent<EnvironmentAudioManagerView>();
			environmentAudioManagerView.mainCamera = Camera.main;
			gameObject2.transform.localPosition = Vector3.zero;
			gameObject2.transform.parent = main.transform;
		}

		private void LoadMinionGroup()
		{
			GameObject gameObject = new GameObject("Minions");
			gameObject.transform.parent = ContextView.transform;
			base.injectionBinder.Bind<GameObject>().ToValue(gameObject).ToName(AnimationToolKitElement.MINIONS);
		}

		private void LoadVillainGroup()
		{
			GameObject gameObject = new GameObject("Villains");
			gameObject.transform.parent = ContextView.transform;
			base.injectionBinder.Bind<GameObject>().ToValue(gameObject).ToName(AnimationToolKitElement.VILLAINS);
		}

		private void LoadCharacterGroup()
		{
			GameObject gameObject = new GameObject("Characters");
			gameObject.transform.parent = ContextView.transform;
			base.injectionBinder.Bind<GameObject>().ToValue(gameObject).ToName(AnimationToolKitElement.CHARACTERS);
		}
	}
}
