using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class MinionPartyDefinition : Definition, IBuilder<Instance>
	{
		private Area normalized;

		public override int TypeCode
		{
			get
			{
				return 1116;
			}
		}

		public int UnlockQuestID { get; set; }

		public PartyMeterDefinition partyMeterDefinition { get; set; }

		public IList<MinionPartyLevelBandDefinition> LevelBands { get; set; }

		public CameraControlSettings cameraControlSettings { get; set; }

		public Area PartyArea { get; set; }

		public Location Center { get; set; }

		public float PartyRadius { get; set; }

		public float partyAnimationRestMin { get; set; }

		public float partyAnimationRestMax { get; set; }

		public int PartyAnimations { get; set; }

		public int PreGOHPartyDuration { get; set; }

		public int PartyDuration { get; set; }

		public float Percent { get; set; }

		public int PartyAssetDelay { get; set; }

		public IList<VFXAssetDefinition> startPartyVFX { get; set; }

		public IList<VFXAssetDefinition> endPartyVFX { get; set; }

		public IList<VFXAssetDefinition> partyVFXDefintions { get; set; }

		public int MinionsPlayingAudioCount { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(UnlockQuestID);
			BinarySerializationUtil.WriteObject(writer, partyMeterDefinition);
			BinarySerializationUtil.WriteList(writer, LevelBands);
			BinarySerializationUtil.WriteCameraControlSettings(writer, cameraControlSettings);
			BinarySerializationUtil.WriteArea(writer, PartyArea);
			BinarySerializationUtil.WriteLocation(writer, Center);
			writer.Write(PartyRadius);
			writer.Write(partyAnimationRestMin);
			writer.Write(partyAnimationRestMax);
			writer.Write(PartyAnimations);
			writer.Write(PreGOHPartyDuration);
			writer.Write(PartyDuration);
			writer.Write(Percent);
			writer.Write(PartyAssetDelay);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteVFXAssetDefinition, startPartyVFX);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteVFXAssetDefinition, endPartyVFX);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteVFXAssetDefinition, partyVFXDefintions);
			writer.Write(MinionsPlayingAudioCount);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			UnlockQuestID = reader.ReadInt32();
			partyMeterDefinition = BinarySerializationUtil.ReadObject<PartyMeterDefinition>(reader);
			LevelBands = BinarySerializationUtil.ReadList(reader, LevelBands);
			cameraControlSettings = BinarySerializationUtil.ReadCameraControlSettings(reader);
			PartyArea = BinarySerializationUtil.ReadArea(reader);
			Center = BinarySerializationUtil.ReadLocation(reader);
			PartyRadius = reader.ReadSingle();
			partyAnimationRestMin = reader.ReadSingle();
			partyAnimationRestMax = reader.ReadSingle();
			PartyAnimations = reader.ReadInt32();
			PreGOHPartyDuration = reader.ReadInt32();
			PartyDuration = reader.ReadInt32();
			Percent = reader.ReadSingle();
			PartyAssetDelay = reader.ReadInt32();
			startPartyVFX = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadVFXAssetDefinition, startPartyVFX);
			endPartyVFX = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadVFXAssetDefinition, endPartyVFX);
			partyVFXDefintions = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadVFXAssetDefinition, partyVFXDefintions);
			MinionsPlayingAudioCount = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "UNLOCKQUESTID":
				reader.Read();
				UnlockQuestID = Convert.ToInt32(reader.Value);
				break;
			case "PARTYMETERDEFINITION":
				reader.Read();
				partyMeterDefinition = FastJSONDeserializer.Deserialize<PartyMeterDefinition>(reader, converters);
				break;
			case "LEVELBANDS":
				reader.Read();
				LevelBands = ReaderUtil.PopulateList(reader, converters, LevelBands);
				break;
			case "CAMERACONTROLSETTINGS":
				reader.Read();
				cameraControlSettings = ReaderUtil.ReadCameraControlSettings(reader, converters);
				break;
			case "PARTYAREA":
				reader.Read();
				PartyArea = ReaderUtil.ReadArea(reader, converters);
				break;
			case "CENTER":
				reader.Read();
				Center = ReaderUtil.ReadLocation(reader, converters);
				break;
			case "PARTYRADIUS":
				reader.Read();
				PartyRadius = Convert.ToSingle(reader.Value);
				break;
			case "PARTYANIMATIONRESTMIN":
				reader.Read();
				partyAnimationRestMin = Convert.ToSingle(reader.Value);
				break;
			case "PARTYANIMATIONRESTMAX":
				reader.Read();
				partyAnimationRestMax = Convert.ToSingle(reader.Value);
				break;
			case "PARTYANIMATIONS":
				reader.Read();
				PartyAnimations = Convert.ToInt32(reader.Value);
				break;
			case "PREGOHPARTYDURATION":
				reader.Read();
				PreGOHPartyDuration = Convert.ToInt32(reader.Value);
				break;
			case "PARTYDURATION":
				reader.Read();
				PartyDuration = Convert.ToInt32(reader.Value);
				break;
			case "PERCENT":
				reader.Read();
				Percent = Convert.ToSingle(reader.Value);
				break;
			case "PARTYASSETDELAY":
				reader.Read();
				PartyAssetDelay = Convert.ToInt32(reader.Value);
				break;
			case "STARTPARTYVFX":
				reader.Read();
				startPartyVFX = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadVFXAssetDefinition, startPartyVFX);
				break;
			case "ENDPARTYVFX":
				reader.Read();
				endPartyVFX = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadVFXAssetDefinition, endPartyVFX);
				break;
			case "PARTYVFXDEFINTIONS":
				reader.Read();
				partyVFXDefintions = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadVFXAssetDefinition, partyVFXDefintions);
				break;
			case "MINIONSPLAYINGAUDIOCOUNT":
				reader.Read();
				MinionsPlayingAudioCount = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		private void AssertNormalized()
		{
			if (normalized == null)
			{
				Location a = PartyArea.a;
				Location b = PartyArea.b;
				int x = Math.Min(a.x, b.x);
				int y = Math.Min(a.y, b.y);
				int x2 = Math.Max(a.x, b.x);
				int y2 = Math.Max(a.y, b.y);
				normalized = new Area(x, y, x2, y2);
			}
		}

		public bool Contains(Point point)
		{
			AssertNormalized();
			Location a = normalized.a;
			Location b = normalized.b;
			return point.x >= a.x && point.y >= a.y && point.x <= b.x && point.y <= b.y;
		}

		public int GetPartyDuration(bool partyShouldProduceBuff)
		{
			if (!partyShouldProduceBuff)
			{
				return PreGOHPartyDuration;
			}
			return PartyDuration;
		}

		public Instance Build()
		{
			return new MinionParty(this);
		}
	}
}
