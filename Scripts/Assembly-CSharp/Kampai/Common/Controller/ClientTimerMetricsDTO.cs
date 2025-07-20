using System;
using System.Collections.Generic;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Common.Controller
{
	public class ClientTimerMetricsDTO : IFastJSONSerializable
	{
		public Dictionary<string, float> timerEvents { get; set; }

		public virtual void Serialize(JsonWriter writer)
		{
			writer.WriteStartObject();
			SerializeProperties(writer);
			writer.WriteEndObject();
		}

		protected virtual void SerializeProperties(JsonWriter writer)
		{
			if (timerEvents == null)
			{
				return;
			}
			writer.WritePropertyName("timerEvents");
			writer.WriteStartObject();
			Dictionary<string, float>.Enumerator enumerator = timerEvents.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<string, float> current = enumerator.Current;
					writer.WritePropertyName(Convert.ToString(current.Key));
					writer.WriteValue(current.Value);
				}
			}
			finally
			{
				enumerator.Dispose();
			}
			writer.WriteEndObject();
		}
	}
}
