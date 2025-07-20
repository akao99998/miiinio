using System;
using Kampai.Game.View;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class StorageBuilding : RepairableBuilding<StorageBuildingDefinition>
	{
		public int CurrentStorageBuildingLevel { get; set; }

		[JsonIgnore]
		public bool MenuOpened { get; set; }

		[JsonIgnore]
		public bool MenuOpening { get; set; }

		public StorageBuilding(StorageBuildingDefinition def)
			: base(def)
		{
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "CURRENTSTORAGEBUILDINGLEVEL":
				reader.Read();
				CurrentStorageBuildingLevel = Convert.ToInt32(reader.Value);
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
			writer.WritePropertyName("CurrentStorageBuildingLevel");
			writer.WriteValue(CurrentStorageBuildingLevel);
		}

		public override BuildingObject AddBuildingObject(GameObject gameObject)
		{
			return gameObject.AddComponent<StorageBuildingObject>();
		}
	}
}
