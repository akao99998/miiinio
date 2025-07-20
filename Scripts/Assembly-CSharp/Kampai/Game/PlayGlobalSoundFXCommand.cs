using Elevation.Logging;
using FMOD;
using FMOD.Studio;
using Kampai.Common.Service.Audio;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game
{
	public class PlayGlobalSoundFXCommand
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("PlayGlobalSoundFXCommand") as IKampaiLogger;

		[Inject]
		public IFMODService fmodService { get; set; }

		[Inject(MainElement.AUDIO_LISTENER)]
		public GameObject audioListener { get; set; }

		public void Execute(string audioSource)
		{
			FMOD_StudioSystem instance = FMOD_StudioSystem.instance;
			string guid = fmodService.GetGuid(audioSource);
			EventInstance @event = instance.GetEvent(guid);
			if (@event == null)
			{
				logger.Log(KampaiLogLevel.Error, "Failed to Load Audio Source: " + audioSource);
				return;
			}
			@event.set3DAttributes(UnityUtil.to3DAttributes(audioListener));
			RESULT rESULT = @event.start();
			if (!UnityUtil.ERRCHECK(rESULT))
			{
				logger.Log(KampaiLogLevel.Error, "Failed to Play (Error Code: " + rESULT.ToString() + ") Audio Source: " + audioSource);
			}
		}
	}
}
