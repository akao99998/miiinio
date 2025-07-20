using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class TSMCharacterDefinition : NamedCharacterDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1083;
			}
		}

		public IList<Vector3> IntroPath { get; set; }

		public float IntroTime { get; set; }

		public int CooldownInSeconds { get; set; }

		public int CooldownMignetteDelayInSeconds { get; set; }

		public int PartyAnimationId { get; set; }

		public string TreasureController { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteVector3, IntroPath);
			writer.Write(IntroTime);
			writer.Write(CooldownInSeconds);
			writer.Write(CooldownMignetteDelayInSeconds);
			writer.Write(PartyAnimationId);
			BinarySerializationUtil.WriteString(writer, TreasureController);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			IntroPath = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadVector3, IntroPath);
			IntroTime = reader.ReadSingle();
			CooldownInSeconds = reader.ReadInt32();
			CooldownMignetteDelayInSeconds = reader.ReadInt32();
			PartyAnimationId = reader.ReadInt32();
			TreasureController = BinarySerializationUtil.ReadString(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "INTROPATH":
				reader.Read();
				IntroPath = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadVector3, IntroPath);
				break;
			case "INTROTIME":
				reader.Read();
				IntroTime = Convert.ToSingle(reader.Value);
				break;
			case "COOLDOWNINSECONDS":
				reader.Read();
				CooldownInSeconds = Convert.ToInt32(reader.Value);
				break;
			case "COOLDOWNMIGNETTEDELAYINSECONDS":
				reader.Read();
				CooldownMignetteDelayInSeconds = Convert.ToInt32(reader.Value);
				break;
			case "PARTYANIMATIONID":
				reader.Read();
				PartyAnimationId = Convert.ToInt32(reader.Value);
				break;
			case "TREASURECONTROLLER":
				reader.Read();
				TreasureController = ReaderUtil.ReadString(reader, converters);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public override Instance Build()
		{
			return new TSMCharacter(this);
		}
	}
}
