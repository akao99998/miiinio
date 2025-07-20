using Newtonsoft.Json;

namespace Kampai.Game
{
	public class DevicePrefs
	{
		[JsonProperty("ConstructionNotif")]
		public bool ConstructionNotif = true;

		[JsonProperty("BlackMarketNotif")]
		public bool BlackMarketNotif = true;

		[JsonProperty("MinionsNotif")]
		public bool MinionsParadiseNotif = true;

		[JsonProperty("BaseResourceNotif")]
		public bool BaseResourceNotif = true;

		[JsonProperty("CraftingNotif")]
		public bool CraftingNotif = true;

		[JsonProperty("EventNotif")]
		public bool EventNotif = true;

		[JsonProperty("MarketPlaceNotif")]
		public bool MarketPlaceNotif = true;

		[JsonProperty("SocialEventNotif")]
		public bool SocialEventNotif = true;

		[JsonProperty("MusicVolume")]
		public float MusicVolume = 1f;

		[JsonProperty("SFXVolume")]
		public float SFXVolume = 1f;
	}
}
