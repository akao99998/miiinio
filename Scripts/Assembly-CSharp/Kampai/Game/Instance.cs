using System;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public abstract class Instance<T> : Instance, IFastJSONDeserializable, IFastJSONSerializable, Identifiable where T : Definition
	{
		Definition Instance.Definition
		{
			get
			{
				return Definition;
			}
		}

		public int ID { get; set; }

		public T Definition { get; protected set; }

		protected Instance()
		{
		}

		protected Instance(T definition)
		{
			Definition = definition;
		}

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
				return true;
			default:
				return false;
			}
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
		}

		public void OnDefinitionHotSwap(Definition definition)
		{
			Definition = definition as T;
		}
	}
	[Serializer("FastInstanceSerializationHelper.SerializeInstanceData")]
	[RequiresJsonConverter]
	public interface Instance : IFastJSONDeserializable, IFastJSONSerializable, Identifiable
	{
		Definition Definition { get; }

		void OnDefinitionHotSwap(Definition definition);
	}
}
