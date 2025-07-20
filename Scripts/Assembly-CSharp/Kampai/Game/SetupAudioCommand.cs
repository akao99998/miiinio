using Elevation.Logging;
using Kampai.Game.View;
using Kampai.Game.View.Audio;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class SetupAudioCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("SetupAudioCommand") as IKampaiLogger;

		[Inject(ContextKeys.CONTEXT_VIEW)]
		public GameObject contextView { get; set; }

		[Inject(MainElement.MANAGER_PARENT)]
		public GameObject managers { get; set; }

		[Inject(MainElement.CAMERA)]
		public Camera camera { get; set; }

		public override void Execute()
		{
			logger.EventStart("SetupAudioCommand.Execute");
			GameObject gameObject = new GameObject("PositionalAudioListener");
			gameObject.AddComponent<PositionalAudioListenerView>();
			base.injectionBinder.Bind<GameObject>().ToValue(gameObject).ToName(MainElement.AUDIO_LISTENER)
				.CrossContext();
			gameObject.transform.parent = contextView.transform;
			gameObject.gameObject.AddComponent<FMOD_Listener>();
			GameObject gameObject2 = GameObject.Find("FMOD_StudioSystem");
			gameObject2.transform.parent = managers.transform;
			GameObject gameObject3 = new GameObject("EnvironmentAudioManager");
			gameObject3.AddComponent<EnvironmentAudioManagerView>();
			gameObject3.transform.localPosition = new Vector3(0f, 0f, 0f);
			gameObject3.transform.parent = camera.transform;
			GameObject gameObject4 = new GameObject("VolcanoEmitter");
			gameObject4.transform.SetParent(camera.transform.parent, false);
			gameObject4.transform.position = new Vector3(150f, 0f, 205f);
			EnvironmentAudioEmitterView environmentAudioEmitterView = gameObject4.AddComponent<EnvironmentAudioEmitterView>();
			environmentAudioEmitterView.AudioName = "Play_volcano_lava_01";
			logger.EventStop("SetupAudioCommand.Execute");
		}
	}
}
