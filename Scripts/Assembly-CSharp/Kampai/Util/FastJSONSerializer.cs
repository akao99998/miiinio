using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Kampai.Util
{
	public static class FastJSONSerializer
	{
		private static UTF8Encoding utf8NoBOM = new UTF8Encoding(false);

		private static JsonSerializer serializer = JsonSerializer.Create(null);

		public static void Serialize(object obj, JsonWriter writer)
		{
			IFastJSONSerializable fastJSONSerializable = obj as IFastJSONSerializable;
			if (fastJSONSerializable != null)
			{
				fastJSONSerializable.Serialize(writer);
			}
			else
			{
				serializer.Serialize(writer, obj);
			}
		}

		public static string Serialize(object obj)
		{
			return Encoding.UTF8.GetString(SerializeUTF8(obj));
		}

		public static byte[] SerializeUTF8(object obj)
		{
			using (MemoryStream memoryStream = new MemoryStream(128))
			{
				using (TextWriter textWriter = new FastTextStreamWriter(memoryStream, utf8NoBOM))
				{
					using (JsonWriter writer = new FastJSONWriter(textWriter))
					{
						Serialize(obj, writer);
						textWriter.Flush();
						return memoryStream.ToArray();
					}
				}
			}
		}
	}
}
