using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Main;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class StoreItemDefinition : Definition, IUTCRangeable
	{
		public override int TypeCode
		{
			get
			{
				return 1145;
			}
		}

		public StoreItemType Type { get; set; }

		public int ReferencedDefID { get; set; }

		public int TransactionID { get; set; }

		public bool OnlyShowIfInInventory { get; set; }

		public bool OnlyShowIfOwned { get; set; }

		public bool OnlyShowIfUnlocked { get; set; }

		public bool EnableBadging { get; set; }

		public bool IsFeatured { get; set; }

		public int SpecialEventID { get; set; }

		public int UTCStartDate { get; set; }

		public int UTCEndDate { get; set; }

		public int PercentOff { get; set; }

		public string Platform { get; set; }

		public List<string> Countries { get; set; }

		public int PriorityDefinition { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteEnum(writer, Type);
			writer.Write(ReferencedDefID);
			writer.Write(TransactionID);
			writer.Write(OnlyShowIfInInventory);
			writer.Write(OnlyShowIfOwned);
			writer.Write(OnlyShowIfUnlocked);
			writer.Write(EnableBadging);
			writer.Write(IsFeatured);
			writer.Write(SpecialEventID);
			writer.Write(UTCStartDate);
			writer.Write(UTCEndDate);
			writer.Write(PercentOff);
			BinarySerializationUtil.WriteString(writer, Platform);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteString, Countries);
			writer.Write(PriorityDefinition);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Type = BinarySerializationUtil.ReadEnum<StoreItemType>(reader);
			ReferencedDefID = reader.ReadInt32();
			TransactionID = reader.ReadInt32();
			OnlyShowIfInInventory = reader.ReadBoolean();
			OnlyShowIfOwned = reader.ReadBoolean();
			OnlyShowIfUnlocked = reader.ReadBoolean();
			EnableBadging = reader.ReadBoolean();
			IsFeatured = reader.ReadBoolean();
			SpecialEventID = reader.ReadInt32();
			UTCStartDate = reader.ReadInt32();
			UTCEndDate = reader.ReadInt32();
			PercentOff = reader.ReadInt32();
			Platform = BinarySerializationUtil.ReadString(reader);
			Countries = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadString, Countries);
			PriorityDefinition = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "TYPE":
				reader.Read();
				Type = ReaderUtil.ReadEnum<StoreItemType>(reader);
				break;
			case "REFERENCEDDEFID":
				reader.Read();
				ReferencedDefID = Convert.ToInt32(reader.Value);
				break;
			case "TRANSACTIONID":
				reader.Read();
				TransactionID = Convert.ToInt32(reader.Value);
				break;
			case "ONLYSHOWIFININVENTORY":
				reader.Read();
				OnlyShowIfInInventory = Convert.ToBoolean(reader.Value);
				break;
			case "ONLYSHOWIFOWNED":
				reader.Read();
				OnlyShowIfOwned = Convert.ToBoolean(reader.Value);
				break;
			case "ONLYSHOWIFUNLOCKED":
				reader.Read();
				OnlyShowIfUnlocked = Convert.ToBoolean(reader.Value);
				break;
			case "ENABLEBADGING":
				reader.Read();
				EnableBadging = Convert.ToBoolean(reader.Value);
				break;
			case "ISFEATURED":
				reader.Read();
				IsFeatured = Convert.ToBoolean(reader.Value);
				break;
			case "SPECIALEVENTID":
				reader.Read();
				SpecialEventID = Convert.ToInt32(reader.Value);
				break;
			case "UTCSTARTDATE":
				reader.Read();
				UTCStartDate = Convert.ToInt32(reader.Value);
				break;
			case "UTCENDDATE":
				reader.Read();
				UTCEndDate = Convert.ToInt32(reader.Value);
				break;
			case "PERCENTOFF":
				reader.Read();
				PercentOff = Convert.ToInt32(reader.Value);
				break;
			case "PLATFORM":
				reader.Read();
				Platform = ReaderUtil.ReadString(reader, converters);
				break;
			case "COUNTRIES":
				reader.Read();
				Countries = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadString, Countries);
				break;
			case "PRIORITYDEFINITION":
				reader.Read();
				PriorityDefinition = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public bool IsOnSale(RuntimePlatform rp, ITimeService timeService, ILocalizationService localeService, IKampaiLogger logger)
		{
			bool flag = false;
			if (timeService.WithinRange(this, true))
			{
				flag = true;
			}
			if (flag)
			{
				bool flag2 = false;
				bool flag3 = false;
				bool flag4 = false;
				bool flag5 = false;
				if (!string.IsNullOrEmpty(Platform))
				{
					flag2 = true;
					string text = Platform.Trim().ToLower();
					string value = StringUtil.UnifiedPlatformName(rp);
					if (string.IsNullOrEmpty(value))
					{
						logger.Error("Unknown platform {0}", rp.ToString());
						return false;
					}
					flag3 = text.Equals(StringUtil.UnifiedPlatformName(rp).ToLower());
				}
				List<string> countries = Countries;
				if (countries != null && countries.Count > 0)
				{
					flag4 = true;
					flag5 = ListUtil.StringIsInList(localeService.GetCountry(), countries);
				}
				if (flag4 && flag2)
				{
					flag = flag5 && flag3;
				}
				else if (flag4)
				{
					flag = flag5;
				}
				else if (flag2)
				{
					flag = flag3;
				}
			}
			return flag;
		}
	}
}
