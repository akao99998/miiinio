using Elevation.Logging;
using Kampai.Audio;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class MuteVolumeCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("MuteVolumeCommand") as IKampaiLogger;

		[Inject]
		public MuteMusicBusSignal muteMusicBusSignal { get; set; }

		public override void Execute()
		{
			CheckForOtherAudio();
		}

		private void MuteAudio(bool mute)
		{
			logger.Error("Music Muted: {0}", mute);
			muteMusicBusSignal.Dispatch(mute);
		}

		private void CheckForOtherAudio()
		{
			if (AudioSettingsModel.MuteIfBackgoundMusic && Native.IsUserMusicPlaying() && !AudioSettingsModel.MusicMuted)
			{
				muteMusicBusSignal.Dispatch(true);
			}
		}
	}
}
