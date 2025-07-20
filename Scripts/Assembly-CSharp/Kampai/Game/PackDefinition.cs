using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Game.Transaction;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class PackDefinition : PremiumCurrencyItemDefinition, IFastJSONSerializable
	{
		public override int TypeCode
		{
			get
			{
				return 1137;
			}
		}

		public UpsellTransactionType TransactionType { get; set; }

		public string BannerAd { get; set; }

		public int UnlockQuestId { get; set; }

		public int UnlockLevel { get; set; }

		public int CurrencyImageID { get; set; }

		public bool UnlockByTrigger { get; set; }

		public bool DisableDynamicUnlock { get; set; }

		public int CanBuyThisManyTimes { get; set; }

		public int PercentagePer100 { get; set; }

		public TransactionInstance TransactionDefinition { get; set; }

		public IList<int> ExclusiveItemList { get; set; }

		public IList<int> AudibleItemList { get; set; }

		public SalePackLayout Layout { get; set; }

		public string LayoutPrefab { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteEnum(writer, TransactionType);
			BinarySerializationUtil.WriteString(writer, BannerAd);
			writer.Write(UnlockQuestId);
			writer.Write(UnlockLevel);
			writer.Write(CurrencyImageID);
			writer.Write(UnlockByTrigger);
			writer.Write(DisableDynamicUnlock);
			writer.Write(CanBuyThisManyTimes);
			writer.Write(PercentagePer100);
			BinarySerializationUtil.WriteTransactionInstance(writer, TransactionDefinition);
			BinarySerializationUtil.WriteListInt32(writer, ExclusiveItemList);
			BinarySerializationUtil.WriteListInt32(writer, AudibleItemList);
			BinarySerializationUtil.WriteEnum(writer, Layout);
			BinarySerializationUtil.WriteString(writer, LayoutPrefab);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			TransactionType = BinarySerializationUtil.ReadEnum<UpsellTransactionType>(reader);
			BannerAd = BinarySerializationUtil.ReadString(reader);
			UnlockQuestId = reader.ReadInt32();
			UnlockLevel = reader.ReadInt32();
			CurrencyImageID = reader.ReadInt32();
			UnlockByTrigger = reader.ReadBoolean();
			DisableDynamicUnlock = reader.ReadBoolean();
			CanBuyThisManyTimes = reader.ReadInt32();
			PercentagePer100 = reader.ReadInt32();
			TransactionDefinition = BinarySerializationUtil.ReadTransactionInstance(reader);
			ExclusiveItemList = BinarySerializationUtil.ReadListInt32(reader, ExclusiveItemList);
			AudibleItemList = BinarySerializationUtil.ReadListInt32(reader, AudibleItemList);
			Layout = BinarySerializationUtil.ReadEnum<SalePackLayout>(reader);
			LayoutPrefab = BinarySerializationUtil.ReadString(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "TRANSACTIONTYPE":
				reader.Read();
				TransactionType = ReaderUtil.ReadEnum<UpsellTransactionType>(reader);
				break;
			case "BANNERAD":
				reader.Read();
				BannerAd = ReaderUtil.ReadString(reader, converters);
				break;
			case "UNLOCKQUESTID":
				reader.Read();
				UnlockQuestId = Convert.ToInt32(reader.Value);
				break;
			case "UNLOCKLEVEL":
				reader.Read();
				UnlockLevel = Convert.ToInt32(reader.Value);
				break;
			case "CURRENCYIMAGEID":
				reader.Read();
				CurrencyImageID = Convert.ToInt32(reader.Value);
				break;
			case "UNLOCKBYTRIGGER":
				reader.Read();
				UnlockByTrigger = Convert.ToBoolean(reader.Value);
				break;
			case "DISABLEDYNAMICUNLOCK":
				reader.Read();
				DisableDynamicUnlock = Convert.ToBoolean(reader.Value);
				break;
			case "CANBUYTHISMANYTIMES":
				reader.Read();
				CanBuyThisManyTimes = Convert.ToInt32(reader.Value);
				break;
			case "PERCENTAGEPER100":
				reader.Read();
				PercentagePer100 = Convert.ToInt32(reader.Value);
				break;
			case "TRANSACTIONDEFINITION":
				reader.Read();
				TransactionDefinition = ReaderUtil.ReadTransactionInstance(reader, converters);
				break;
			case "EXCLUSIVEITEMLIST":
				reader.Read();
				ExclusiveItemList = ReaderUtil.PopulateListInt32(reader, ExclusiveItemList);
				break;
			case "AUDIBLEITEMLIST":
				reader.Read();
				AudibleItemList = ReaderUtil.PopulateListInt32(reader, AudibleItemList);
				break;
			case "LAYOUT":
				reader.Read();
				Layout = ReaderUtil.ReadEnum<SalePackLayout>(reader);
				break;
			case "LAYOUTPREFAB":
				reader.Read();
				LayoutPrefab = ReaderUtil.ReadString(reader, converters);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
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
			writer.WritePropertyName("TransactionType");
			writer.WriteValue((int)TransactionType);
			if (BannerAd != null)
			{
				writer.WritePropertyName("BannerAd");
				writer.WriteValue(BannerAd);
			}
			writer.WritePropertyName("UnlockQuestId");
			writer.WriteValue(UnlockQuestId);
			writer.WritePropertyName("UnlockLevel");
			writer.WriteValue(UnlockLevel);
			writer.WritePropertyName("CurrencyImageID");
			writer.WriteValue(CurrencyImageID);
			writer.WritePropertyName("UnlockByTrigger");
			writer.WriteValue(UnlockByTrigger);
			writer.WritePropertyName("DisableDynamicUnlock");
			writer.WriteValue(DisableDynamicUnlock);
			writer.WritePropertyName("CanBuyThisManyTimes");
			writer.WriteValue(CanBuyThisManyTimes);
			writer.WritePropertyName("PercentagePer100");
			writer.WriteValue(PercentagePer100);
			if (TransactionDefinition != null)
			{
				writer.WritePropertyName("TransactionDefinition");
				writer.WriteStartObject();
				writer.WritePropertyName("ID");
				writer.WriteValue(TransactionDefinition.ID);
				if (TransactionDefinition.Inputs != null)
				{
					writer.WritePropertyName("Inputs");
					writer.WriteStartArray();
					IEnumerator<QuantityItem> enumerator2 = TransactionDefinition.Inputs.GetEnumerator();
					try
					{
						while (enumerator2.MoveNext())
						{
							QuantityItem current2 = enumerator2.Current;
							writer.WriteStartObject();
							writer.WritePropertyName("ID");
							writer.WriteValue(current2.ID);
							writer.WritePropertyName("Quantity");
							writer.WriteValue(current2.Quantity);
							writer.WriteEndObject();
						}
					}
					finally
					{
						enumerator2.Dispose();
					}
					writer.WriteEndArray();
				}
				if (TransactionDefinition.Outputs != null)
				{
					writer.WritePropertyName("Outputs");
					writer.WriteStartArray();
					IEnumerator<QuantityItem> enumerator3 = TransactionDefinition.Outputs.GetEnumerator();
					try
					{
						while (enumerator3.MoveNext())
						{
							QuantityItem current3 = enumerator3.Current;
							writer.WriteStartObject();
							writer.WritePropertyName("ID");
							writer.WriteValue(current3.ID);
							writer.WritePropertyName("Quantity");
							writer.WriteValue(current3.Quantity);
							writer.WriteEndObject();
						}
					}
					finally
					{
						enumerator3.Dispose();
					}
					writer.WriteEndArray();
				}
				writer.WriteEndObject();
			}
			if (ExclusiveItemList != null)
			{
				writer.WritePropertyName("ExclusiveItemList");
				writer.WriteStartArray();
				IEnumerator<int> enumerator4 = ExclusiveItemList.GetEnumerator();
				try
				{
					while (enumerator4.MoveNext())
					{
						int current4 = enumerator4.Current;
						writer.WriteValue(current4);
					}
				}
				finally
				{
					enumerator4.Dispose();
				}
				writer.WriteEndArray();
			}
			if (AudibleItemList != null)
			{
				writer.WritePropertyName("AudibleItemList");
				writer.WriteStartArray();
				IEnumerator<int> enumerator5 = AudibleItemList.GetEnumerator();
				try
				{
					while (enumerator5.MoveNext())
					{
						int current5 = enumerator5.Current;
						writer.WriteValue(current5);
					}
				}
				finally
				{
					enumerator5.Dispose();
				}
				writer.WriteEndArray();
			}
			writer.WritePropertyName("Layout");
			writer.WriteValue((int)Layout);
			if (LayoutPrefab != null)
			{
				writer.WritePropertyName("LayoutPrefab");
				writer.WriteValue(LayoutPrefab);
			}
		}

		public float getDiscountRate()
		{
			return 1f - (float)PercentagePer100 / 100f;
		}
	}
}
