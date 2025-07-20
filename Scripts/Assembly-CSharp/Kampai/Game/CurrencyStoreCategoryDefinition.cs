using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class CurrencyStoreCategoryDefinition : Definition, IDisplayableDefinition, IComparable<CurrencyStoreCategoryDefinition>, IEquatable<CurrencyStoreCategoryDefinition>, IComparer<CurrencyStoreCategoryDefinition>
	{
		public override int TypeCode
		{
			get
			{
				return 1142;
			}
		}

		public StoreCategoryType StoreCategoryType { get; set; }

		public string Image { get; set; }

		public string Mask { get; set; }

		public string Description { get; set; }

		public IList<int> StoreItemDefinitionIDs { get; set; }

		public int Priority { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteEnum(writer, StoreCategoryType);
			BinarySerializationUtil.WriteString(writer, Image);
			BinarySerializationUtil.WriteString(writer, Mask);
			BinarySerializationUtil.WriteString(writer, Description);
			BinarySerializationUtil.WriteListInt32(writer, StoreItemDefinitionIDs);
			writer.Write(Priority);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			StoreCategoryType = BinarySerializationUtil.ReadEnum<StoreCategoryType>(reader);
			Image = BinarySerializationUtil.ReadString(reader);
			Mask = BinarySerializationUtil.ReadString(reader);
			Description = BinarySerializationUtil.ReadString(reader);
			StoreItemDefinitionIDs = BinarySerializationUtil.ReadListInt32(reader, StoreItemDefinitionIDs);
			Priority = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "STORECATEGORYTYPE":
				reader.Read();
				StoreCategoryType = ReaderUtil.ReadEnum<StoreCategoryType>(reader);
				break;
			case "IMAGE":
				reader.Read();
				Image = ReaderUtil.ReadString(reader, converters);
				break;
			case "MASK":
				reader.Read();
				Mask = ReaderUtil.ReadString(reader, converters);
				break;
			case "DESCRIPTION":
				reader.Read();
				Description = ReaderUtil.ReadString(reader, converters);
				break;
			case "STOREITEMDEFINITIONIDS":
				reader.Read();
				StoreItemDefinitionIDs = ReaderUtil.PopulateListInt32(reader, StoreItemDefinitionIDs);
				break;
			case "PRIORITY":
				reader.Read();
				Priority = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public virtual int CompareTo(CurrencyStoreCategoryDefinition rhs)
		{
			if (rhs == null)
			{
				return 1;
			}
			int num = rhs.Priority.CompareTo(Priority);
			if (num != 0)
			{
				return num;
			}
			return ID.CompareTo(rhs.ID);
		}

		public int Compare(CurrencyStoreCategoryDefinition x, CurrencyStoreCategoryDefinition y)
		{
			if (x == null)
			{
				return -1;
			}
			return x.CompareTo(y);
		}

		public bool Equals(CurrencyStoreCategoryDefinition obj)
		{
			return obj != null && Equals((object)obj);
		}

		public override string ToString()
		{
			return string.Format("{0}, PRIORITY: {1}, CATEGORY: {2}", base.ToString(), Priority, StoreCategoryType);
		}

		public override bool Equals(object obj)
		{
			if (object.ReferenceEquals(null, obj))
			{
				return false;
			}
			if (object.ReferenceEquals(this, obj))
			{
				return true;
			}
			CurrencyStoreCategoryDefinition currencyStoreCategoryDefinition = obj as CurrencyStoreCategoryDefinition;
			return !object.ReferenceEquals(null, currencyStoreCategoryDefinition) && CompareTo(currencyStoreCategoryDefinition) == 0;
		}

		public override int GetHashCode()
		{
			return new { Priority, ID }.GetHashCode();
		}
	}
}
