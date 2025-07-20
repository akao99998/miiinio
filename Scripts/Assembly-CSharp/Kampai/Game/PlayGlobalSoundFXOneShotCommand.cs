using Kampai.Main;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class PlayGlobalSoundFXOneShotCommand : Command
	{
		[Inject(MainElement.AUDIO_LISTENER)]
		public GameObject audioListener { get; set; }

		[Inject]
		public string audioClip { get; set; }

		public override void Execute()
		{
			FMOD_StudioSystem instance = FMOD_StudioSystem.instance;
			instance.PlayOneShot(audioClip, audioListener.transform.position);
		}
	}
}
