using System;

namespace Kampai.Util
{
	public class BinarySerializationException : Exception
	{
		public BinarySerializationException()
		{
		}

		public BinarySerializationException(string message)
			: base(message)
		{
		}

		public BinarySerializationException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
