using System.Collections.Generic;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class GetTeamsRequest : IFastJSONSerializable
	{
		[JsonProperty("teamIds")]
		public IList<long> TeamIDs { get; set; }

		public virtual void Serialize(JsonWriter writer)
		{
			writer.WriteStartObject();
			SerializeProperties(writer);
			writer.WriteEndObject();
		}

		protected virtual void SerializeProperties(JsonWriter writer)
		{
			if (TeamIDs == null)
			{
				return;
			}
			writer.WritePropertyName("teamIds");
			writer.WriteStartArray();
			IEnumerator<long> enumerator = TeamIDs.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					long current = enumerator.Current;
					writer.WriteValue(current);
				}
			}
			finally
			{
				enumerator.Dispose();
			}
			writer.WriteEndArray();
		}
	}
}
