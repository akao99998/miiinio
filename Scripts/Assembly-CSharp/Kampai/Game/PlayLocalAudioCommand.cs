using System.Collections.Generic;
using Elevation.Logging;
using FMOD.Studio;
using Kampai.Common.Service.Audio;
using Kampai.Util;

namespace Kampai.Game
{
	public class PlayLocalAudioCommand
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("PlayLocalAudioCommand") as IKampaiLogger;

		[Inject]
		public IFMODService fmodService { get; set; }

		public void Execute(CustomFMOD_StudioEventEmitter emitter, string audioEvent, Dictionary<string, float> eventParameters)
		{
			PLAYBACK_STATE state = PLAYBACK_STATE.STOPPED;
			if (emitter.evt != null)
			{
				emitter.evt.getPlaybackState(out state);
			}
			string guid = fmodService.GetGuid(audioEvent);
			if (string.IsNullOrEmpty(guid))
			{
				logger.Error("Failed to find event {0}", audioEvent);
			}
			else
			{
				if (state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STARTING)
				{
					return;
				}
				if (emitter.path != null && emitter.path != guid)
				{
					if (emitter.evt != null)
					{
						emitter.evt.release();
						emitter.evt = null;
					}
					emitter.path = guid;
				}
				emitter.SetEventParameters(eventParameters);
				if (emitter.path != null)
				{
					emitter.StartEvent();
				}
			}
		}
	}
}
