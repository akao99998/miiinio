using System.Collections.Generic;
using Kampai.Game.View;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class TikiBarBuilding : TaskableMinionPartyBuilding<TikiBarBuildingDefinition>, Building, ZoomableBuilding, Instance, Locatable, IFastJSONDeserializable, IFastJSONSerializable, Identifiable
	{
		public IList<int> minionQueue { get; set; }

		public ZoomableBuildingDefinition ZoomableDefinition
		{
			get
			{
				return base.Definition;
			}
		}

		public TikiBarBuilding(TikiBarBuildingDefinition def)
			: base(def)
		{
			minionQueue = new List<int>();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "MINIONQUEUE":
				reader.Read();
				minionQueue = ReaderUtil.PopulateListInt32(reader, minionQueue);
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
			if (minionQueue == null)
			{
				return;
			}
			writer.WritePropertyName("minionQueue");
			writer.WriteStartArray();
			IEnumerator<int> enumerator = minionQueue.GetEnumerator();
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

		public override BuildingObject AddBuildingObject(GameObject gameObject)
		{
			return gameObject.AddComponent<TikiBarBuildingObjectView>();
		}

		public int GetMinionSlotIndex(int characterDefinitionID)
		{
			return minionQueue.IndexOf(characterDefinitionID);
		}

		public int GetOpenSlot()
		{
			for (int i = 0; i < 3; i++)
			{
				if (minionQueue[i] == -1)
				{
					return i;
				}
			}
			return -1;
		}

		public override int GetTransactionID(IDefinitionService definitionService)
		{
			return 0;
		}

		public override bool HasDetailMenuToShow()
		{
			return false;
		}

		public override string GetPrefab(int index = 0)
		{
			if (State == BuildingState.Disabled)
			{
				SetState(BuildingState.MissingTikiSign);
			}
			if (State == BuildingState.MissingTikiSign)
			{
				return base.Definition.noSignPrefab;
			}
			return base.GetPrefab(0);
		}
	}
}
