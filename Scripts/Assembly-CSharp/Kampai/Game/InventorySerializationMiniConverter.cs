using System;
using Kampai.Util.MiniJSON;

namespace Kampai.Game
{
	public class InventorySerializationMiniConverter : MiniJSONSerializeConverter
	{
		public object Convert(object value)
		{
			Type type = value.GetType();
			if (typeof(Definition).IsAssignableFrom(type) && (!type.Equals(typeof(SalePackDefinition)) || ((SalePackDefinition)value).Type != SalePackType.Upsell) && !typeof(DynamicQuestDefinition).IsAssignableFrom(type))
			{
				return ((Definition)value).ID;
			}
			return value;
		}
	}
}
