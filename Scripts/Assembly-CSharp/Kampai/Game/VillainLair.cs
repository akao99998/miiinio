using System;
using System.Collections.Generic;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class VillainLair : Instance<VillainLairDefinition>
	{
		public bool hasVisited { get; set; }

		public List<int> resourcePlotInstanceIDs { get; set; }

		public int portalInstanceID { get; set; }

		public VillainLair(VillainLairDefinition def)
			: base(def)
		{
			resourcePlotInstanceIDs = new List<int>();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "HASVISITED":
				reader.Read();
				hasVisited = Convert.ToBoolean(reader.Value);
				break;
			case "RESOURCEPLOTINSTANCEIDS":
				reader.Read();
				resourcePlotInstanceIDs = ReaderUtil.PopulateListInt32(reader, resourcePlotInstanceIDs);
				break;
			case "PORTALINSTANCEID":
				reader.Read();
				portalInstanceID = Convert.ToInt32(reader.Value);
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
			writer.WritePropertyName("hasVisited");
			writer.WriteValue(hasVisited);
			if (resourcePlotInstanceIDs != null)
			{
				writer.WritePropertyName("resourcePlotInstanceIDs");
				writer.WriteStartArray();
				List<int>.Enumerator enumerator = resourcePlotInstanceIDs.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						int current = enumerator.Current;
						writer.WriteValue(current);
					}
				}
				finally
				{
					enumerator.Dispose();
				}
				writer.WriteEndArray();
			}
			writer.WritePropertyName("portalInstanceID");
			writer.WriteValue(portalInstanceID);
		}

		public List<int> GetAllPlotBonusItems(IPlayerService playerService)
		{
			List<int> list = new List<int>();
			foreach (int resourcePlotInstanceID in resourcePlotInstanceIDs)
			{
				VillainLairResourcePlot byInstanceId = playerService.GetByInstanceId<VillainLairResourcePlot>(resourcePlotInstanceID);
				if (byInstanceId.BonusMinionItems.Count > 0)
				{
					list.AddRange(byInstanceId.BonusMinionItems);
				}
			}
			return list;
		}

		public int GetFirstBuildingNumberOfHarvestableResourcePlot(IPlayerService playerService, bool isBonus)
		{
			foreach (int resourcePlotInstanceID in resourcePlotInstanceIDs)
			{
				VillainLairResourcePlot byInstanceId = playerService.GetByInstanceId<VillainLairResourcePlot>(resourcePlotInstanceID);
				if (isBonus)
				{
					if (byInstanceId.BonusMinionItems.Count > 0)
					{
						return byInstanceId.ID;
					}
				}
				else if (byInstanceId.State == BuildingState.Harvestable)
				{
					return byInstanceId.ID;
				}
			}
			return 0;
		}
	}
}
