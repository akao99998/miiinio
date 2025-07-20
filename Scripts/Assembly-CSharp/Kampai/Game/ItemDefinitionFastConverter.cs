using System;
using Kampai.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kampai.Game
{
	public class ItemDefinitionFastConverter : FastJsonCreationConverter<ItemDefinition>
	{
		private ItemType.ItemTypeIdentifier itemType;

		public override ItemDefinition ReadJson(JsonReader reader, JsonConverters converters)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			JObject jObject = JObject.Load(reader);
			JProperty jProperty = jObject.Property("type");
			if (jProperty != null)
			{
				string value = jProperty.Value.ToString();
				itemType = (ItemType.ItemTypeIdentifier)(int)Enum.Parse(typeof(ItemType.ItemTypeIdentifier), value);
			}
			else
			{
				itemType = ItemType.ItemTypeIdentifier.DEFAULT;
			}
			reader = jObject.CreateReader();
			return base.ReadJson(reader, converters);
		}

		public override ItemDefinition Create()
		{
			switch (itemType)
			{
			case ItemType.ItemTypeIdentifier.DEFAULT:
				return new ItemDefinition();
			case ItemType.ItemTypeIdentifier.INGREDIENTS:
				return new IngredientsItemDefinition();
			case ItemType.ItemTypeIdentifier.DYNAMIC_INGREDIENTS:
				return new DynamicIngredientsDefinition();
			case ItemType.ItemTypeIdentifier.COSTUME:
				return new CostumeItemDefinition();
			case ItemType.ItemTypeIdentifier.UNLOCK:
				return new UnlockDefinition();
			case ItemType.ItemTypeIdentifier.BRIDGE:
				return new BridgeDefinition();
			case ItemType.ItemTypeIdentifier.DROP:
				return new DropItemDefinition();
			case ItemType.ItemTypeIdentifier.PARTY_ANIMATION_ITEM:
				return new PartyFavorAnimationItemDefinition();
			case ItemType.ItemTypeIdentifier.SPECIAL_EVENT:
				return new SpecialEventItemDefinition();
			default:
				throw new JsonSerializationException(string.Format("Unexpected ItemDefinition type: {0}", itemType));
			}
		}
	}
}
