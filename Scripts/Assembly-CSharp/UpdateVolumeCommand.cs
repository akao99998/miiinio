using Elevation.Logging;
using FMOD.Studio;
using Kampai.Audio;
using Kampai.Game;
using Kampai.Util;
using strange.extensions.command.impl;

public class UpdateVolumeCommand : Command
{
	public IKampaiLogger logger = LogManager.GetClassLogger("UpdateVolumeCommand") as IKampaiLogger;

	[Inject]
	public IDevicePrefsService prefs { get; set; }

	public override void Execute()
	{
		DevicePrefs devicePrefs = prefs.GetDevicePrefs();
		FMOD_StudioSystem instance = FMOD_StudioSystem.instance;
		Bus bus;
		instance.System.getBus("bus:/Non-Diegetic/u_Music", out bus);
		if (bus != null)
		{
			if (!AudioSettingsModel.MusicMuted)
			{
				bool mute = false;
				bus.getMute(out mute);
				if (mute)
				{
					bus.setMute(false);
				}
			}
			bus.setFaderLevel(devicePrefs.MusicVolume);
		}
		else
		{
			logger.Log(KampaiLogLevel.Info, "Could not find Music bus.");
		}
		Bus bus2;
		instance.System.getBus("bus:/Diagetic/u_SFX", out bus2);
		if (bus2 != null)
		{
			bus2.setFaderLevel(devicePrefs.SFXVolume);
		}
		else
		{
			logger.Log(KampaiLogLevel.Info, "Could not find SFX bus.");
		}
		Bus bus3;
		instance.System.getBus("bus:/Non-Diegetic/u_UI", out bus3);
		if (bus3 != null)
		{
			bus3.setFaderLevel(devicePrefs.SFXVolume);
		}
		else
		{
			logger.Log(KampaiLogLevel.Info, "Could not find UI bus.");
		}
	}
}
