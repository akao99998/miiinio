using System;
using Kampai.Game.Mtx;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Kampai.Game
{
	public class MtxReceiptConverter : CustomCreationConverter<IMtxReceipt>
	{
		private IMtxReceipt.PlatformStore platformStore;

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JObject jObject = JObject.Load(reader);
			if (jObject.Property("platformStore") != null)
			{
				string value = jObject.Property("platformStore").Value.ToString();
				platformStore = (IMtxReceipt.PlatformStore)(int)Enum.Parse(typeof(IMtxReceipt.PlatformStore), value);
			}
			reader = jObject.CreateReader();
			return base.ReadJson(reader, objectType, existingValue, serializer);
		}

		public override IMtxReceipt Create(Type objectType)
		{
			switch (platformStore)
			{
			case IMtxReceipt.PlatformStore.AppleAppStore:
				return new AppleAppStoreReceipt();
			case IMtxReceipt.PlatformStore.GooglePlay:
				return new GooglePlayReceipt();
			case IMtxReceipt.PlatformStore.AmazonAppStore:
				return new AmazonAppStoreReceipt();
			default:
				return null;
			}
		}
	}
}
