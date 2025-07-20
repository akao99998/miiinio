using System.Collections.Generic;
using System.IO;
using Kampai.Game;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Splash
{
	public class LoadInTipDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1192;
			}
		}

		public string Text { get; set; }

		public IList<BucketAssignment> Buckets { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, Text);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteBucketAssignment, Buckets);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Text = BinarySerializationUtil.ReadString(reader);
			Buckets = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadBucketAssignment, Buckets);
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
					Buckets = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadBucketAssignment, Buckets);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "TEXT":
				reader.Read();
				Text = ReaderUtil.ReadString(reader, converters);
				break;
			}
			return true;
		}
	}
}
