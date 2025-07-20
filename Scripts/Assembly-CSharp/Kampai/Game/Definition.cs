using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public abstract class Definition : IBinarySerializable, IFastJSONDeserializable, Identifiable
	{
		public virtual int TypeCode
		{
			get
			{
				return 1000;
			}
		}

		public string LocalizedKey { get; set; }

		public virtual int ID { get; set; }

		public bool Disabled { get; set; }

		public virtual void Write(BinaryWriter writer)
		{
			BinarySerializationUtil.WriteString(writer, LocalizedKey);
			writer.Write(ID);
			writer.Write(Disabled);
		}

		public virtual void Read(BinaryReader reader)
		{
			LocalizedKey = BinarySerializationUtil.ReadString(reader);
			ID = reader.ReadInt32();
			Disabled = reader.ReadBoolean();
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
			case "LOCALIZEDKEY":
				reader.Read();
				LocalizedKey = ReaderUtil.ReadString(reader, converters);
				break;
			case "ID":
				reader.Read();
				ID = Convert.ToInt32(reader.Value);
				break;
			case "DISABLED":
				reader.Read();
				Disabled = Convert.ToBoolean(reader.Value);
				break;
			default:
				return false;
			}
			return true;
		}

		public override string ToString()
		{
			return string.Format("Defintion TYPE={0} ID={1} KEY={2}", GetType().Name, ID, LocalizedKey);
		}
	}
}
