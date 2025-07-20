using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public abstract class MinionPartyBuildingDefinition : AnimatingBuildingDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1057;
			}
		}

		public IList<MinionPartyPrefabDefinition> MinionPartyPrefabs { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteMinionPartyPrefabDefinition, MinionPartyPrefabs);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			MinionPartyPrefabs = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadMinionPartyPrefabDefinition, MinionPartyPrefabs);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "MINIONPARTYPREFABS":
				reader.Read();
				MinionPartyPrefabs = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadMinionPartyPrefabDefinition, MinionPartyPrefabs);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}
	}
}
