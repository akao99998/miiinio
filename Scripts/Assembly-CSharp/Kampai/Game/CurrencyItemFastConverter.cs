using System;
using Kampai.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kampai.Game
{
	public class CurrencyItemFastConverter : FastJsonCreationConverter<CurrencyItemDefinition>
	{
		private bool salePackType;

		private bool platformStoreSkuPropertyExists;

		public override CurrencyItemDefinition ReadJson(JsonReader reader, JsonConverters converters)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			JObject jObject = JObject.Load(reader);
			JProperty jProperty = jObject.Property("type");
			if (jProperty != null)
			{
				salePackType = Enum.IsDefined(typeof(SalePackType), jProperty.Value.ToString());
			}
			platformStoreSkuPropertyExists = jObject.Property("platformStoreSku") != null;
			reader = jObject.CreateReader();
			return base.ReadJson(reader, converters);
		}

		public override CurrencyItemDefinition Create()
		{
			if (salePackType)
			{
				return new SalePackDefinition();
			}
			if (platformStoreSkuPropertyExists)
			{
				return new PremiumCurrencyItemDefinition();
			}
			return new CurrencyItemDefinition();
		}
	}
}
