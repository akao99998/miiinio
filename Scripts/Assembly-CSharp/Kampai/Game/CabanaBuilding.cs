using Kampai.Game.View;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class CabanaBuilding : Building<CabanaBuildingDefinition>
	{
		private bool occupied;

		[JsonIgnore]
		public bool Occupied
		{
			get
			{
				return occupied;
			}
			set
			{
				occupied = value;
			}
		}

		public Quest Quest { get; set; }

		public CabanaBuilding(CabanaBuildingDefinition def)
			: base(def)
		{
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "QUEST":
				reader.Read();
				Quest = (Quest)converters.instanceConverter.ReadJson(reader, converters);
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
			if (Quest != null)
			{
				writer.WritePropertyName("Quest");
				Quest.Serialize(writer);
			}
		}

		public override BuildingObject AddBuildingObject(GameObject gameObject)
		{
			return gameObject.AddComponent<CabanaBuildingObject>();
		}

		public override string GetPrefab(int index = 0)
		{
			if (State == BuildingState.Inaccessible || State == BuildingState.Broken)
			{
				return base.Definition.brokenPrefab;
			}
			if (State == BuildingState.Idle)
			{
				return base.Definition.InactivePrefab;
			}
			return base.Definition.Prefab;
		}

		public override bool IsBuildingRepaired()
		{
			return State != BuildingState.Broken && State != BuildingState.Inaccessible;
		}
	}
}
