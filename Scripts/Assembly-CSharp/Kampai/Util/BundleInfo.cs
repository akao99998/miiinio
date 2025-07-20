using System;
using Newtonsoft.Json;

namespace Kampai.Util
{
	public class BundleInfo : IFastJSONDeserializable
	{
		public string name { get; set; }

		public string originalName { get; set; }

		public int tier { get; set; }

		public string sum { get; set; }

		public ulong size { get; set; }

		public bool shared { get; set; }

		public bool shaders { get; set; }

		public bool audio { get; set; }

		public bool isZipped { get; set; }

		public ulong zipsize { get; set; }

		public string zipsum { get; set; }

		public bool isPackaged { get; set; }

		public bool isStreamable
		{
			get
			{
				return isPackaged && !isZipped;
			}
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
			case "NAME":
				reader.Read();
				name = ReaderUtil.ReadString(reader, converters);
				break;
			case "ORIGINALNAME":
				reader.Read();
				originalName = ReaderUtil.ReadString(reader, converters);
				break;
			case "TIER":
				reader.Read();
				tier = Convert.ToInt32(reader.Value);
				break;
			case "SUM":
				reader.Read();
				sum = ReaderUtil.ReadString(reader, converters);
				break;
			case "SIZE":
				reader.Read();
				size = Convert.ToUInt64(reader.Value);
				break;
			case "SHARED":
				reader.Read();
				shared = Convert.ToBoolean(reader.Value);
				break;
			case "SHADERS":
				reader.Read();
				shaders = Convert.ToBoolean(reader.Value);
				break;
			case "AUDIO":
				reader.Read();
				audio = Convert.ToBoolean(reader.Value);
				break;
			case "ISZIPPED":
				reader.Read();
				isZipped = Convert.ToBoolean(reader.Value);
				break;
			case "ZIPSIZE":
				reader.Read();
				zipsize = Convert.ToUInt64(reader.Value);
				break;
			case "ZIPSUM":
				reader.Read();
				zipsum = ReaderUtil.ReadString(reader, converters);
				break;
			case "ISPACKAGED":
				reader.Read();
				isPackaged = Convert.ToBoolean(reader.Value);
				break;
			default:
				return false;
			}
			return true;
		}
	}
}
