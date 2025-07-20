using System.IO;
using System.Text;

namespace Kampai.Util
{
	internal sealed class FastTextStreamWriter : StreamWriter
	{
		private FastDoubleToString doubleConverter = new FastDoubleToString();

		private FastIntToString intConverter = new FastIntToString();

		public FastTextStreamWriter(Stream stream)
			: base(stream)
		{
		}

		public FastTextStreamWriter(Stream stream, Encoding encoding)
			: base(stream, encoding)
		{
		}

		public FastTextStreamWriter(Stream stream, Encoding encoding, int bufferSize)
			: base(stream, encoding, bufferSize)
		{
		}

		public override void Write(double value)
		{
			char[] buffer;
			int len;
			doubleConverter.ToCharArray(value, out buffer, out len);
			Write(buffer, 0, len);
		}

		public override void Write(int value)
		{
			char[] buffer;
			int len;
			intConverter.ToCharArray(value, out buffer, out len);
			Write(buffer, 0, len);
		}

		public override void Write(long value)
		{
			char[] buffer;
			int len;
			intConverter.ToCharArray(value, out buffer, out len);
			Write(buffer, 0, len);
		}

		public override void Write(float value)
		{
			char[] buffer;
			int len;
			doubleConverter.ToCharArray(value, out buffer, out len);
			Write(buffer, 0, len);
		}

		public override void Write(uint value)
		{
			char[] buffer;
			int len;
			intConverter.ToCharArray(value, out buffer, out len);
			Write(buffer, 0, len);
		}

		public override void Write(ulong value)
		{
			char[] buffer;
			int len;
			intConverter.ToCharArray(value, out buffer, out len);
			Write(buffer, 0, len);
		}
	}
}
