using System.Collections;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class FadeAudioAction : KampaiAction
	{
		private ActionableObject obj;

		private float duration;

		private bool fadeIn;

		private CustomFMOD_StudioEventEmitter[] emitters;

		public FadeAudioAction(ActionableObject obj, float duration, bool fadeIn, IKampaiLogger logger)
			: base(logger)
		{
			this.obj = obj;
			this.duration = duration;
			this.fadeIn = fadeIn;
		}

		public override void Execute()
		{
			if (!obj.gameObject.activeSelf)
			{
				Abort();
				return;
			}
			emitters = obj.gameObject.GetComponentsInChildren<CustomFMOD_StudioEventEmitter>();
			if (emitters.Length > 0)
			{
				FadeAudio(fadeIn, duration);
			}
			obj.StartCoroutine(Delay(duration));
		}

		public override void Abort()
		{
			if (base.Done)
			{
				return;
			}
			if (emitters != null)
			{
				CustomFMOD_StudioEventEmitter[] array = emitters;
				foreach (CustomFMOD_StudioEventEmitter customFMOD_StudioEventEmitter in array)
				{
					customFMOD_StudioEventEmitter.Fade(0f, 1f, 0.1f);
				}
			}
			base.Done = true;
		}

		private IEnumerator Delay(float t)
		{
			yield return new WaitForSeconds(t);
			base.Done = true;
		}

		private void FadeAudio(bool fadeIn, float duration)
		{
			float startVol = 1f;
			float endVol = 0f;
			if (fadeIn)
			{
				startVol = 0f;
				endVol = 1f;
			}
			CustomFMOD_StudioEventEmitter[] array = emitters;
			foreach (CustomFMOD_StudioEventEmitter customFMOD_StudioEventEmitter in array)
			{
				if (customFMOD_StudioEventEmitter.id == "LocalAudio")
				{
					customFMOD_StudioEventEmitter.Fade(startVol, endVol, duration);
				}
			}
		}
	}
}
