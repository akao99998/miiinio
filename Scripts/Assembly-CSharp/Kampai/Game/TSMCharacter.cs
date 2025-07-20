using System;
using Kampai.Game.View;
using Kampai.Util.AI;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class TSMCharacter : NamedCharacter<TSMCharacterDefinition>
	{
		public bool HasShownIntroNarrative { get; set; }

		public int PreviousTaskUTCTime { get; set; }

		public TSMCharacter(TSMCharacterDefinition def)
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
					PreviousTaskUTCTime = Convert.ToInt32(reader.Value);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "HASSHOWNINTRONARRATIVE":
				reader.Read();
				HasShownIntroNarrative = Convert.ToBoolean(reader.Value);
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
			writer.WritePropertyName("HasShownIntroNarrative");
			writer.WriteValue(HasShownIntroNarrative);
			writer.WritePropertyName("PreviousTaskUTCTime");
			writer.WriteValue(PreviousTaskUTCTime);
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
			return go.AddComponent<TSMCharacterView>();
		}
	}
}
