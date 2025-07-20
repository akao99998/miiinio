using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class StageBuildingDefinition : AnimatingBuildingDefinition, ZoomableBuildingDefinition
	{
		private Area normalizedArea;

		public override int TypeCode
		{
			get
			{
				return 1062;
			}
		}

		public Vector3 zoomOffset { get; set; }

		public Vector3 zoomEulers { get; set; }

		public float zoomFOV { get; set; }

		public string backdropPrefabName { get; set; }

		public int temporaryMinionNum { get; set; }

		public int temporaryMinionAnimationCount { get; set; }

		public float temporaryMinionsOffset { get; set; }

		public string temporaryMinionASM { get; set; }

		public string AspirationalMessage { get; set; }

		public int SocialEventMinimumLevel { get; set; }

		public Area concertArea { get; set; }

		public float maxMinionOffsetX { get; set; }

		public float maxMinionOffsetY { get; set; }

		public float posSkipPercent { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteVector3(writer, zoomOffset);
			BinarySerializationUtil.WriteVector3(writer, zoomEulers);
			writer.Write(zoomFOV);
			BinarySerializationUtil.WriteString(writer, backdropPrefabName);
			writer.Write(temporaryMinionNum);
			writer.Write(temporaryMinionAnimationCount);
			writer.Write(temporaryMinionsOffset);
			BinarySerializationUtil.WriteString(writer, temporaryMinionASM);
			BinarySerializationUtil.WriteString(writer, AspirationalMessage);
			writer.Write(SocialEventMinimumLevel);
			BinarySerializationUtil.WriteArea(writer, concertArea);
			writer.Write(maxMinionOffsetX);
			writer.Write(maxMinionOffsetY);
			writer.Write(posSkipPercent);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			zoomOffset = BinarySerializationUtil.ReadVector3(reader);
			zoomEulers = BinarySerializationUtil.ReadVector3(reader);
			zoomFOV = reader.ReadSingle();
			backdropPrefabName = BinarySerializationUtil.ReadString(reader);
			temporaryMinionNum = reader.ReadInt32();
			temporaryMinionAnimationCount = reader.ReadInt32();
			temporaryMinionsOffset = reader.ReadSingle();
			temporaryMinionASM = BinarySerializationUtil.ReadString(reader);
			AspirationalMessage = BinarySerializationUtil.ReadString(reader);
			SocialEventMinimumLevel = reader.ReadInt32();
			concertArea = BinarySerializationUtil.ReadArea(reader);
			maxMinionOffsetX = reader.ReadSingle();
			maxMinionOffsetY = reader.ReadSingle();
			posSkipPercent = reader.ReadSingle();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "ZOOMOFFSET":
				reader.Read();
				zoomOffset = ReaderUtil.ReadVector3(reader, converters);
				break;
			case "ZOOMEULERS":
				reader.Read();
				zoomEulers = ReaderUtil.ReadVector3(reader, converters);
				break;
			case "ZOOMFOV":
				reader.Read();
				zoomFOV = Convert.ToSingle(reader.Value);
				break;
			case "BACKDROPPREFABNAME":
				reader.Read();
				backdropPrefabName = ReaderUtil.ReadString(reader, converters);
				break;
			case "TEMPORARYMINIONNUM":
				reader.Read();
				temporaryMinionNum = Convert.ToInt32(reader.Value);
				break;
			case "TEMPORARYMINIONANIMATIONCOUNT":
				reader.Read();
				temporaryMinionAnimationCount = Convert.ToInt32(reader.Value);
				break;
			case "TEMPORARYMINIONSOFFSET":
				reader.Read();
				temporaryMinionsOffset = Convert.ToSingle(reader.Value);
				break;
			case "TEMPORARYMINIONASM":
				reader.Read();
				temporaryMinionASM = ReaderUtil.ReadString(reader, converters);
				break;
			case "ASPIRATIONALMESSAGE":
				reader.Read();
				AspirationalMessage = ReaderUtil.ReadString(reader, converters);
				break;
			case "SOCIALEVENTMINIMUMLEVEL":
				reader.Read();
				SocialEventMinimumLevel = Convert.ToInt32(reader.Value);
				break;
			case "CONCERTAREA":
				reader.Read();
				concertArea = ReaderUtil.ReadArea(reader, converters);
				break;
			case "MAXMINIONOFFSETX":
				reader.Read();
				maxMinionOffsetX = Convert.ToSingle(reader.Value);
				break;
			case "MAXMINIONOFFSETY":
				reader.Read();
				maxMinionOffsetY = Convert.ToSingle(reader.Value);
				break;
			case "POSSKIPPERCENT":
				reader.Read();
				posSkipPercent = Convert.ToSingle(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public override Building BuildBuilding()
		{
			return new StageBuilding(this);
		}

		private bool AssertNormalized()
		{
			if (normalizedArea == null)
			{
				if (concertArea == null)
				{
					return false;
				}
				Location a = concertArea.a;
				Location b = concertArea.b;
				int x = Math.Min(a.x, b.x);
				int y = Math.Min(a.y, b.y);
				int x2 = Math.Max(a.x, b.x);
				int y2 = Math.Max(a.y, b.y);
				normalizedArea = new Area(x, y, x2, y2);
			}
			return true;
		}

		public bool Contains(Point point)
		{
			if (!AssertNormalized())
			{
				return false;
			}
			Location a = normalizedArea.a;
			Location b = normalizedArea.b;
			return point.x >= a.x && point.y >= a.y && point.x <= b.x && point.y <= b.y;
		}
	}
}
