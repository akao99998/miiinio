using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class PrestigeDefinition : TaxonomyDefinition, IBuilder<Instance>
	{
		public override int TypeCode
		{
			get
			{
				return 1013;
			}
		}

		public PrestigeType Type { get; set; }

		public uint PreUnlockLevel { get; set; }

		public uint MaxedBadgedOrder { get; set; }

		public uint OrderBoardWeight { get; set; }

		public string CollectionTitle { get; set; }

		public IList<CharacterPrestigeLevelDefinition> PrestigeLevelSettings { get; set; }

		public string UniqueTikiBarstoolASMPatron1 { get; set; }

		public string UniqueTikiBarstoolASMPatron2 { get; set; }

		public string UniqueArrivalStateMachine { get; set; }

		public int SmallAvatarResouceId { get; set; }

		public int WayFinderIconResourceId { get; set; }

		public int BigAvatarResourceId { get; set; }

		public int TrackedDefinitionID { get; set; }

		public int GuestOfHonorDefinitionID { get; set; }

		public int PlayerTrainingNonPrestigeDefinitionId { get; set; }

		public int PlayerTrainingPrestigeDefinitionId { get; set; }

		public int PlayerTrainingReprestigeDefinitionId { get; set; }

		public StickerbookCharacterDisplayableType StickerbookDisplayableType { get; set; }

		public GOHDisplayableType GuestOfHonorDisplayableType { get; set; }

		public int CostumeDefinitionID { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteEnum(writer, Type);
			writer.Write(PreUnlockLevel);
			writer.Write(MaxedBadgedOrder);
			writer.Write(OrderBoardWeight);
			BinarySerializationUtil.WriteString(writer, CollectionTitle);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteCharacterPrestigeLevelDefinition, PrestigeLevelSettings);
			BinarySerializationUtil.WriteString(writer, UniqueTikiBarstoolASMPatron1);
			BinarySerializationUtil.WriteString(writer, UniqueTikiBarstoolASMPatron2);
			BinarySerializationUtil.WriteString(writer, UniqueArrivalStateMachine);
			writer.Write(SmallAvatarResouceId);
			writer.Write(WayFinderIconResourceId);
			writer.Write(BigAvatarResourceId);
			writer.Write(TrackedDefinitionID);
			writer.Write(GuestOfHonorDefinitionID);
			writer.Write(PlayerTrainingNonPrestigeDefinitionId);
			writer.Write(PlayerTrainingPrestigeDefinitionId);
			writer.Write(PlayerTrainingReprestigeDefinitionId);
			BinarySerializationUtil.WriteEnum(writer, StickerbookDisplayableType);
			BinarySerializationUtil.WriteEnum(writer, GuestOfHonorDisplayableType);
			writer.Write(CostumeDefinitionID);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Type = BinarySerializationUtil.ReadEnum<PrestigeType>(reader);
			PreUnlockLevel = reader.ReadUInt32();
			MaxedBadgedOrder = reader.ReadUInt32();
			OrderBoardWeight = reader.ReadUInt32();
			CollectionTitle = BinarySerializationUtil.ReadString(reader);
			PrestigeLevelSettings = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadCharacterPrestigeLevelDefinition, PrestigeLevelSettings);
			UniqueTikiBarstoolASMPatron1 = BinarySerializationUtil.ReadString(reader);
			UniqueTikiBarstoolASMPatron2 = BinarySerializationUtil.ReadString(reader);
			UniqueArrivalStateMachine = BinarySerializationUtil.ReadString(reader);
			SmallAvatarResouceId = reader.ReadInt32();
			WayFinderIconResourceId = reader.ReadInt32();
			BigAvatarResourceId = reader.ReadInt32();
			TrackedDefinitionID = reader.ReadInt32();
			GuestOfHonorDefinitionID = reader.ReadInt32();
			PlayerTrainingNonPrestigeDefinitionId = reader.ReadInt32();
			PlayerTrainingPrestigeDefinitionId = reader.ReadInt32();
			PlayerTrainingReprestigeDefinitionId = reader.ReadInt32();
			StickerbookDisplayableType = BinarySerializationUtil.ReadEnum<StickerbookCharacterDisplayableType>(reader);
			GuestOfHonorDisplayableType = BinarySerializationUtil.ReadEnum<GOHDisplayableType>(reader);
			CostumeDefinitionID = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "TYPE":
				reader.Read();
				Type = ReaderUtil.ReadEnum<PrestigeType>(reader);
				break;
			case "PREUNLOCKLEVEL":
				reader.Read();
				PreUnlockLevel = Convert.ToUInt32(reader.Value);
				break;
			case "MAXEDBADGEDORDER":
				reader.Read();
				MaxedBadgedOrder = Convert.ToUInt32(reader.Value);
				break;
			case "ORDERBOARDWEIGHT":
				reader.Read();
				OrderBoardWeight = Convert.ToUInt32(reader.Value);
				break;
			case "COLLECTIONTITLE":
				reader.Read();
				CollectionTitle = ReaderUtil.ReadString(reader, converters);
				break;
			case "PRESTIGELEVELSETTINGS":
				reader.Read();
				PrestigeLevelSettings = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadCharacterPrestigeLevelDefinition, PrestigeLevelSettings);
				break;
			case "UNIQUETIKIBARSTOOLASMPATRON1":
				reader.Read();
				UniqueTikiBarstoolASMPatron1 = ReaderUtil.ReadString(reader, converters);
				break;
			case "UNIQUETIKIBARSTOOLASMPATRON2":
				reader.Read();
				UniqueTikiBarstoolASMPatron2 = ReaderUtil.ReadString(reader, converters);
				break;
			case "UNIQUEARRIVALSTATEMACHINE":
				reader.Read();
				UniqueArrivalStateMachine = ReaderUtil.ReadString(reader, converters);
				break;
			case "SMALLAVATARRESOUCEID":
				reader.Read();
				SmallAvatarResouceId = Convert.ToInt32(reader.Value);
				break;
			case "WAYFINDERICONRESOURCEID":
				reader.Read();
				WayFinderIconResourceId = Convert.ToInt32(reader.Value);
				break;
			case "BIGAVATARRESOURCEID":
				reader.Read();
				BigAvatarResourceId = Convert.ToInt32(reader.Value);
				break;
			case "TRACKEDDEFINITIONID":
				reader.Read();
				TrackedDefinitionID = Convert.ToInt32(reader.Value);
				break;
			case "GUESTOFHONORDEFINITIONID":
				reader.Read();
				GuestOfHonorDefinitionID = Convert.ToInt32(reader.Value);
				break;
			case "PLAYERTRAININGNONPRESTIGEDEFINITIONID":
				reader.Read();
				PlayerTrainingNonPrestigeDefinitionId = Convert.ToInt32(reader.Value);
				break;
			case "PLAYERTRAININGPRESTIGEDEFINITIONID":
				reader.Read();
				PlayerTrainingPrestigeDefinitionId = Convert.ToInt32(reader.Value);
				break;
			case "PLAYERTRAININGREPRESTIGEDEFINITIONID":
				reader.Read();
				PlayerTrainingReprestigeDefinitionId = Convert.ToInt32(reader.Value);
				break;
			case "STICKERBOOKDISPLAYABLETYPE":
				reader.Read();
				StickerbookDisplayableType = ReaderUtil.ReadEnum<StickerbookCharacterDisplayableType>(reader);
				break;
			case "GUESTOFHONORDISPLAYABLETYPE":
				reader.Read();
				GuestOfHonorDisplayableType = ReaderUtil.ReadEnum<GOHDisplayableType>(reader);
				break;
			case "COSTUMEDEFINITIONID":
				reader.Read();
				CostumeDefinitionID = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public Instance Build()
		{
			return new Prestige(this);
		}
	}
}
