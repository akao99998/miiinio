using System;
using System.IO;
using Newtonsoft.Json;
using strange.extensions.context.api;

namespace Kampai.Game.Trigger
{
	public class QuantityItemTriggerConditionDefinition : TriggerConditionDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1168;
			}
		}

		public int itemDefId { get; set; }

		public uint quantity { get; set; }

		public override TriggerConditionType.Identifier type
		{
			get
			{
				return TriggerConditionType.Identifier.QuantityItem;
			}
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(itemDefId);
			writer.Write(quantity);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			itemDefId = reader.ReadInt32();
			quantity = reader.ReadUInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			default:
			{
				int num;
				if (num == 1)
				{
					reader.Read();
					quantity = Convert.ToUInt32(reader.Value);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "ITEMDEFID":
				reader.Read();
				itemDefId = Convert.ToInt32(reader.Value);
				break;
			}
			return true;
		}

		public override string ToString()
		{
			return string.Format("{0}, Operator: {1}, Type: {2}, ItemDefID: {3}, Quantity: {4}", GetType(), base.conditionOp, type, itemDefId, quantity);
		}

		public override bool IsTriggered(ICrossContextCapable gameContext)
		{
			IPlayerService instance = gameContext.injectionBinder.GetInstance<IPlayerService>();
			if (instance == null)
			{
				return false;
			}
			uint totalCountByDefinitionId = instance.GetTotalCountByDefinitionId(itemDefId);
			return TestOperator(quantity, totalCountByDefinitionId);
		}
	}
}
