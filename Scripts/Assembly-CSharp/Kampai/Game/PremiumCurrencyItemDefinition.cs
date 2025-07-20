using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class PremiumCurrencyItemDefinition : CurrencyItemDefinition, MTXItem
	{
		public override int TypeCode
		{
			get
			{
				return 1138;
			}
		}

		public IList<PlatformStoreSkuDefinition> PlatformStoreSku { get; set; }

		public int ActiveSKUIndex { get; set; }

		public string SKU
		{
			get
			{
				if (PlatformStoreSku == null || PlatformStoreSku.Count <= ActiveSKUIndex)
				{
					return string.Empty;
				}
				return PlatformStoreSku[ActiveSKUIndex].googlePlay;
			}
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WritePlatformStoreSkuDefinition, PlatformStoreSku);
			writer.Write(ActiveSKUIndex);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			PlatformStoreSku = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadPlatformStoreSkuDefinition, PlatformStoreSku);
			ActiveSKUIndex = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			default:
			{
				int num;
				if (num == 1)
				{
					reader.Read();
					ActiveSKUIndex = Convert.ToInt32(reader.Value);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "PLATFORMSTORESKU":
				reader.Read();
				PlatformStoreSku = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadPlatformStoreSkuDefinition, PlatformStoreSku);
				break;
			}
			return true;
		}
	}
}
