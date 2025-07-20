using System.Collections;
using Kampai.Common.Service.Audio;
using Kampai.Main;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.Game.View
{
	public class EnvironmentAudioEmitterMediator : Mediator
	{
		internal CustomFMOD_StudioEventEmitter emitter;

		[Inject]
		public EnvironmentAudioEmitterView View { get; set; }

		[Inject(MainElement.CAMERA)]
		public Camera MainCamera { get; set; }

		[Inject]
		public IFMODService FMODService { get; set; }

		[Inject]
		public GameLoadedModel gameLoadedModel { get; set; }

		public override void OnRegister()
		{
			View.OnTargetVisible.AddListener(OnTargetVisible);
			View.Init(MainCamera);
			StartCoroutine(WaitAFrame());
		}

		private IEnumerator WaitAFrame()
		{
			yield return null;
			while (!gameLoadedModel.gameLoaded)
			{
				yield return null;
			}
			emitter = base.gameObject.AddComponent<CustomFMOD_StudioEventEmitter>();
			emitter.shiftPosition = false;
			emitter.staticSound = false;
			emitter.path = FMODService.GetGuid(View.AudioName);
			emitter.Play();
		}

		public override void OnRemove()
		{
			View.OnTargetVisible.RemoveListener(OnTargetVisible);
		}

		internal virtual void OnTargetVisible(bool visible)
		{
			if (!(emitter == null))
			{
				if (visible)
				{
					emitter.Fade(0f, 1f, View.FadeDuration);
				}
				else
				{
					emitter.Fade(1f, 0f, View.FadeDuration);
				}
			}
		}
	}
}
