using System.Collections.Generic;
using Kampai.Game.View;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class CompositeBuilding : Building<CompositeBuildingDefinition>
	{
		public IList<int> AttachedCompositePieceIDs { get; set; }

		public CompositeBuilding(CompositeBuildingDefinition definition)
			: base(definition)
		{
			AttachedCompositePieceIDs = new List<int>();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "ATTACHEDCOMPOSITEPIECEIDS":
				reader.Read();
				AttachedCompositePieceIDs = ReaderUtil.PopulateListInt32(reader, AttachedCompositePieceIDs);
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
			if (AttachedCompositePieceIDs == null)
			{
				return;
			}
			writer.WritePropertyName("AttachedCompositePieceIDs");
			writer.WriteStartArray();
			IEnumerator<int> enumerator = AttachedCompositePieceIDs.GetEnumerator();
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
			return gameObject.AddComponent<CompositeBuildingObject>();
		}

		public void ShufflePieceIDs()
		{
			int item = AttachedCompositePieceIDs[AttachedCompositePieceIDs.Count - 1];
			AttachedCompositePieceIDs.RemoveAt(AttachedCompositePieceIDs.Count - 1);
			AttachedCompositePieceIDs.Insert(0, item);
		}
	}
}
