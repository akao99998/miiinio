using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class MignetteBuildingDefinition : TaskableBuildingDefinition
	{
		public string CollectableImage;

		public string CollectableImageMask;

		public IList<MignetteRuleDefinition> MignetteRules;

		public IList<MignetteChildObjectDefinition> ChildObjects;

		public IList<MignetteChildObjectDefinition> CooldownObjects;

		public override int TypeCode
		{
			get
			{
				return 1056;
			}
		}

		public bool ShowPlayConfirmMenu { get; set; }

		public bool ShowMignetteHUD { get; set; }

		public string ContextRootName { get; set; }

		public int CooldownInSeconds { get; set; }

		public int LevelUnlocked { get; set; }

		public float XPRewardFactor { get; set; }

		public IList<int> MainCollectionDefinitionIDs { get; set; }

		public IList<int> RepeatableCollectionDefinitionIDs { get; set; }

		public string AspirationalMessage { get; set; }

		public int LandExpansionID { get; set; }

		public MignetteBuildingDefinition()
		{
			ShowPlayConfirmMenu = false;
			ShowMignetteHUD = true;
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(ShowPlayConfirmMenu);
			writer.Write(ShowMignetteHUD);
			BinarySerializationUtil.WriteString(writer, ContextRootName);
			writer.Write(CooldownInSeconds);
			writer.Write(LevelUnlocked);
			writer.Write(XPRewardFactor);
			BinarySerializationUtil.WriteListInt32(writer, MainCollectionDefinitionIDs);
			BinarySerializationUtil.WriteListInt32(writer, RepeatableCollectionDefinitionIDs);
			BinarySerializationUtil.WriteString(writer, AspirationalMessage);
			writer.Write(LandExpansionID);
			BinarySerializationUtil.WriteString(writer, CollectableImage);
			BinarySerializationUtil.WriteString(writer, CollectableImageMask);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteMignetteRuleDefinition, MignetteRules);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteMignetteChildObjectDefinition, ChildObjects);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteMignetteChildObjectDefinition, CooldownObjects);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			ShowPlayConfirmMenu = reader.ReadBoolean();
			ShowMignetteHUD = reader.ReadBoolean();
			ContextRootName = BinarySerializationUtil.ReadString(reader);
			CooldownInSeconds = reader.ReadInt32();
			LevelUnlocked = reader.ReadInt32();
			XPRewardFactor = reader.ReadSingle();
			MainCollectionDefinitionIDs = BinarySerializationUtil.ReadListInt32(reader, MainCollectionDefinitionIDs);
			RepeatableCollectionDefinitionIDs = BinarySerializationUtil.ReadListInt32(reader, RepeatableCollectionDefinitionIDs);
			AspirationalMessage = BinarySerializationUtil.ReadString(reader);
			LandExpansionID = reader.ReadInt32();
			CollectableImage = BinarySerializationUtil.ReadString(reader);
			CollectableImageMask = BinarySerializationUtil.ReadString(reader);
			MignetteRules = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadMignetteRuleDefinition, MignetteRules);
			ChildObjects = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadMignetteChildObjectDefinition, ChildObjects);
			CooldownObjects = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadMignetteChildObjectDefinition, CooldownObjects);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "SHOWPLAYCONFIRMMENU":
				reader.Read();
				ShowPlayConfirmMenu = Convert.ToBoolean(reader.Value);
				break;
			case "SHOWMIGNETTEHUD":
				reader.Read();
				ShowMignetteHUD = Convert.ToBoolean(reader.Value);
				break;
			case "CONTEXTROOTNAME":
				reader.Read();
				ContextRootName = ReaderUtil.ReadString(reader, converters);
				break;
			case "COOLDOWNINSECONDS":
				reader.Read();
				CooldownInSeconds = Convert.ToInt32(reader.Value);
				break;
			case "LEVELUNLOCKED":
				reader.Read();
				LevelUnlocked = Convert.ToInt32(reader.Value);
				break;
			case "XPREWARDFACTOR":
				reader.Read();
				XPRewardFactor = Convert.ToSingle(reader.Value);
				break;
			case "MAINCOLLECTIONDEFINITIONIDS":
				reader.Read();
				MainCollectionDefinitionIDs = ReaderUtil.PopulateListInt32(reader, MainCollectionDefinitionIDs);
				break;
			case "REPEATABLECOLLECTIONDEFINITIONIDS":
				reader.Read();
				RepeatableCollectionDefinitionIDs = ReaderUtil.PopulateListInt32(reader, RepeatableCollectionDefinitionIDs);
				break;
			case "ASPIRATIONALMESSAGE":
				reader.Read();
				AspirationalMessage = ReaderUtil.ReadString(reader, converters);
				break;
			case "LANDEXPANSIONID":
				reader.Read();
				LandExpansionID = Convert.ToInt32(reader.Value);
				break;
			case "COLLECTABLEIMAGE":
				reader.Read();
				CollectableImage = ReaderUtil.ReadString(reader, converters);
				break;
			case "COLLECTABLEIMAGEMASK":
				reader.Read();
				CollectableImageMask = ReaderUtil.ReadString(reader, converters);
				break;
			case "MIGNETTERULES":
				reader.Read();
				MignetteRules = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadMignetteRuleDefinition, MignetteRules);
				break;
			case "CHILDOBJECTS":
				reader.Read();
				ChildObjects = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadMignetteChildObjectDefinition, ChildObjects);
				break;
			case "COOLDOWNOBJECTS":
				reader.Read();
				CooldownObjects = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadMignetteChildObjectDefinition, CooldownObjects);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public override Building BuildBuilding()
		{
			return new MignetteBuilding(this);
		}
	}
}
