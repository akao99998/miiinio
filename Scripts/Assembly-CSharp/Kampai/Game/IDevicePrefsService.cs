namespace Kampai.Game
{
	public interface IDevicePrefsService
	{
		DevicePrefs GetDevicePrefs();

		void Deserialize(string serialized);

		string Serialize();
	}
}
