using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class MIBBuildingDefinition : BuildingDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1053;
			}
		}

		public int ExpiryInSeconds { get; set; }

		public int CooldownInSeconds { get; set; }

		public bool DisableTapRewards { get; set; }

		public bool DisableReturnRewards { get; set; }

		public int AfterXTapRewards { get; set; }

		public int FirstXTapsWeightedDefinitionId { get; set; }

		public int SecondXTapsWeightedDefinitionId { get; set; }

		public List<UserSegment> ReturnSpenderLevelSegments { get; set; }

		public List<UserSegment> ReturnNonSpenderLevelSegments { get; set; }

		public Vector3 HarvestableIconOffset { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(ExpiryInSeconds);
			writer.Write(CooldownInSeconds);
			writer.Write(DisableTapRewards);
			writer.Write(DisableReturnRewards);
			writer.Write(AfterXTapRewards);
			writer.Write(FirstXTapsWeightedDefinitionId);
			writer.Write(SecondXTapsWeightedDefinitionId);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteUserSegment, ReturnSpenderLevelSegments);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteUserSegment, ReturnNonSpenderLevelSegments);
			BinarySerializationUtil.WriteVector3(writer, HarvestableIconOffset);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			ExpiryInSeconds = reader.ReadInt32();
			CooldownInSeconds = reader.ReadInt32();
			DisableTapRewards = reader.ReadBoolean();
			DisableReturnRewards = reader.ReadBoolean();
			AfterXTapRewards = reader.ReadInt32();
			FirstXTapsWeightedDefinitionId = reader.ReadInt32();
			SecondXTapsWeightedDefinitionId = reader.ReadInt32();
			ReturnSpenderLevelSegments = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadUserSegment, ReturnSpenderLevelSegments);
			ReturnNonSpenderLevelSegments = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadUserSegment, ReturnNonSpenderLevelSegments);
			HarvestableIconOffset = BinarySerializationUtil.ReadVector3(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "EXPIRYINSECONDS":
				reader.Read();
				ExpiryInSeconds = Convert.ToInt32(reader.Value);
				break;
			case "COOLDOWNINSECONDS":
				reader.Read();
				CooldownInSeconds = Convert.ToInt32(reader.Value);
				break;
			case "DISABLETAPREWARDS":
				reader.Read();
				DisableTapRewards = Convert.ToBoolean(reader.Value);
				break;
			case "DISABLERETURNREWARDS":
				reader.Read();
				DisableReturnRewards = Convert.ToBoolean(reader.Value);
				break;
			case "AFTERXTAPREWARDS":
				reader.Read();
				AfterXTapRewards = Convert.ToInt32(reader.Value);
				break;
			case "FIRSTXTAPSWEIGHTEDDEFINITIONID":
				reader.Read();
				FirstXTapsWeightedDefinitionId = Convert.ToInt32(reader.Value);
				break;
			case "SECONDXTAPSWEIGHTEDDEFINITIONID":
				reader.Read();
				SecondXTapsWeightedDefinitionId = Convert.ToInt32(reader.Value);
				break;
			case "RETURNSPENDERLEVELSEGMENTS":
				reader.Read();
				ReturnSpenderLevelSegments = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadUserSegment, ReturnSpenderLevelSegments);
				break;
			case "RETURNNONSPENDERLEVELSEGMENTS":
				reader.Read();
				ReturnNonSpenderLevelSegments = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadUserSegment, ReturnNonSpenderLevelSegments);
				break;
			case "HARVESTABLEICONOFFSET":
				reader.Read();
				HarvestableIconOffset = ReaderUtil.ReadVector3(reader, converters);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public override Building BuildBuilding()
		{
			return new MIBBuilding(this);
		}
	}
}
