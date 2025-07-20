using Elevation.Logging;
using Kampai.Game;
using Kampai.Util;
using strange.extensions.command.impl;

public class SaveDevicePrefsCommand : Command
{
	public IKampaiLogger logger = LogManager.GetClassLogger("SaveDevicePrefsCommand") as IKampaiLogger;

	[Inject]
	public IDevicePrefsService prefsService { get; set; }

	[Inject]
	public ILocalPersistanceService persistService { get; set; }

	public override void Execute()
	{
		string value = prefsService.Serialize();
		if (string.IsNullOrEmpty(value))
		{
			logger.Log(KampaiLogLevel.Debug, "Problem serializing device prefs");
		}
		else
		{
			persistService.PutData("DevicePrefs", value);
		}
	}
}
