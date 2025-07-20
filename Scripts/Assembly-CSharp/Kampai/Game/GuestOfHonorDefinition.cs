using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class GuestOfHonorDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1115;
			}
		}

		public List<int> buffDefinitionIDs { get; set; }

		public List<int> buffStarValues { get; set; }

		public int partyDurationBoost { get; set; }

		public float partyDurationMultipler { get; set; }

		public int availableInvites { get; set; }

		public int cooldown { get; set; }

		public int rushCostPerParty { get; set; }

		public int gohAnimationID { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteListInt32(writer, buffDefinitionIDs);
			BinarySerializationUtil.WriteListInt32(writer, buffStarValues);
			writer.Write(partyDurationBoost);
			writer.Write(partyDurationMultipler);
			writer.Write(availableInvites);
			writer.Write(cooldown);
			writer.Write(rushCostPerParty);
			writer.Write(gohAnimationID);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			buffDefinitionIDs = BinarySerializationUtil.ReadListInt32(reader, buffDefinitionIDs);
			buffStarValues = BinarySerializationUtil.ReadListInt32(reader, buffStarValues);
			partyDurationBoost = reader.ReadInt32();
			partyDurationMultipler = reader.ReadSingle();
			availableInvites = reader.ReadInt32();
			cooldown = reader.ReadInt32();
			rushCostPerParty = reader.ReadInt32();
			gohAnimationID = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "BUFFDEFINITIONIDS":
				reader.Read();
				buffDefinitionIDs = ReaderUtil.PopulateListInt32(reader, buffDefinitionIDs);
				break;
			case "BUFFSTARVALUES":
				reader.Read();
				buffStarValues = ReaderUtil.PopulateListInt32(reader, buffStarValues);
				break;
			case "PARTYDURATIONBOOST":
				reader.Read();
				partyDurationBoost = Convert.ToInt32(reader.Value);
				break;
			case "PARTYDURATIONMULTIPLER":
				reader.Read();
				partyDurationMultipler = Convert.ToSingle(reader.Value);
				break;
			case "AVAILABLEINVITES":
				reader.Read();
				availableInvites = Convert.ToInt32(reader.Value);
				break;
			case "COOLDOWN":
				reader.Read();
				cooldown = Convert.ToInt32(reader.Value);
				break;
			case "RUSHCOSTPERPARTY":
				reader.Read();
				rushCostPerParty = Convert.ToInt32(reader.Value);
				break;
			case "GOHANIMATIONID":
				reader.Read();
				gohAnimationID = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
