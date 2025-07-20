using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class LandExpansionConfig : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1099;
			}
		}

		public int expansionId { get; set; }

		public IList<int> adjacentExpansionIds { get; set; }

		public IList<int> containedDebris { get; set; }

		public IList<int> containedAspirationalBuildings { get; set; }

		public int transactionId { get; set; }

		public Location routingSlot { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(expansionId);
			BinarySerializationUtil.WriteListInt32(writer, adjacentExpansionIds);
			BinarySerializationUtil.WriteListInt32(writer, containedDebris);
			BinarySerializationUtil.WriteListInt32(writer, containedAspirationalBuildings);
			writer.Write(transactionId);
			BinarySerializationUtil.WriteLocation(writer, routingSlot);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			expansionId = reader.ReadInt32();
			adjacentExpansionIds = BinarySerializationUtil.ReadListInt32(reader, adjacentExpansionIds);
			containedDebris = BinarySerializationUtil.ReadListInt32(reader, containedDebris);
			containedAspirationalBuildings = BinarySerializationUtil.ReadListInt32(reader, containedAspirationalBuildings);
			transactionId = reader.ReadInt32();
			routingSlot = BinarySerializationUtil.ReadLocation(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "EXPANSIONID":
				reader.Read();
				expansionId = Convert.ToInt32(reader.Value);
				break;
			case "ADJACENTEXPANSIONIDS":
				reader.Read();
				adjacentExpansionIds = ReaderUtil.PopulateListInt32(reader, adjacentExpansionIds);
				break;
			case "CONTAINEDDEBRIS":
				reader.Read();
				containedDebris = ReaderUtil.PopulateListInt32(reader, containedDebris);
				break;
			case "CONTAINEDASPIRATIONALBUILDINGS":
				reader.Read();
				containedAspirationalBuildings = ReaderUtil.PopulateListInt32(reader, containedAspirationalBuildings);
				break;
			case "TRANSACTIONID":
				reader.Read();
				transactionId = Convert.ToInt32(reader.Value);
				break;
			case "ROUTINGSLOT":
				reader.Read();
				routingSlot = ReaderUtil.ReadLocation(reader, converters);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
