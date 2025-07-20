using Newtonsoft.Json;

namespace Kampai.Util
{
	public interface IFastJSONSerializable
	{
		void Serialize(JsonWriter writer);
	}
}
