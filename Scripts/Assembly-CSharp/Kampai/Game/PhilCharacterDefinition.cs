using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class PhilCharacterDefinition : NamedCharacterDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1080;
			}
		}

		public string TikiBarStateMachine { get; set; }

		public IList<Vector3> IntroPath { get; set; }

		public float IntroTime { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, TikiBarStateMachine);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteVector3, IntroPath);
			writer.Write(IntroTime);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			TikiBarStateMachine = BinarySerializationUtil.ReadString(reader);
			IntroPath = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadVector3, IntroPath);
			IntroTime = reader.ReadSingle();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "TIKIBARSTATEMACHINE":
				reader.Read();
				TikiBarStateMachine = ReaderUtil.ReadString(reader, converters);
				break;
			case "INTROPATH":
				reader.Read();
				IntroPath = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadVector3, IntroPath);
				break;
			case "INTROTIME":
				reader.Read();
				IntroTime = Convert.ToSingle(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public override Instance Build()
		{
			return new PhilCharacter(this);
		}
	}
}
