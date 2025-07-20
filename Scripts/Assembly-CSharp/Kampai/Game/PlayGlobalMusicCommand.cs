using System.Collections;
using System.Collections.Generic;
using Kampai.Common.Service.Audio;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class PlayGlobalMusicCommand : Command
	{
		[Inject]
		public string audioSource { get; set; }

		[Inject]
		public Dictionary<string, float> eventParameters { get; set; }

		[Inject(MainElement.AUDIO_LISTENER)]
		public GameObject audioListener { get; set; }

		[Inject]
		public IFMODService fmodService { get; set; }

		[Inject]
		public IRoutineRunner runner { get; set; }

		public override void Execute()
		{
			CustomFMOD_StudioEventEmitter customFMOD_StudioEventEmitter = audioListener.GetComponent<CustomFMOD_StudioEventEmitter>();
			if (customFMOD_StudioEventEmitter == null)
			{
				customFMOD_StudioEventEmitter = audioListener.AddComponent<CustomFMOD_StudioEventEmitter>();
				if (customFMOD_StudioEventEmitter == null)
				{
					return;
				}
				customFMOD_StudioEventEmitter.startEventOnAwake = false;
			}
			bool flag = false;
			if (customFMOD_StudioEventEmitter.path != null && customFMOD_StudioEventEmitter.path != audioSource)
			{
				string guid = fmodService.GetGuid(audioSource);
				if (guid != customFMOD_StudioEventEmitter.path)
				{
					flag = true;
					customFMOD_StudioEventEmitter.Stop();
					if (customFMOD_StudioEventEmitter.evt != null)
					{
						customFMOD_StudioEventEmitter.evt.release();
						customFMOD_StudioEventEmitter.evt = null;
					}
					customFMOD_StudioEventEmitter.path = fmodService.GetGuid(audioSource);
				}
			}
			customFMOD_StudioEventEmitter.SetEventParameters(eventParameters);
			if (flag)
			{
				runner.StartCoroutine(WaitAFrame(customFMOD_StudioEventEmitter));
			}
			else
			{
				customFMOD_StudioEventEmitter.UpdateEventParameters();
			}
		}

		private IEnumerator WaitAFrame(CustomFMOD_StudioEventEmitter emitter)
		{
			yield return new WaitForEndOfFrame();
			if (emitter.path != null)
			{
				emitter.StartEvent();
			}
		}
	}
}
