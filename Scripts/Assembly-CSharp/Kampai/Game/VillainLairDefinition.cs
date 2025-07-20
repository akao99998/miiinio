using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class VillainLairDefinition : Definition, Locatable
	{
		public override int TypeCode
		{
			get
			{
				return 1066;
			}
		}

		public int CustomCameraPositionDefinitionId { get; set; }

		public Vector3 KevinOffset { get; set; }

		public float KevinRotation { get; set; }

		public Vector3 VillainOffset { get; set; }

		public float VillainRotation { get; set; }

		public string IntroAnimController { get; set; }

		public Location Location { get; set; }

		public string Prefab { get; set; }

		public Location MinionArrivalOffset { get; set; }

		public int ResourceBuildingDefID { get; set; }

		public int ResourceItemID { get; set; }

		public List<PlatformDefinition> Platforms { get; set; }

		public List<ResourcePlotDefinition> ResourcePlots { get; set; }

		public int SecondsToHarvest { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(CustomCameraPositionDefinitionId);
			BinarySerializationUtil.WriteVector3(writer, KevinOffset);
			writer.Write(KevinRotation);
			BinarySerializationUtil.WriteVector3(writer, VillainOffset);
			writer.Write(VillainRotation);
			BinarySerializationUtil.WriteString(writer, IntroAnimController);
			BinarySerializationUtil.WriteLocation(writer, Location);
			BinarySerializationUtil.WriteString(writer, Prefab);
			BinarySerializationUtil.WriteLocation(writer, MinionArrivalOffset);
			writer.Write(ResourceBuildingDefID);
			writer.Write(ResourceItemID);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WritePlatformDefinition, Platforms);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteResourcePlotDefinition, ResourcePlots);
			writer.Write(SecondsToHarvest);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			CustomCameraPositionDefinitionId = reader.ReadInt32();
			KevinOffset = BinarySerializationUtil.ReadVector3(reader);
			KevinRotation = reader.ReadSingle();
			VillainOffset = BinarySerializationUtil.ReadVector3(reader);
			VillainRotation = reader.ReadSingle();
			IntroAnimController = BinarySerializationUtil.ReadString(reader);
			Location = BinarySerializationUtil.ReadLocation(reader);
			Prefab = BinarySerializationUtil.ReadString(reader);
			MinionArrivalOffset = BinarySerializationUtil.ReadLocation(reader);
			ResourceBuildingDefID = reader.ReadInt32();
			ResourceItemID = reader.ReadInt32();
			Platforms = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadPlatformDefinition, Platforms);
			ResourcePlots = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadResourcePlotDefinition, ResourcePlots);
			SecondsToHarvest = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "CUSTOMCAMERAPOSITIONDEFINITIONID":
				reader.Read();
				CustomCameraPositionDefinitionId = Convert.ToInt32(reader.Value);
				break;
			case "KEVINOFFSET":
				reader.Read();
				KevinOffset = ReaderUtil.ReadVector3(reader, converters);
				break;
			case "KEVINROTATION":
				reader.Read();
				KevinRotation = Convert.ToSingle(reader.Value);
				break;
			case "VILLAINOFFSET":
				reader.Read();
				VillainOffset = ReaderUtil.ReadVector3(reader, converters);
				break;
			case "VILLAINROTATION":
				reader.Read();
				VillainRotation = Convert.ToSingle(reader.Value);
				break;
			case "INTROANIMCONTROLLER":
				reader.Read();
				IntroAnimController = ReaderUtil.ReadString(reader, converters);
				break;
			case "LOCATION":
				reader.Read();
				Location = ReaderUtil.ReadLocation(reader, converters);
				break;
			case "PREFAB":
				reader.Read();
				Prefab = ReaderUtil.ReadString(reader, converters);
				break;
			case "MINIONARRIVALOFFSET":
				reader.Read();
				MinionArrivalOffset = ReaderUtil.ReadLocation(reader, converters);
				break;
			case "RESOURCEBUILDINGDEFID":
				reader.Read();
				ResourceBuildingDefID = Convert.ToInt32(reader.Value);
				break;
			case "RESOURCEITEMID":
				reader.Read();
				ResourceItemID = Convert.ToInt32(reader.Value);
				break;
			case "PLATFORMS":
				reader.Read();
				Platforms = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadPlatformDefinition, Platforms);
				break;
			case "RESOURCEPLOTS":
				reader.Read();
				ResourcePlots = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadResourcePlotDefinition, ResourcePlots);
				break;
			case "SECONDSTOHARVEST":
				reader.Read();
				SecondsToHarvest = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public Instance Build()
		{
			return new VillainLair(this);
		}
	}
}
