using System;
using Kampai.Game.View;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class ConnectableBuilding : Building<ConnectableBuildingDefinition>
	{
		public ConnectableBuildingPieceType pieceType { get; set; }

		public int rotation { get; set; }

		public ConnectableBuilding(ConnectableBuildingDefinition def)
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
					rotation = Convert.ToInt32(reader.Value);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "PIECETYPE":
				reader.Read();
				pieceType = ReaderUtil.ReadEnum<ConnectableBuildingPieceType>(reader);
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
			writer.WritePropertyName("pieceType");
			writer.WriteValue((int)pieceType);
			writer.WritePropertyName("rotation");
			writer.WriteValue(rotation);
		}

		public override BuildingObject AddBuildingObject(GameObject gameObject)
		{
			return gameObject.AddComponent<ConnectableBuildingObject>();
		}
	}
}
