using System;
using Kampai.Game.View;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class StuartCharacter : FrolicCharacter<StuartCharacterDefinition>
	{
		public bool WasHonorGuest { get; set; }

		public StuartCharacter(StuartCharacterDefinition def)
			: base(def)
		{
			Name = "Stuart";
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "WASHONORGUEST":
				reader.Read();
				WasHonorGuest = Convert.ToBoolean(reader.Value);
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
			writer.WritePropertyName("WasHonorGuest");
			writer.WriteValue(WasHonorGuest);
		}

		public override NamedCharacterObject Setup(GameObject go)
		{
			return go.AddComponent<StuartView>();
		}
	}
}
