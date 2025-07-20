using System;
using Kampai.Game.View;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class LandExpansionBuilding : Building<LandExpansionBuildingDefinition>, DestructibleBuilding
	{
		public int ExpansionID { get; set; }

		public int MinimumLevel { get; set; }

		public LandExpansionBuilding(LandExpansionBuildingDefinition def)
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
					MinimumLevel = Convert.ToInt32(reader.Value);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "EXPANSIONID":
				reader.Read();
				ExpansionID = Convert.ToInt32(reader.Value);
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
			writer.WritePropertyName("ExpansionID");
			writer.WriteValue(ExpansionID);
			writer.WritePropertyName("MinimumLevel");
			writer.WriteValue(MinimumLevel);
		}

		public override BuildingObject AddBuildingObject(GameObject gameObject)
		{
			return gameObject.AddComponent<LandExpansionBuildingObject>();
		}
	}
}
