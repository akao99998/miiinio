using System;
using Kampai.Game.View;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class BridgeBuilding : Building<BridgeBuildingDefinition>
	{
		public int BridgeId { get; set; }

		public int UnlockLevel { get; set; }

		public BridgeBuilding(BridgeBuildingDefinition def)
			: base(def)
		{
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
					UnlockLevel = Convert.ToInt32(reader.Value);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "BRIDGEID":
				reader.Read();
				BridgeId = Convert.ToInt32(reader.Value);
				break;
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
			writer.WritePropertyName("BridgeId");
			writer.WriteValue(BridgeId);
			writer.WritePropertyName("UnlockLevel");
			writer.WriteValue(UnlockLevel);
		}

		public override BuildingObject AddBuildingObject(GameObject gameObject)
		{
			return gameObject.AddComponent<BridgeBuildingObject>();
		}
	}
}
