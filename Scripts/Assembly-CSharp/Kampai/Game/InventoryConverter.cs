using System;
using Kampai.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Kampai.Game
{
	public class InventoryConverter : CustomCreationConverter<Instance>
	{
		private IDefinitionService definitionService;

		private IKampaiLogger logger;

		private Definition def;

		private bool isSaleItem;

		public InventoryConverter(IDefinitionService definitionService, IKampaiLogger logger)
		{
			this.definitionService = definitionService;
			this.logger = logger;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType != JsonToken.Null)
			{
				def = null;
				JObject jObject = JObject.Load(reader);
				JProperty jProperty = ((jObject.Property("def") != null) ? jObject.Property("def") : jObject.Property("Definition"));
				isSaleItem = jObject.Property("BuyQuantity") == null;
				if (jProperty != null)
				{
					JProperty jProperty2 = jObject.Property("isDynamicSaleDefinition");
					if (jProperty2 != null && jProperty2.Value.ToString().ToLower().Equals("true"))
					{
						def = JsonConvert.DeserializeObject<SalePackDefinition>(jProperty.Value.ToString());
					}
					else
					{
						int num = jProperty.Value.Value<int>();
						def = definitionService.Get(num);
						if (num == 77777)
						{
							JProperty jProperty3 = jObject.Property("dynamicDefinition");
							def = JsonConvert.DeserializeObject<DynamicQuestDefinition>(jProperty3.Value.ToString());
						}
					}
				}
				reader = jObject.CreateReader();
			}
			return base.ReadJson(reader, objectType, existingValue, serializer);
		}

		public override Instance Create(Type objectType)
		{
			if (def == null)
			{
				throw new JsonSerializationException("InventoryConverter.Create(): null definition.");
			}
			IBuilder<Instance> builder = def as IBuilder<Instance>;
			if (builder != null)
			{
				return builder.Build();
			}
			Type type = def.GetType();
			if (type == typeof(MarketplaceItemDefinition))
			{
				if (isSaleItem)
				{
					return new MarketplaceSaleItem((MarketplaceItemDefinition)def);
				}
				return new MarketplaceBuyItem((MarketplaceItemDefinition)def);
			}
			if (type == typeof(VillainLairDefinition))
			{
				return new VillainLair((VillainLairDefinition)def);
			}
			logger.Error("Unable to map inventory type of {0}", type);
			throw new JsonSerializationException(string.Format("InventoryConverter.Create: Unable to map inventory type of {0}", type));
		}
	}
}
