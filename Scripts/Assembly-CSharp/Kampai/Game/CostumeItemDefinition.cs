using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class CostumeItemDefinition : ItemDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1090;
			}
		}

		public string Skeleton { get; set; }

		public IList<string> MeshList { get; set; }

		public CharacterUIAnimationDefinition characterUIAnimationDefinition { get; set; }

		public int PartyAnimations { get; set; }

		public CostumeItemDefinition()
		{
			base.Storable = false;
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, Skeleton);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteString, MeshList);
			BinarySerializationUtil.WriteCharacterUIAnimationDefinition(writer, characterUIAnimationDefinition);
			writer.Write(PartyAnimations);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Skeleton = BinarySerializationUtil.ReadString(reader);
			MeshList = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadString, MeshList);
			characterUIAnimationDefinition = BinarySerializationUtil.ReadCharacterUIAnimationDefinition(reader);
			PartyAnimations = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "SKELETON":
				reader.Read();
				Skeleton = ReaderUtil.ReadString(reader, converters);
				break;
			case "MESHLIST":
				reader.Read();
				MeshList = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadString, MeshList);
				break;
			case "CHARACTERUIANIMATIONDEFINITION":
				reader.Read();
				characterUIAnimationDefinition = ReaderUtil.ReadCharacterUIAnimationDefinition(reader, converters);
				break;
			case "PARTYANIMATIONS":
				reader.Read();
				PartyAnimations = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
