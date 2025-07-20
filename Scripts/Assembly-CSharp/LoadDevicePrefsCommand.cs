using Elevation.Logging;
using Kampai.Game;
using Kampai.Util;
using strange.extensions.command.impl;

public class LoadDevicePrefsCommand : Command
{
	public IKampaiLogger logger = LogManager.GetClassLogger("LoadDevicePrefsCommand") as IKampaiLogger;

	[Inject]
	public IDevicePrefsService deviceService { get; set; }

	[Inject]
	public ILocalPersistanceService localPersistService { get; set; }

	public override void Execute()
	{
		string text = localPersistService.GetData("DevicePrefs");
		if (text.Length != 0)
		{
			deviceService.Deserialize(text);
		}
		else
		{
			logger.Log(KampaiLogLevel.Info, "Couldn't load device prefs. This could be because it's first time playing the game.");
		}
	}
}
