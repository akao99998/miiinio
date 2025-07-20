using Elevation.Logging;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class DevicePrefsService : IDevicePrefsService
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("DevicePrefsService") as IKampaiLogger;

		private DevicePrefs DevicePrefs = new DevicePrefs();

		private object mutex = new object();

		public DevicePrefs GetDevicePrefs()
		{
			return DevicePrefs;
		}

		public void Deserialize(string serialized)
		{
			lock (mutex)
			{
				try
				{
					DevicePrefs devicePrefs = JsonConvert.DeserializeObject<DevicePrefs>(serialized);
					if (devicePrefs != null)
					{
						DevicePrefs = devicePrefs;
					}
					else
					{
						logger.Fatal(FatalCode.PS_JSON_PARSE_ERR, 1, "Json Parse Err: null Device");
					}
				}
				catch (JsonSerializationException ex)
				{
					logger.Fatal(FatalCode.PS_JSON_PARSE_ERR, 2, "Json Parse Err: {0}", ex);
				}
				catch (JsonReaderException ex2)
				{
					logger.Fatal(FatalCode.PS_JSON_PARSE_ERR, 3, "Json Parse Err: {0}", ex2);
				}
			}
		}

		public string Serialize()
		{
			string text = null;
			lock (mutex)
			{
				try
				{
					text = JsonConvert.SerializeObject(DevicePrefs);
					logger.Debug(text);
				}
				catch (JsonSerializationException ex)
				{
					logger.Fatal(FatalCode.PS_JSON_PARSE_ERR, 4, "Json Parse Err: {0}", ex.ToString());
				}
			}
			return text;
		}
	}
}
