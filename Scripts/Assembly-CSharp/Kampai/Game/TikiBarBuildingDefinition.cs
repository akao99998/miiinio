using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class TikiBarBuildingDefinition : TaskableMinionPartyBuildingDefinition, ZoomableBuildingDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1065;
			}
		}

		public Vector3 zoomOffset { get; set; }

		public Vector3 zoomEulers { get; set; }

		public float zoomFOV { get; set; }

		public string noSignPrefab { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteVector3(writer, zoomOffset);
			BinarySerializationUtil.WriteVector3(writer, zoomEulers);
			writer.Write(zoomFOV);
			BinarySerializationUtil.WriteString(writer, noSignPrefab);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			zoomOffset = BinarySerializationUtil.ReadVector3(reader);
			zoomEulers = BinarySerializationUtil.ReadVector3(reader);
			zoomFOV = reader.ReadSingle();
			noSignPrefab = BinarySerializationUtil.ReadString(reader);
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
			case "NOSIGNPREFAB":
				reader.Read();
				noSignPrefab = ReaderUtil.ReadString(reader, converters);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public override Building BuildBuilding()
		{
			return new TikiBarBuilding(this);
		}
	}
}
