using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	[RequiresJsonConverter]
	public abstract class BuildingDefinition : TaxonomyDefinition, IBuilder<Instance>
	{
		public override int TypeCode
		{
			get
			{
				return 1034;
			}
		}

		public BuildingType.BuildingTypeIdentifier Type { get; set; }

		public int FootprintID { get; set; }

		public int PlatformFootprintID { get; set; }

		public ScreenPosition ScreenPosition { get; set; }

		public int ConstructionTime { get; set; }

		public bool Movable { get; set; }

		public int RewardTransactionId { get; set; }

		public virtual string Prefab { get; set; }

		public virtual string Paintover { get; set; }

		public string RevealVFX { get; set; }

		public string ScaffoldingPrefab { get; set; }

		public string RibbonPrefab { get; set; }

		public string PlatformPrefab { get; set; }

		public bool Storable { get; set; }

		public Vector3 QuestIconOffset { get; set; }

		public string MenuPrefab { get; set; }

		public int IncrementalCost { get; set; }

		public int IncrementalConstructionTime { get; set; }

		public bool RouteToSlot { get; set; }

		public int WorkStations { get; set; }

		public string PartyPointsLocalizedKey { get; set; }

		public int PlayerTrainingDefinitionID { get; set; }

		public float UiScale { get; set; }

		public Vector3 UiPosition { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteEnum(writer, Type);
			writer.Write(FootprintID);
			writer.Write(PlatformFootprintID);
			BinarySerializationUtil.WriteScreenPosition(writer, ScreenPosition);
			writer.Write(ConstructionTime);
			writer.Write(Movable);
			writer.Write(RewardTransactionId);
			BinarySerializationUtil.WriteString(writer, Prefab);
			BinarySerializationUtil.WriteString(writer, Paintover);
			BinarySerializationUtil.WriteString(writer, RevealVFX);
			BinarySerializationUtil.WriteString(writer, ScaffoldingPrefab);
			BinarySerializationUtil.WriteString(writer, RibbonPrefab);
			BinarySerializationUtil.WriteString(writer, PlatformPrefab);
			writer.Write(Storable);
			BinarySerializationUtil.WriteVector3(writer, QuestIconOffset);
			BinarySerializationUtil.WriteString(writer, MenuPrefab);
			writer.Write(IncrementalCost);
			writer.Write(IncrementalConstructionTime);
			writer.Write(RouteToSlot);
			writer.Write(WorkStations);
			BinarySerializationUtil.WriteString(writer, PartyPointsLocalizedKey);
			writer.Write(PlayerTrainingDefinitionID);
			writer.Write(UiScale);
			BinarySerializationUtil.WriteVector3(writer, UiPosition);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Type = BinarySerializationUtil.ReadEnum<BuildingType.BuildingTypeIdentifier>(reader);
			FootprintID = reader.ReadInt32();
			PlatformFootprintID = reader.ReadInt32();
			ScreenPosition = BinarySerializationUtil.ReadScreenPosition(reader);
			ConstructionTime = reader.ReadInt32();
			Movable = reader.ReadBoolean();
			RewardTransactionId = reader.ReadInt32();
			Prefab = BinarySerializationUtil.ReadString(reader);
			Paintover = BinarySerializationUtil.ReadString(reader);
			RevealVFX = BinarySerializationUtil.ReadString(reader);
			ScaffoldingPrefab = BinarySerializationUtil.ReadString(reader);
			RibbonPrefab = BinarySerializationUtil.ReadString(reader);
			PlatformPrefab = BinarySerializationUtil.ReadString(reader);
			Storable = reader.ReadBoolean();
			QuestIconOffset = BinarySerializationUtil.ReadVector3(reader);
			MenuPrefab = BinarySerializationUtil.ReadString(reader);
			IncrementalCost = reader.ReadInt32();
			IncrementalConstructionTime = reader.ReadInt32();
			RouteToSlot = reader.ReadBoolean();
			WorkStations = reader.ReadInt32();
			PartyPointsLocalizedKey = BinarySerializationUtil.ReadString(reader);
			PlayerTrainingDefinitionID = reader.ReadInt32();
			UiScale = reader.ReadSingle();
			UiPosition = BinarySerializationUtil.ReadVector3(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "TYPE":
				reader.Read();
				Type = ReaderUtil.ReadEnum<BuildingType.BuildingTypeIdentifier>(reader);
				break;
			case "FOOTPRINTID":
				reader.Read();
				FootprintID = Convert.ToInt32(reader.Value);
				break;
			case "PLATFORMFOOTPRINTID":
				reader.Read();
				PlatformFootprintID = Convert.ToInt32(reader.Value);
				break;
			case "SCREENPOSITION":
				reader.Read();
				ScreenPosition = ReaderUtil.ReadScreenPosition(reader, converters);
				break;
			case "CONSTRUCTIONTIME":
				reader.Read();
				ConstructionTime = Convert.ToInt32(reader.Value);
				break;
			case "MOVABLE":
				reader.Read();
				Movable = Convert.ToBoolean(reader.Value);
				break;
			case "REWARDTRANSACTIONID":
				reader.Read();
				RewardTransactionId = Convert.ToInt32(reader.Value);
				break;
			case "PREFAB":
				reader.Read();
				Prefab = ReaderUtil.ReadString(reader, converters);
				break;
			case "PAINTOVER":
				reader.Read();
				Paintover = ReaderUtil.ReadString(reader, converters);
				break;
			case "REVEALVFX":
				reader.Read();
				RevealVFX = ReaderUtil.ReadString(reader, converters);
				break;
			case "SCAFFOLDINGPREFAB":
				reader.Read();
				ScaffoldingPrefab = ReaderUtil.ReadString(reader, converters);
				break;
			case "RIBBONPREFAB":
				reader.Read();
				RibbonPrefab = ReaderUtil.ReadString(reader, converters);
				break;
			case "PLATFORMPREFAB":
				reader.Read();
				PlatformPrefab = ReaderUtil.ReadString(reader, converters);
				break;
			case "STORABLE":
				reader.Read();
				Storable = Convert.ToBoolean(reader.Value);
				break;
			case "QUESTICONOFFSET":
				reader.Read();
				QuestIconOffset = ReaderUtil.ReadVector3(reader, converters);
				break;
			case "MENUPREFAB":
				reader.Read();
				MenuPrefab = ReaderUtil.ReadString(reader, converters);
				break;
			case "INCREMENTALCOST":
				reader.Read();
				IncrementalCost = Convert.ToInt32(reader.Value);
				break;
			case "INCREMENTALCONSTRUCTIONTIME":
				reader.Read();
				IncrementalConstructionTime = Convert.ToInt32(reader.Value);
				break;
			case "ROUTETOSLOT":
				reader.Read();
				RouteToSlot = Convert.ToBoolean(reader.Value);
				break;
			case "WORKSTATIONS":
				reader.Read();
				WorkStations = Convert.ToInt32(reader.Value);
				break;
			case "PARTYPOINTSLOCALIZEDKEY":
				reader.Read();
				PartyPointsLocalizedKey = ReaderUtil.ReadString(reader, converters);
				break;
			case "PLAYERTRAININGDEFINITIONID":
				reader.Read();
				PlayerTrainingDefinitionID = Convert.ToInt32(reader.Value);
				break;
			case "UISCALE":
				reader.Read();
				UiScale = Convert.ToSingle(reader.Value);
				break;
			case "UIPOSITION":
				reader.Read();
				UiPosition = ReaderUtil.ReadVector3(reader, converters);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public abstract Building BuildBuilding();

		public virtual string GetPrefab(int index = 0)
		{
			return Prefab;
		}

		public Instance Build()
		{
			return BuildBuilding();
		}
	}
}
