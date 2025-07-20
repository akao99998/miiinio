using Elevation.Logging;
using FMOD.Studio;
using Kampai.Audio;
using Kampai.Util;
using strange.extensions.command.impl;

public class MuteMusicBusCommand : Command
{
	public IKampaiLogger logger = LogManager.GetClassLogger("MuteMusicBusCommand") as IKampaiLogger;

	[Inject]
	public bool mute { get; set; }

	public override void Execute()
	{
		AudioSettingsModel.MusicMuted = mute;
		Bus bus;
		FMOD_StudioSystem.instance.System.getBus("bus:/Non-Diegetic/u_Music", out bus);
		if (bus != null)
		{
			bus.setMute(mute);
		}
		else
		{
			logger.Log(KampaiLogLevel.Error, "Could not find Music bus.");
		}
	}
}
