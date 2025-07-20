using System;
using System.Collections.Generic;
using Kampai.Game.View;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class VillainLairResourcePlot : Building<VillainLairResourcePlotDefinition>
	{
		public List<int> BonusMinionItems = new List<int>();

		public int MinionIDInBuilding;

		public int LastMinionTasked;

		public int rotation { get; set; }

		public int unlockTransactionID { get; set; }

		public VillainLair parentLair { get; set; }

		public int indexInLairResourcePlots { get; set; }

		public int UTCLastTaskingTimeStarted { get; set; }

		public int harvestCount { get; set; }

		public VillainLairResourcePlot(VillainLairResourcePlotDefinition def)
			: base(def)
		{
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "ROTATION":
				reader.Read();
				rotation = Convert.ToInt32(reader.Value);
				break;
			case "UNLOCKTRANSACTIONID":
				reader.Read();
				unlockTransactionID = Convert.ToInt32(reader.Value);
				break;
			case "PARENTLAIR":
				reader.Read();
				parentLair = (VillainLair)converters.instanceConverter.ReadJson(reader, converters);
				break;
			case "INDEXINLAIRRESOURCEPLOTS":
				reader.Read();
				indexInLairResourcePlots = Convert.ToInt32(reader.Value);
				break;
			case "UTCLASTTASKINGTIMESTARTED":
				reader.Read();
				UTCLastTaskingTimeStarted = Convert.ToInt32(reader.Value);
				break;
			case "HARVESTCOUNT":
				reader.Read();
				harvestCount = Convert.ToInt32(reader.Value);
				break;
			case "BONUSMINIONITEMS":
				reader.Read();
				BonusMinionItems = ReaderUtil.PopulateListInt32(reader, BonusMinionItems);
				break;
			case "MINIONIDINBUILDING":
				reader.Read();
				MinionIDInBuilding = Convert.ToInt32(reader.Value);
				break;
			case "LASTMINIONTASKED":
				reader.Read();
				LastMinionTasked = Convert.ToInt32(reader.Value);
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
			writer.WritePropertyName("rotation");
			writer.WriteValue(rotation);
			writer.WritePropertyName("unlockTransactionID");
			writer.WriteValue(unlockTransactionID);
			if (parentLair != null)
			{
				writer.WritePropertyName("parentLair");
				parentLair.Serialize(writer);
			}
			writer.WritePropertyName("indexInLairResourcePlots");
			writer.WriteValue(indexInLairResourcePlots);
			writer.WritePropertyName("UTCLastTaskingTimeStarted");
			writer.WriteValue(UTCLastTaskingTimeStarted);
			writer.WritePropertyName("harvestCount");
			writer.WriteValue(harvestCount);
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
			writer.WritePropertyName("MinionIDInBuilding");
			writer.WriteValue(MinionIDInBuilding);
			writer.WritePropertyName("LastMinionTasked");
			writer.WriteValue(LastMinionTasked);
		}

		public override BuildingObject AddBuildingObject(GameObject gameObject)
		{
			return gameObject.AddComponent<VillainLairResourcePlotObjectView>();
		}

		public void AddMinion(int minionID, int utcTime)
		{
			if (!MinionIsTaskedToBuilding())
			{
				MinionIDInBuilding = minionID;
				LastMinionTasked = minionID;
				if (UTCLastTaskingTimeStarted == 0)
				{
					UTCLastTaskingTimeStarted = utcTime;
				}
			}
		}

		public bool MinionIsTaskedToBuilding()
		{
			if (MinionIDInBuilding != 0)
			{
				return true;
			}
			return false;
		}

		public void ClearMinionInBuilding()
		{
			MinionIDInBuilding = 0;
		}
	}
}
