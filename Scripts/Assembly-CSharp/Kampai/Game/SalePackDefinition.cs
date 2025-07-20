using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	[RequiresJsonConverter]
	public class SalePackDefinition : PackDefinition, IBuilder<Instance>, IUTCRangeable
	{
		public override int TypeCode
		{
			get
			{
				return 1136;
			}
		}

		public SalePackType Type { get; set; }

		public int UTCStartDate { get; set; }

		public int UTCEndDate { get; set; }

		public int Impressions { get; set; }

		public int ImpressionInterval { get; set; }

		public int Duration { get; set; }

		public SalePackMessageType MessageType { get; set; }

		public SalePackMessageLinkType MessageLinkType { get; set; }

		public string MessageImage { get; set; }

		public string MessageMask { get; set; }

		public string GlassIconImage { get; set; }

		public string GlassIconMask { get; set; }

		public string MessageUrl { get; set; }

		public string ServerSaleId { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteEnum(writer, Type);
			writer.Write(UTCStartDate);
			writer.Write(UTCEndDate);
			writer.Write(Impressions);
			writer.Write(ImpressionInterval);
			writer.Write(Duration);
			BinarySerializationUtil.WriteEnum(writer, MessageType);
			BinarySerializationUtil.WriteEnum(writer, MessageLinkType);
			BinarySerializationUtil.WriteString(writer, MessageImage);
			BinarySerializationUtil.WriteString(writer, MessageMask);
			BinarySerializationUtil.WriteString(writer, GlassIconImage);
			BinarySerializationUtil.WriteString(writer, GlassIconMask);
			BinarySerializationUtil.WriteString(writer, MessageUrl);
			BinarySerializationUtil.WriteString(writer, ServerSaleId);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Type = BinarySerializationUtil.ReadEnum<SalePackType>(reader);
			UTCStartDate = reader.ReadInt32();
			UTCEndDate = reader.ReadInt32();
			Impressions = reader.ReadInt32();
			ImpressionInterval = reader.ReadInt32();
			Duration = reader.ReadInt32();
			MessageType = BinarySerializationUtil.ReadEnum<SalePackMessageType>(reader);
			MessageLinkType = BinarySerializationUtil.ReadEnum<SalePackMessageLinkType>(reader);
			MessageImage = BinarySerializationUtil.ReadString(reader);
			MessageMask = BinarySerializationUtil.ReadString(reader);
			GlassIconImage = BinarySerializationUtil.ReadString(reader);
			GlassIconMask = BinarySerializationUtil.ReadString(reader);
			MessageUrl = BinarySerializationUtil.ReadString(reader);
			ServerSaleId = BinarySerializationUtil.ReadString(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "TYPE":
				reader.Read();
				Type = ReaderUtil.ReadEnum<SalePackType>(reader);
				break;
			case "UTCSTARTDATE":
				reader.Read();
				UTCStartDate = Convert.ToInt32(reader.Value);
				break;
			case "UTCENDDATE":
				reader.Read();
				UTCEndDate = Convert.ToInt32(reader.Value);
				break;
			case "IMPRESSIONS":
				reader.Read();
				Impressions = Convert.ToInt32(reader.Value);
				break;
			case "IMPRESSIONINTERVAL":
				reader.Read();
				ImpressionInterval = Convert.ToInt32(reader.Value);
				break;
			case "DURATION":
				reader.Read();
				Duration = Convert.ToInt32(reader.Value);
				break;
			case "MESSAGETYPE":
				reader.Read();
				MessageType = ReaderUtil.ReadEnum<SalePackMessageType>(reader);
				break;
			case "MESSAGELINKTYPE":
				reader.Read();
				MessageLinkType = ReaderUtil.ReadEnum<SalePackMessageLinkType>(reader);
				break;
			case "MESSAGEIMAGE":
				reader.Read();
				MessageImage = ReaderUtil.ReadString(reader, converters);
				break;
			case "MESSAGEMASK":
				reader.Read();
				MessageMask = ReaderUtil.ReadString(reader, converters);
				break;
			case "GLASSICONIMAGE":
				reader.Read();
				GlassIconImage = ReaderUtil.ReadString(reader, converters);
				break;
			case "GLASSICONMASK":
				reader.Read();
				GlassIconMask = ReaderUtil.ReadString(reader, converters);
				break;
			case "MESSAGEURL":
				reader.Read();
				MessageUrl = ReaderUtil.ReadString(reader, converters);
				break;
			case "SERVERSALEID":
				reader.Read();
				ServerSaleId = ReaderUtil.ReadString(reader, converters);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public override void Serialize(JsonWriter writer)
		{
			writer.WriteStartObject();
			SerializeProperties(writer);
			writer.WriteEndObject();
		}

		protected override void SerializeProperties(JsonWriter writer)
		{
			base.SerializeProperties(writer);
			if (base.LocalizedKey != null)
			{
				writer.WritePropertyName("LocalizedKey");
				writer.WriteValue(base.LocalizedKey);
			}
			writer.WritePropertyName("ID");
			writer.WriteValue(ID);
			writer.WritePropertyName("Disabled");
			writer.WriteValue(base.Disabled);
			if (Image != null)
			{
				writer.WritePropertyName("Image");
				writer.WriteValue(Image);
			}
			if (Mask != null)
			{
				writer.WritePropertyName("Mask");
				writer.WriteValue(Mask);
			}
			if (Description != null)
			{
				writer.WritePropertyName("Description");
				writer.WriteValue(Description);
			}
			if (base.TaxonomyHighLevel != null)
			{
				writer.WritePropertyName("TaxonomyHighLevel");
				writer.WriteValue(base.TaxonomyHighLevel);
			}
			if (base.TaxonomySpecific != null)
			{
				writer.WritePropertyName("TaxonomySpecific");
				writer.WriteValue(base.TaxonomySpecific);
			}
			if (base.TaxonomyType != null)
			{
				writer.WritePropertyName("TaxonomyType");
				writer.WriteValue(base.TaxonomyType);
			}
			if (base.TaxonomyOther != null)
			{
				writer.WritePropertyName("TaxonomyOther");
				writer.WriteValue(base.TaxonomyOther);
			}
			if (base.VFX != null)
			{
				writer.WritePropertyName("VFX");
				writer.WriteValue(base.VFX);
			}
			if (base.VFXOffset != null)
			{
				writer.WritePropertyName("VFXOffset");
				writer.WriteStartObject();
				writer.WritePropertyName("x");
				writer.WriteValue(base.VFXOffset.x);
				writer.WritePropertyName("y");
				writer.WriteValue(base.VFXOffset.y);
				writer.WritePropertyName("z");
				writer.WriteValue(base.VFXOffset.z);
				writer.WriteEndObject();
			}
			if (base.Audio != null)
			{
				writer.WritePropertyName("Audio");
				writer.WriteValue(base.Audio);
			}
			writer.WritePropertyName("COPPAGated");
			writer.WriteValue(base.COPPAGated);
			if (base.PlatformStoreSku != null)
			{
				writer.WritePropertyName("PlatformStoreSku");
				writer.WriteStartArray();
				IEnumerator<PlatformStoreSkuDefinition> enumerator = base.PlatformStoreSku.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						PlatformStoreSkuDefinition current = enumerator.Current;
						writer.WriteStartObject();
						if (current.appleAppstore != null)
						{
							writer.WritePropertyName("appleAppstore");
							writer.WriteValue(current.appleAppstore);
						}
						if (current.googlePlay != null)
						{
							writer.WritePropertyName("googlePlay");
							writer.WriteValue(current.googlePlay);
						}
						if (current.defaultStore != null)
						{
							writer.WritePropertyName("defaultStore");
							writer.WriteValue(current.defaultStore);
						}
						writer.WriteEndObject();
					}
				}
				finally
				{
					enumerator.Dispose();
				}
				writer.WriteEndArray();
			}
			writer.WritePropertyName("ActiveSKUIndex");
			writer.WriteValue(base.ActiveSKUIndex);
			writer.WritePropertyName("Type");
			writer.WriteValue((int)Type);
			writer.WritePropertyName("UTCStartDate");
			writer.WriteValue(UTCStartDate);
			writer.WritePropertyName("UTCEndDate");
			writer.WriteValue(UTCEndDate);
			writer.WritePropertyName("Impressions");
			writer.WriteValue(Impressions);
			writer.WritePropertyName("ImpressionInterval");
			writer.WriteValue(ImpressionInterval);
			writer.WritePropertyName("Duration");
			writer.WriteValue(Duration);
			writer.WritePropertyName("MessageType");
			writer.WriteValue((int)MessageType);
			writer.WritePropertyName("MessageLinkType");
			writer.WriteValue((int)MessageLinkType);
			if (MessageImage != null)
			{
				writer.WritePropertyName("MessageImage");
				writer.WriteValue(MessageImage);
			}
			if (MessageMask != null)
			{
				writer.WritePropertyName("MessageMask");
				writer.WriteValue(MessageMask);
			}
			if (GlassIconImage != null)
			{
				writer.WritePropertyName("GlassIconImage");
				writer.WriteValue(GlassIconImage);
			}
			if (GlassIconMask != null)
			{
				writer.WritePropertyName("GlassIconMask");
				writer.WriteValue(GlassIconMask);
			}
			if (MessageUrl != null)
			{
				writer.WritePropertyName("MessageUrl");
				writer.WriteValue(MessageUrl);
			}
			if (ServerSaleId != null)
			{
				writer.WritePropertyName("ServerSaleId");
				writer.WriteValue(ServerSaleId);
			}
		}

		public Instance Build()
		{
			return new Sale(this);
		}
	}
}
