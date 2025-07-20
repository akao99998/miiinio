using System.IO;

namespace Kampai.Util
{
	public interface IBinarySerializable
	{
		int TypeCode { get; }

		void Write(BinaryWriter writer);

		void Read(BinaryReader reader);
	}
}
