using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	[RequiresJsonConverter]
	public abstract class NamedCharacterDefinition : TaxonomyDefinition, IBuilder<Instance>, Locatable
	{
		public override int TypeCode
		{
			get
			{
				return 1073;
			}
		}

		public string Prefab { get; set; }

		public Location Location { get; set; }

		public Vector3 RotationEulers { get; set; }

		public NamedCharacterAnimationDefinition CharacterAnimations { get; set; }

		public NamedCharacterType Type { get; set; }

		public int VFXBuildingID { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, Prefab);
			BinarySerializationUtil.WriteLocation(writer, Location);
			BinarySerializationUtil.WriteVector3(writer, RotationEulers);
			BinarySerializationUtil.WriteObject(writer, CharacterAnimations);
			BinarySerializationUtil.WriteEnum(writer, Type);
			writer.Write(VFXBuildingID);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Prefab = BinarySerializationUtil.ReadString(reader);
			Location = BinarySerializationUtil.ReadLocation(reader);
			RotationEulers = BinarySerializationUtil.ReadVector3(reader);
			CharacterAnimations = BinarySerializationUtil.ReadObject<NamedCharacterAnimationDefinition>(reader);
			Type = BinarySerializationUtil.ReadEnum<NamedCharacterType>(reader);
			VFXBuildingID = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "PREFAB":
				reader.Read();
				Prefab = ReaderUtil.ReadString(reader, converters);
				break;
			case "LOCATION":
				reader.Read();
				Location = ReaderUtil.ReadLocation(reader, converters);
				break;
			case "ROTATIONEULERS":
				reader.Read();
				RotationEulers = ReaderUtil.ReadVector3(reader, converters);
				break;
			case "CHARACTERANIMATIONS":
				reader.Read();
				CharacterAnimations = FastJSONDeserializer.Deserialize<NamedCharacterAnimationDefinition>(reader, converters);
				break;
			case "TYPE":
				reader.Read();
				Type = ReaderUtil.ReadEnum<NamedCharacterType>(reader);
				break;
			case "VFXBUILDINGID":
				reader.Read();
				VFXBuildingID = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public abstract Instance Build();
	}
}
