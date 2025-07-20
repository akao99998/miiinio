using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Game;
using Newtonsoft.Json;

namespace Kampai.Util
{
	public class QuantityItem : IEquatable<QuantityItem>, IBinarySerializable, IFastJSONDeserializable, Identifiable
	{
		public virtual int TypeCode
		{
			get
			{
				return 1008;
			}
		}

		public int ID { get; set; }

		public uint Quantity { get; set; }

		public QuantityItem()
		{
		}

		public QuantityItem(int id)
		{
			ID = id;
		}

		public QuantityItem(int id, uint quantity)
		{
			ID = id;
			Quantity = quantity;
		}

		public virtual void Write(BinaryWriter writer)
		{
			writer.Write(ID);
			writer.Write(Quantity);
		}

		public virtual void Read(BinaryReader reader)
		{
			ID = reader.ReadInt32();
			Quantity = reader.ReadUInt32();
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
			default:
			{
				int num;
				if (num == 1)
				{
					reader.Read();
					Quantity = Convert.ToUInt32(reader.Value);
					break;
				}
				return false;
			}
			case "ID":
				reader.Read();
				ID = Convert.ToInt32(reader.Value);
				break;
			}
			return true;
		}

		public static QuantityItem Build(IDictionary<string, object> src, QuantityItem qi = null)
		{
			if (src != null)
			{
				if (qi == null)
				{
					qi = new QuantityItem();
				}
				qi.ID = Convert.ToInt32(src["id"]);
				qi.Quantity = Convert.ToUInt32(src["quantity"]);
				return qi;
			}
			return null;
		}

		public override string ToString()
		{
			return string.Format("ID: {0}, Quantity: {1}", ID, Quantity);
		}

		public string ToString(IDefinitionService definitionService)
		{
			Definition definition;
			return (definitionService.TryGet<Definition>(ID, out definition) && !string.IsNullOrEmpty(definition.LocalizedKey)) ? string.Format("{0}: {1}", definition.LocalizedKey, Quantity) : ToString();
		}

		public override bool Equals(object other)
		{
			return Equals(other as QuantityItem);
		}

		public bool Equals(QuantityItem other)
		{
			return other != null && ID == other.ID && Quantity == other.Quantity;
		}

		public override int GetHashCode()
		{
			int num = 17;
			num = num * 23 + ID.GetHashCode();
			return num * 23 + Quantity.GetHashCode();
		}
	}
}
