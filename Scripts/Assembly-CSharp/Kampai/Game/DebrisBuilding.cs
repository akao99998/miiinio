using System;
using Kampai.Game.View;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class DebrisBuilding : TaskableBuilding<DebrisBuildingDefinition>, DestructibleBuilding
	{
		public bool PaidInputCostToClear { get; set; }

		public DebrisBuilding(DebrisBuildingDefinition def)
			: base(def)
		{
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "PAIDINPUTCOSTTOCLEAR":
				reader.Read();
				PaidInputCostToClear = Convert.ToBoolean(reader.Value);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
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
			writer.WritePropertyName("PaidInputCostToClear");
			writer.WriteValue(PaidInputCostToClear);
		}

		public override BuildingObject AddBuildingObject(GameObject gameObject)
		{
			return gameObject.AddComponent<DebrisBuildingObject>();
		}

		public override int GetTransactionID(IDefinitionService definitionService)
		{
			return base.Definition.TransactionID;
		}
	}
}
