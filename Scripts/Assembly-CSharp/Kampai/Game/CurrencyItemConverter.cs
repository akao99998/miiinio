using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Kampai.Game
{
	public class CurrencyItemConverter : CustomCreationConverter<CurrencyItemDefinition>
	{
		private bool salePackType;

		private SalePackType type;

		private bool platformStoreSkuPropertyExists;

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JObject jObject = JObject.Load(reader);
			JProperty jProperty = jObject.Property("type");
			if (jProperty != null)
			{
				salePackType = Enum.IsDefined(typeof(SalePackType), jProperty.Value.ToString());
				if (salePackType)
				{
					string value = jProperty.Value.ToString();
					type = (SalePackType)(int)Enum.Parse(typeof(SalePackType), value);
				}
			}
			platformStoreSkuPropertyExists = jObject.Property("platformStoreSku") != null;
			reader = jObject.CreateReader();
			return base.ReadJson(reader, objectType, existingValue, serializer);
		}

		public override CurrencyItemDefinition Create(Type objectType)
		{
			if (salePackType)
			{
				if (type == SalePackType.Store)
				{
					return new CurrencyStorePackDefinition();
				}
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
