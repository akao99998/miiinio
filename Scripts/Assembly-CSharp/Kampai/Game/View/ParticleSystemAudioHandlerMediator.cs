using System.Collections;
using Elevation.Logging;
using Kampai.Main;
using Kampai.Util;
using Kampai.Util.Audio;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.Game.View
{
	public class ParticleSystemAudioHandlerMediator : Mediator
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("ParticleSystemAudioHandlerMediator") as IKampaiLogger;

		[Inject]
		public ParticleSystemAudioHandlerView view { get; set; }

		[Inject]
		public PlayLocalAudioSignal playLocalAudioSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playGlobalAudioSignal { get; set; }

		public override void OnRegister()
		{
			PlayAudio();
		}

		private void PlayAudio()
		{
			if (view.system == null)
			{
				logger.Warning("No particle system found.  Audio will not play.");
			}
			else if (string.IsNullOrEmpty(view.audioEventName))
			{
				logger.Warning("No audio event specified.  Audio will not play.");
			}
			else if (view.syncStart)
			{
				StartCoroutine(PlayAudioWithDelay());
			}
			else
			{
				PlayAudio(view.audioEventName);
			}
		}

		private IEnumerator PlayAudioWithDelay()
		{
			yield return new WaitForSeconds(view.system.startDelay);
			PlayAudio(view.audioEventName);
		}

		private void PlayAudio(string eventName)
		{
			if (view.isLocal)
			{
				playLocalAudioSignal.Dispatch(GetAudioEmitter.Get(base.gameObject, "ParticleSystem"), eventName, null);
			}
			else
			{
				playGlobalAudioSignal.Dispatch(eventName);
			}
		}
	}
}
