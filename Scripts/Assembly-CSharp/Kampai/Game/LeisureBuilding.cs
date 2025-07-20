using System;
using System.Collections.Generic;
using Kampai.Game.View;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class LeisureBuilding : Building<LeisureBuildingDefintiion>
	{
		protected IList<int> minionList;

		[JsonIgnore]
		public IList<int> MinionList
		{
			get
			{
				return minionList;
			}
			set
			{
				minionList = value;
			}
		}

		public int UTCLastTaskingTimeStarted { get; set; }

		public LeisureBuilding(LeisureBuildingDefintiion def)
			: base(def)
		{
			minionList = new List<int>();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "UTCLASTTASKINGTIMESTARTED":
				reader.Read();
				UTCLastTaskingTimeStarted = Convert.ToInt32(reader.Value);
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
			writer.WritePropertyName("UTCLastTaskingTimeStarted");
			writer.WriteValue(UTCLastTaskingTimeStarted);
		}

		public override BuildingObject AddBuildingObject(GameObject gameObject)
		{
			return gameObject.AddComponent<LeisureBuildingObjectView>();
		}

		public int GetMinionsInBuilding()
		{
			return minionList.Count;
		}

		public void AddMinion(int minionID, int utcTime)
		{
			if (!minionList.Contains(minionID))
			{
				minionList.Add(minionID);
				if (UTCLastTaskingTimeStarted == 0)
				{
					UTCLastTaskingTimeStarted = utcTime;
				}
			}
		}

		public int GetMinionRouteIndex(int minionID)
		{
			return minionList.IndexOf(minionID);
		}

		public void CleanMinionQueue()
		{
			minionList.Clear();
		}

		public int GetMinionByIndex(int index)
		{
			return minionList[index];
		}
	}
}
