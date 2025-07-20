using System;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class QuantityInstance : Instance, IFastJSONDeserializable, IFastJSONSerializable, Identifiable
	{
		public int ID { get; set; }

		public uint Quantity { get; set; }

		[Deserializer("ReaderUtil.ReaderNotImplemented<Definition>")]
		public Definition Definition { get; set; }

		public virtual object Deserialize(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			ReaderUtil.EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
				{
					string propertyName = ((string)reader.Value).ToUpper();
					if (!DeserializeProperty(propertyName, reader, converters))
					{
						reader.Skip();
					}
					break;
				}
				case JsonToken.EndObject:
					return this;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, ReaderUtil.GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		protected virtual bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "ID":
				reader.Read();
				ID = Convert.ToInt32(reader.Value);
				break;
			case "QUANTITY":
				reader.Read();
				Quantity = Convert.ToUInt32(reader.Value);
				break;
			case "DEFINITION":
				reader.Read();
				Definition = ReaderUtil.ReaderNotImplemented<Definition>(reader, converters);
				break;
			default:
				return false;
			}
			return true;
		}

		public virtual void Serialize(JsonWriter writer)
		{
			writer.WriteStartObject();
			SerializeProperties(writer);
			writer.WriteEndObject();
		}

		protected virtual void SerializeProperties(JsonWriter writer)
		{
			FastInstanceSerializationHelper.SerializeInstanceData(writer, this);
			writer.WritePropertyName("Quantity");
			writer.WriteValue(Quantity);
		}

		public void OnDefinitionHotSwap(Definition definition)
		{
			Definition = definition;
		}
	}
}
