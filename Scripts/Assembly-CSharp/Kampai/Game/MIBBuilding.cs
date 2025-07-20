using System;
using Kampai.Game.View;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class MIBBuilding : Building<MIBBuildingDefinition>
	{
		public int UTCExpiryTime { get; set; }

		public int UTCCooldownTime { get; set; }

		public MIBBuildingState MIBState { get; set; }

		public int NumOfRewardsCollectedOnTap { get; set; }

		public int NumOfRewardsCollectedOnReturn { get; set; }

		public MIBBuilding(MIBBuildingDefinition def)
			: base(def)
		{
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "UTCEXPIRYTIME":
				reader.Read();
				UTCExpiryTime = Convert.ToInt32(reader.Value);
				break;
			case "UTCCOOLDOWNTIME":
				reader.Read();
				UTCCooldownTime = Convert.ToInt32(reader.Value);
				break;
			case "MIBSTATE":
				reader.Read();
				MIBState = ReaderUtil.ReadEnum<MIBBuildingState>(reader);
				break;
			case "NUMOFREWARDSCOLLECTEDONTAP":
				reader.Read();
				NumOfRewardsCollectedOnTap = Convert.ToInt32(reader.Value);
				break;
			case "NUMOFREWARDSCOLLECTEDONRETURN":
				reader.Read();
				NumOfRewardsCollectedOnReturn = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public override void Serialize(JsonWriter writer)
		{
			writer.WriteStartObject();
			SerializeProperties(writer);
			writer.WriteEndObject();
		}

		protected override void SerializeProperties(JsonWriter writer)
		{
			base.SerializeProperties(writer);
			writer.WritePropertyName("UTCExpiryTime");
			writer.WriteValue(UTCExpiryTime);
			writer.WritePropertyName("UTCCooldownTime");
			writer.WriteValue(UTCCooldownTime);
			writer.WritePropertyName("MIBState");
			writer.WriteValue((int)MIBState);
			writer.WritePropertyName("NumOfRewardsCollectedOnTap");
			writer.WriteValue(NumOfRewardsCollectedOnTap);
			writer.WritePropertyName("NumOfRewardsCollectedOnReturn");
			writer.WriteValue(NumOfRewardsCollectedOnReturn);
		}

		public override BuildingObject AddBuildingObject(GameObject gameObject)
		{
			return gameObject.AddComponent<MIBBuildingObjectView>();
		}
	}
}
