using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class BridgeDefinition : ItemDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1088;
			}
		}

		public Location location { get; set; }

		public int TransactionId { get; set; }

		public int BuildingId { get; set; }

		public int RepairedBuildingID { get; set; }

		public int LandExpansionID { get; set; }

		public BridgeScreenPosition cameraPan { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteLocation(writer, location);
			writer.Write(TransactionId);
			writer.Write(BuildingId);
			writer.Write(RepairedBuildingID);
			writer.Write(LandExpansionID);
			BinarySerializationUtil.WriteBridgeScreenPosition(writer, cameraPan);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			location = BinarySerializationUtil.ReadLocation(reader);
			TransactionId = reader.ReadInt32();
			BuildingId = reader.ReadInt32();
			RepairedBuildingID = reader.ReadInt32();
			LandExpansionID = reader.ReadInt32();
			cameraPan = BinarySerializationUtil.ReadBridgeScreenPosition(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "LOCATION":
				reader.Read();
				location = ReaderUtil.ReadLocation(reader, converters);
				break;
			case "TRANSACTIONID":
				reader.Read();
				TransactionId = Convert.ToInt32(reader.Value);
				break;
			case "BUILDINGID":
				reader.Read();
				BuildingId = Convert.ToInt32(reader.Value);
				break;
			case "REPAIREDBUILDINGID":
				reader.Read();
				RepairedBuildingID = Convert.ToInt32(reader.Value);
				break;
			case "LANDEXPANSIONID":
				reader.Read();
				LandExpansionID = Convert.ToInt32(reader.Value);
				break;
			case "CAMERAPAN":
				reader.Read();
				cameraPan = ReaderUtil.ReadBridgeScreenPosition(reader, converters);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
