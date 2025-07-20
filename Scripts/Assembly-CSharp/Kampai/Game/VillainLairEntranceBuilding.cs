using System;
using Kampai.Game.View;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class VillainLairEntranceBuilding : RepairableBuilding<VillainLairEntranceBuildingDefinition>
	{
		public bool IsUnlocked { get; set; }

		public int VillainLairInstanceID { get; set; }

		public VillainLairEntranceBuilding(VillainLairEntranceBuildingDefinition def)
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
					VillainLairInstanceID = Convert.ToInt32(reader.Value);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "ISUNLOCKED":
				reader.Read();
				IsUnlocked = Convert.ToBoolean(reader.Value);
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
			writer.WritePropertyName("IsUnlocked");
			writer.WriteValue(IsUnlocked);
			writer.WritePropertyName("VillainLairInstanceID");
			writer.WriteValue(VillainLairInstanceID);
		}

		public override BuildingObject AddBuildingObject(GameObject gameObject)
		{
			return gameObject.AddComponent<VillainLairEntranceBuildingObject>();
		}

		public void unlockVillainLair(int lairInstanceID)
		{
			IsUnlocked = true;
			VillainLairInstanceID = lairInstanceID;
		}

		public Tuple<int, int> GetNewHarvestAvailableForPortal(IPlayerService playerService)
		{
			int num = 0;
			int num2 = 0;
			VillainLair byInstanceId = playerService.GetByInstanceId<VillainLair>(VillainLairInstanceID);
			foreach (int resourcePlotInstanceID in byInstanceId.resourcePlotInstanceIDs)
			{
				VillainLairResourcePlot byInstanceId2 = playerService.GetByInstanceId<VillainLairResourcePlot>(resourcePlotInstanceID);
				if (byInstanceId2.State == BuildingState.Harvestable)
				{
					num++;
				}
				if (byInstanceId2.BonusMinionItems.Count > 0)
				{
					num2 += byInstanceId2.BonusMinionItems.Count;
				}
			}
			return new Tuple<int, int>(num, num2);
		}
	}
}
