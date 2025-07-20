using System;
using System.Collections.Generic;
using Kampai.Game.View;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class CraftingBuilding : Building<CraftingBuildingDefinition>
	{
		public int Slots { get; set; }

		public IList<int> RecipeInQueue { get; set; }

		public IList<int> CompletedCrafts { get; set; }

		public int CraftingStartTime { get; set; }

		public int PartyTimeReduction { get; set; }

		public CraftingBuilding(CraftingBuildingDefinition def)
			: base(def)
		{
			Slots = def.InitialSlots;
			RecipeInQueue = new List<int>();
			CompletedCrafts = new List<int>();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "SLOTS":
				reader.Read();
				Slots = Convert.ToInt32(reader.Value);
				break;
			case "RECIPEINQUEUE":
				reader.Read();
				RecipeInQueue = ReaderUtil.PopulateListInt32(reader, RecipeInQueue);
				break;
			case "COMPLETEDCRAFTS":
				reader.Read();
				CompletedCrafts = ReaderUtil.PopulateListInt32(reader, CompletedCrafts);
				break;
			case "CRAFTINGSTARTTIME":
				reader.Read();
				CraftingStartTime = Convert.ToInt32(reader.Value);
				break;
			case "PARTYTIMEREDUCTION":
				reader.Read();
				PartyTimeReduction = Convert.ToInt32(reader.Value);
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
			writer.WritePropertyName("Slots");
			writer.WriteValue(Slots);
			if (RecipeInQueue != null)
			{
				writer.WritePropertyName("RecipeInQueue");
				writer.WriteStartArray();
				IEnumerator<int> enumerator = RecipeInQueue.GetEnumerator();
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
			if (CompletedCrafts != null)
			{
				writer.WritePropertyName("CompletedCrafts");
				writer.WriteStartArray();
				IEnumerator<int> enumerator2 = CompletedCrafts.GetEnumerator();
				try
				{
					while (enumerator2.MoveNext())
					{
						int current2 = enumerator2.Current;
						writer.WriteValue(current2);
					}
				}
				finally
				{
					enumerator2.Dispose();
				}
				writer.WriteEndArray();
			}
			writer.WritePropertyName("CraftingStartTime");
			writer.WriteValue(CraftingStartTime);
			writer.WritePropertyName("PartyTimeReduction");
			writer.WriteValue(PartyTimeReduction);
		}

		public int getNextIncrementalCost()
		{
			return base.Definition.SlotCost + (Slots - base.Definition.InitialSlots) * base.Definition.SlotIncrementalCost;
		}

		public override BuildingObject AddBuildingObject(GameObject gameObject)
		{
			return gameObject.AddComponent<CraftableBuildingObject>();
		}
	}
}
