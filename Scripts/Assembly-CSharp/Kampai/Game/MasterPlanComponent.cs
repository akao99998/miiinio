using System;
using System.Collections.Generic;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class MasterPlanComponent : Instance<MasterPlanComponentDefinition>
	{
		public List<MasterPlanComponentTask> tasks = new List<MasterPlanComponentTask>();

		public MasterPlanComponentState State { get; set; }

		public int planTrackingInstance { get; set; }

		public int buildingDefID { get; set; }

		public Location buildingLocation { get; set; }

		public MasterPlanComponentReward reward { get; set; }

		public MasterPlanComponent(MasterPlanComponentDefinition def)
			: base(def)
		{
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "STATE":
				reader.Read();
				State = ReaderUtil.ReadEnum<MasterPlanComponentState>(reader);
				break;
			case "PLANTRACKINGINSTANCE":
				reader.Read();
				planTrackingInstance = Convert.ToInt32(reader.Value);
				break;
			case "BUILDINGDEFID":
				reader.Read();
				buildingDefID = Convert.ToInt32(reader.Value);
				break;
			case "BUILDINGLOCATION":
				reader.Read();
				buildingLocation = ReaderUtil.ReadLocation(reader, converters);
				break;
			case "REWARD":
				reader.Read();
				reward = ReaderUtil.ReadMasterPlanComponentReward(reader, converters);
				break;
			case "TASKS":
				reader.Read();
				tasks = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadMasterPlanComponentTask, tasks);
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
			writer.WritePropertyName("State");
			writer.WriteValue((int)State);
			writer.WritePropertyName("planTrackingInstance");
			writer.WriteValue(planTrackingInstance);
			writer.WritePropertyName("buildingDefID");
			writer.WriteValue(buildingDefID);
			if (buildingLocation != null)
			{
				writer.WritePropertyName("buildingLocation");
				writer.WriteStartObject();
				writer.WritePropertyName("x");
				writer.WriteValue(buildingLocation.x);
				writer.WritePropertyName("y");
				writer.WriteValue(buildingLocation.y);
				writer.WriteEndObject();
			}
			if (reward != null)
			{
				writer.WritePropertyName("reward");
				writer.WriteStartObject();
				if (reward.Definition != null)
				{
					writer.WritePropertyName("Definition");
					writer.WriteStartObject();
					writer.WritePropertyName("rewardItemId");
					writer.WriteValue(reward.Definition.rewardItemId);
					writer.WritePropertyName("rewardQuantity");
					writer.WriteValue(reward.Definition.rewardQuantity);
					writer.WritePropertyName("grindReward");
					writer.WriteValue(reward.Definition.grindReward);
					writer.WritePropertyName("premiumReward");
					writer.WriteValue(reward.Definition.premiumReward);
					writer.WriteEndObject();
				}
				writer.WriteEndObject();
			}
			if (tasks == null)
			{
				return;
			}
			writer.WritePropertyName("tasks");
			writer.WriteStartArray();
			List<MasterPlanComponentTask>.Enumerator enumerator = tasks.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					MasterPlanComponentTask current = enumerator.Current;
					writer.WriteStartObject();
					writer.WritePropertyName("isComplete");
					writer.WriteValue(current.isComplete);
					writer.WritePropertyName("earnedQuantity");
					writer.WriteValue(current.earnedQuantity);
					if (current.Definition != null)
					{
						writer.WritePropertyName("Definition");
						writer.WriteStartObject();
						writer.WritePropertyName("requiredItemId");
						writer.WriteValue(current.Definition.requiredItemId);
						writer.WritePropertyName("requiredQuantity");
						writer.WriteValue(current.Definition.requiredQuantity);
						writer.WritePropertyName("ShowWayfinder");
						writer.WriteValue(current.Definition.ShowWayfinder);
						writer.WritePropertyName("Type");
						writer.WriteValue((int)current.Definition.Type);
						writer.WriteEndObject();
					}
					writer.WriteEndObject();
				}
			}
			finally
			{
				enumerator.Dispose();
			}
			writer.WriteEndArray();
		}
	}
}
