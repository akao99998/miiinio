using System.Collections;
using System.Collections.Generic;
using Kampai.Common.Service.Audio;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using strange.extensions.injector.api;

namespace Kampai.Game
{
	public class FadeBackgroundAudioCommand : Command
	{
		[Inject(ContextKeys.CONTEXT_VIEW)]
		public GameObject contextView { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public IFMODService fmodService { get; set; }

		[Inject]
		public bool fadeIn { get; set; }

		[Inject]
		public string snapshotName { get; set; }

		public override void Execute()
		{
			IInjectionBinding binding = gameContext.injectionBinder.GetBinding<CustomFMOD_StudioEventEmitter>(GameElement.FADE_AUDIO_EMITTER);
			if (binding == null)
			{
				GameObject gameObject = new GameObject("AudioFader");
				gameObject.transform.parent = contextView.transform;
				CustomFMOD_StudioEventEmitter customFMOD_StudioEventEmitter = gameObject.AddComponent<CustomFMOD_StudioEventEmitter>();
				gameContext.injectionBinder.Bind<CustomFMOD_StudioEventEmitter>().ToValue(customFMOD_StudioEventEmitter).ToName(GameElement.FADE_AUDIO_EMITTER);
				BeginFade(customFMOD_StudioEventEmitter);
			}
			else if (binding.value != null)
			{
				BeginFade((CustomFMOD_StudioEventEmitter)binding.value);
			}
		}

		private void BeginFade(CustomFMOD_StudioEventEmitter emitter)
		{
			if (fadeIn)
			{
				BeginFadeIn(emitter);
			}
			else
			{
				BeginFadeOut(emitter);
			}
		}

		private void BeginFadeOut(CustomFMOD_StudioEventEmitter emitter)
		{
			emitter.startEventOnAwake = false;
			emitter.SetEventParameters(new Dictionary<string, float>());
			emitter.path = fmodService.GetGuid(snapshotName);
			routineRunner.StartCoroutine(FadeOut(emitter));
		}

		private void BeginFadeIn(CustomFMOD_StudioEventEmitter emitter)
		{
			if (emitter != null && emitter.path != null)
			{
				emitter.Stop();
			}
		}

		private IEnumerator FadeOut(CustomFMOD_StudioEventEmitter emitter)
		{
			yield return new WaitForEndOfFrame();
			if (emitter != null && emitter.path != null)
			{
				emitter.StartEvent();
			}
		}
	}
}
