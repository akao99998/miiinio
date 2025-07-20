using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Kampai.Game
{
	public class ItemDefinitionConverter : CustomCreationConverter<ItemDefinition>
	{
		private ItemType.ItemTypeIdentifier itemType;

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JObject jObject = JObject.Load(reader);
			if (jObject.Property("type") != null)
			{
				string value = jObject.Property("type").Value.ToString();
				itemType = (ItemType.ItemTypeIdentifier)(int)Enum.Parse(typeof(ItemType.ItemTypeIdentifier), value);
			}
			else
			{
				itemType = ItemType.ItemTypeIdentifier.DEFAULT;
			}
			reader = jObject.CreateReader();
			return base.ReadJson(reader, objectType, existingValue, serializer);
		}

		public override ItemDefinition Create(Type objectType)
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
				return null;
			}
		}
	}
}
