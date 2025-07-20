using System.Collections.Generic;
using FMOD.Studio;
using Kampai.Common.Service.Audio;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class StartLoopingAudioCommand : Command
	{
		[Inject]
		public IFMODService fmodService { get; set; }

		public void Execute(CustomFMOD_StudioEventEmitter emitter, string eventName, Dictionary<string, float> parameters)
		{
			string guid = fmodService.GetGuid(eventName);
			if (emitter.path != null && emitter.path != guid)
			{
				if (emitter.evt != null)
				{
					emitter.evt.release();
					emitter.evt = null;
				}
				emitter.path = guid;
			}
			emitter.SetEventParameters(parameters);
			PLAYBACK_STATE playbackState = emitter.getPlaybackState();
			if (playbackState == PLAYBACK_STATE.STOPPING || playbackState == PLAYBACK_STATE.STOPPED)
			{
				emitter.StartEvent();
			}
			else
			{
				emitter.UpdateEventParameters();
			}
		}
	}
}
