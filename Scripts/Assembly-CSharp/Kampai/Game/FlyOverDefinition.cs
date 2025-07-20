using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class FlyOverDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1087;
			}
		}

		public float time { get; set; }

		public IList<FlyOverNode> path { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(time);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteFlyOverNode, path);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			time = reader.ReadSingle();
			path = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadFlyOverNode, path);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			default:
			{
				int num;
				if (num == 1)
				{
					reader.Read();
					path = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadFlyOverNode, path);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "TIME":
				reader.Read();
				time = Convert.ToSingle(reader.Value);
				break;
			}
			return true;
		}
	}
}
