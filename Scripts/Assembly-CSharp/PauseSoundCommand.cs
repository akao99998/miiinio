using Elevation.Logging;
using FMOD.Studio;
using Kampai.Util;
using strange.extensions.command.impl;

public class PauseSoundCommand : Command
{
	private IKampaiLogger logger = LogManager.GetClassLogger("PauseSoundCommand") as IKampaiLogger;

	[Inject]
	public bool isPaused { get; set; }

	public override void Execute()
	{
		Pause("bus:/Non-Diegetic/u_Music", isPaused);
		Pause("bus:/Diagetic/u_SFX", isPaused);
	}

	private void Pause(string busName, bool isPaused)
	{
		FMOD_StudioSystem instance = FMOD_StudioSystem.instance;
		Bus bus;
		instance.System.getBus(busName, out bus);
		if (bus != null)
		{
			bus.setPaused(isPaused);
		}
		else
		{
			logger.Log(KampaiLogLevel.Info, "Could not find bus: " + busName);
		}
	}
}
