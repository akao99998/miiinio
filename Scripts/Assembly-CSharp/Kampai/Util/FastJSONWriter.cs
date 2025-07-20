using System.IO;
using Newtonsoft.Json;

namespace Kampai.Util
{
	internal sealed class FastJSONWriter : JsonTextWriter
	{
		private TextWriter writer;

		public FastJSONWriter(TextWriter writer)
			: base(writer)
		{
			this.writer = writer;
		}

		public override void WriteValue(int value)
		{
			AutoComplete(JsonToken.Integer);
			writer.Write(value);
		}

		public override void WriteValue(uint value)
		{
			AutoComplete(JsonToken.Integer);
			writer.Write(value);
		}

		public override void WriteValue(long value)
		{
			AutoComplete(JsonToken.Integer);
			writer.Write(value);
		}

		public override void WriteValue(ulong value)
		{
			AutoComplete(JsonToken.Integer);
			writer.Write(value);
		}

		public override void WriteValue(float value)
		{
			AutoComplete(JsonToken.Float);
			writer.Write(value);
		}

		public override void WriteValue(double value)
		{
			AutoComplete(JsonToken.Float);
			writer.Write(value);
		}

		public override void WriteValue(short value)
		{
			AutoComplete(JsonToken.Integer);
			writer.Write(value);
		}

		public override void WriteValue(ushort value)
		{
			AutoComplete(JsonToken.Integer);
			writer.Write(value);
		}

		public override void WriteValue(byte value)
		{
			AutoComplete(JsonToken.Integer);
			writer.Write(value);
		}

		public override void WriteValue(sbyte value)
		{
			AutoComplete(JsonToken.Integer);
			writer.Write(value);
		}
	}
}
