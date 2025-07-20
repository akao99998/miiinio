using System;
using System.Collections.Generic;
using Kampai.Game.View;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class ResourceBuilding : TaskableBuilding<ResourceBuildingDefinition>
	{
		public List<int> BonusMinionItems = new List<int>();

		public List<int> minionHarvesters = new List<int>();

		public int AvailableHarvest { get; set; }

		public ResourceBuilding(ResourceBuildingDefinition def)
			: base(def)
		{
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "AVAILABLEHARVEST":
				reader.Read();
				AvailableHarvest = Convert.ToInt32(reader.Value);
				break;
			case "BONUSMINIONITEMS":
				reader.Read();
				BonusMinionItems = ReaderUtil.PopulateListInt32(reader, BonusMinionItems);
				break;
			case "MINIONHARVESTERS":
				reader.Read();
				minionHarvesters = ReaderUtil.PopulateListInt32(reader, minionHarvesters);
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
			writer.WritePropertyName("AvailableHarvest");
			writer.WriteValue(AvailableHarvest);
			if (BonusMinionItems != null)
			{
				writer.WritePropertyName("BonusMinionItems");
				writer.WriteStartArray();
				List<int>.Enumerator enumerator = BonusMinionItems.GetEnumerator();
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
			if (minionHarvesters == null)
			{
				return;
			}
			writer.WritePropertyName("minionHarvesters");
			writer.WriteStartArray();
			List<int>.Enumerator enumerator2 = minionHarvesters.GetEnumerator();
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

		public int GetMaxSlotCount()
		{
			return GetProperSlotUnlock().SlotUnlockLevels.Count;
		}

		public int GetSlotCostByIndex(int i)
		{
			return GetProperSlotUnlock().SlotUnlockCosts[i];
		}

		public int GetSlotUnlockLevelByIndex(int i)
		{
			return GetProperSlotUnlock().SlotUnlockLevels[i];
		}

		private SlotUnlock GetProperSlotUnlock()
		{
			int count = base.Definition.SlotUnlocks.Count;
			int index = ((BuildingNumber <= count) ? (BuildingNumber - 1) : (count - 1));
			return base.Definition.SlotUnlocks[index];
		}

		public void IncrementMinionSlotsOwned()
		{
			base.MinionSlotsOwned++;
		}

		public override BuildingObject AddBuildingObject(GameObject gameObject)
		{
			return gameObject.AddComponent<GaggableBuildingObject>();
		}

		public override int GetTransactionID(IDefinitionService definitionService)
		{
			int itemId = base.Definition.ItemId;
			IngredientsItemDefinition ingredientsItemDefinition = definitionService.Get<IngredientsItemDefinition>(itemId);
			return ingredientsItemDefinition.TransactionId;
		}

		public void PrepareForHarvest(int utcTime, int minionId)
		{
			ReconcileMinionStoppedTasking(utcTime);
			AvailableHarvest++;
			minionHarvesters.Add(minionId);
		}

		public int GetLastMinionToHarvest()
		{
			if (minionHarvesters.Count > 0)
			{
				return minionHarvesters[0];
			}
			return -1;
		}

		public void CompleteHarvest()
		{
			AvailableHarvest--;
			if (minionHarvesters.Count > 0)
			{
				minionHarvesters.RemoveAt(0);
			}
		}

		public override int GetAvailableHarvest()
		{
			return AvailableHarvest;
		}

		public int GetTotalHarvests()
		{
			return AvailableHarvest + BonusMinionItems.Count;
		}
	}
}
