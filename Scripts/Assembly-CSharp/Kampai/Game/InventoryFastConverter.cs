using System;
using Kampai.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kampai.Game
{
	public class InventoryFastConverter : FastJsonCreationConverter<Instance>
	{
		private IDefinitionService definitionService;

		private IKampaiLogger logger;

		private int defId = -1;

		private int id = -1;

		private Definition def;

		private bool isSaleItem;

		public InventoryFastConverter(IDefinitionService definitionService, IKampaiLogger logger)
		{
			this.definitionService = definitionService;
			this.logger = logger;
		}

		public override Instance ReadJson(JsonReader reader, JsonConverters converters)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			def = null;
			JObject jObject = JObject.Load(reader);
			JProperty jProperty = jObject.Property("def");
			JProperty jProperty2 = ((jProperty != null) ? jProperty : jObject.Property("Definition"));
			isSaleItem = jObject.Property("BuyQuantity") == null;
			if (jProperty2 != null)
			{
				JProperty jProperty3 = jObject.Property("isDynamicSaleDefinition");
				if (jProperty3 != null)
				{
					if (jProperty3.Value.ToString().ToLower().Equals("true"))
					{
						if (jProperty2.Value.Type == JTokenType.Object)
						{
							JObject jObject2 = (JObject)jProperty2.Value;
							JProperty jProperty4 = jObject2.Property("PlatformStoreSku");
							if (jProperty4 != null && jProperty4.Value.Type != JTokenType.Array)
							{
								JToken value = jProperty4.Value;
								JArray jArray = new JArray();
								jArray.Add(value);
								jProperty4.Value.Replace(jArray);
							}
						}
						def = JsonConvert.DeserializeObject<SalePackDefinition>(jProperty2.Value.ToString());
					}
					else
					{
						defId = jProperty2.Value.Value<int>();
						if (!definitionService.TryGet<Definition>(defId, out def))
						{
							def = new SalePackDefinition
							{
								ID = defId,
								Type = SalePackType.Store
							};
						}
					}
				}
				else
				{
					defId = jProperty2.Value.Value<int>();
					def = definitionService.Get(defId);
					if (defId == 77777)
					{
						JProperty jProperty5 = jObject.Property("dynamicDefinition");
						def = JsonConvert.DeserializeObject<DynamicQuestDefinition>(jProperty5.Value.ToString());
					}
				}
			}
			reader = jObject.CreateReader();
			return base.ReadJson(reader, converters);
		}

		public override Instance Create()
		{
			if (def == null)
			{
				throw new JsonSerializationException(string.Format("InventoryFastConverter.Create(): null definition id={0} defId={1}", id, defId));
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
			if (type == typeof(MarketplaceRefreshTimerDefinition))
			{
				return new MarketplaceRefreshTimer((MarketplaceRefreshTimerDefinition)def);
			}
			if (type == typeof(MinionPartyDefinition))
			{
				return new MinionParty((MinionPartyDefinition)def);
			}
			if (type == typeof(AchievementDefinition))
			{
				return new Achievement((AchievementDefinition)def);
			}
			if (type == typeof(VillainLairDefinition))
			{
				return new VillainLair((VillainLairDefinition)def);
			}
			if (type == typeof(CurrencyStorePackDefinition))
			{
				SalePackDefinition salePackDefinition = new SalePackDefinition();
				salePackDefinition.ID = def.ID;
				salePackDefinition.Type = SalePackType.Store;
				return new Sale(salePackDefinition);
			}
			logger.Error("Unable to map inventory type of {0}", type);
			throw new JsonSerializationException(string.Format("InventoryFastConverter.Create: Unable to map inventory type of {0}", type));
		}
	}
}
