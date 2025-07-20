using System;
using Kampai.Game.View;
using Kampai.Util.AI;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class SpecialEventCharacter : FrolicCharacter<SpecialEventCharacterDefinition>
	{
		public bool HasShownIntroNarrative { get; set; }

		public int PreviousTaskUTCTime { get; set; }

		public int SpecialEventID { get; set; }

		public int PrestigeDefinitionID { get; set; }

		public SpecialEventCharacter(SpecialEventCharacterDefinition def)
			: base(def)
		{
			Name = "SpecialEventMinion";
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "HASSHOWNINTRONARRATIVE":
				reader.Read();
				HasShownIntroNarrative = Convert.ToBoolean(reader.Value);
				break;
			case "PREVIOUSTASKUTCTIME":
				reader.Read();
				PreviousTaskUTCTime = Convert.ToInt32(reader.Value);
				break;
			case "SPECIALEVENTID":
				reader.Read();
				SpecialEventID = Convert.ToInt32(reader.Value);
				break;
			case "PRESTIGEDEFINITIONID":
				reader.Read();
				PrestigeDefinitionID = Convert.ToInt32(reader.Value);
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
			writer.WritePropertyName("HasShownIntroNarrative");
			writer.WriteValue(HasShownIntroNarrative);
			writer.WritePropertyName("PreviousTaskUTCTime");
			writer.WriteValue(PreviousTaskUTCTime);
			writer.WritePropertyName("SpecialEventID");
			writer.WriteValue(SpecialEventID);
			writer.WritePropertyName("PrestigeDefinitionID");
			writer.WriteValue(PrestigeDefinitionID);
		}

		public override NamedCharacterObject Setup(GameObject go)
		{
			Agent agent = go.GetComponent<Agent>();
			if (agent == null)
			{
				agent = go.AddComponent<Agent>();
			}
			agent.Radius = 0.5f;
			agent.Mass = 1f;
			agent.MaxForce = 0f;
			agent.MaxSpeed = 0f;
			return go.AddComponent<SpecialEventCharacterView>();
		}
	}
}
