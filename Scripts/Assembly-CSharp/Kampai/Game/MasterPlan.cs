using System;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class MasterPlan : Instance<MasterPlanDefinition>, IGameTimeTracker
	{
		public int cooldownUTCStartTime { get; set; }

		public bool introHasBeenDisplayed { get; set; }

		public bool displayCooldownReward { get; set; }

		public bool displayCooldownAlert { get; set; }

		public int StartGameTime { get; set; }

		public int completionCount { get; set; }

		public MasterPlan(MasterPlanDefinition definition)
			: base(definition)
		{
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "COOLDOWNUTCSTARTTIME":
				reader.Read();
				cooldownUTCStartTime = Convert.ToInt32(reader.Value);
				break;
			case "INTROHASBEENDISPLAYED":
				reader.Read();
				introHasBeenDisplayed = Convert.ToBoolean(reader.Value);
				break;
			case "DISPLAYCOOLDOWNREWARD":
				reader.Read();
				displayCooldownReward = Convert.ToBoolean(reader.Value);
				break;
			case "DISPLAYCOOLDOWNALERT":
				reader.Read();
				displayCooldownAlert = Convert.ToBoolean(reader.Value);
				break;
			case "STARTGAMETIME":
				reader.Read();
				StartGameTime = Convert.ToInt32(reader.Value);
				break;
			case "COMPLETIONCOUNT":
				reader.Read();
				completionCount = Convert.ToInt32(reader.Value);
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
			writer.WritePropertyName("cooldownUTCStartTime");
			writer.WriteValue(cooldownUTCStartTime);
			writer.WritePropertyName("introHasBeenDisplayed");
			writer.WriteValue(introHasBeenDisplayed);
			writer.WritePropertyName("displayCooldownReward");
			writer.WriteValue(displayCooldownReward);
			writer.WritePropertyName("displayCooldownAlert");
			writer.WriteValue(displayCooldownAlert);
			writer.WritePropertyName("StartGameTime");
			writer.WriteValue(StartGameTime);
			writer.WritePropertyName("completionCount");
			writer.WriteValue(completionCount);
		}
	}
}
