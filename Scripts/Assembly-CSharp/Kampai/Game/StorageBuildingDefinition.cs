using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class StorageBuildingDefinition : AnimatingBuildingDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1063;
			}
		}

		public int Capacity { get; set; }

		public IList<StorageUpgradeDefinition> StorageUpgrades { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(Capacity);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteStorageUpgradeDefinition, StorageUpgrades);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Capacity = reader.ReadInt32();
			StorageUpgrades = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadStorageUpgradeDefinition, StorageUpgrades);
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
					StorageUpgrades = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadStorageUpgradeDefinition, StorageUpgrades);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "CAPACITY":
				reader.Read();
				Capacity = Convert.ToInt32(reader.Value);
				break;
			}
			return true;
		}

		public override Building BuildBuilding()
		{
			return new StorageBuilding(this);
		}
	}
}
