using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class RewardCollectionDefinition : Definition, IBuilder<Instance>
	{
		public override int TypeCode
		{
			get
			{
				return 1085;
			}
		}

		public IList<CollectionReward> Rewards { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteCollectionReward, Rewards);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Rewards = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadCollectionReward, Rewards);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "REWARDS":
				reader.Read();
				Rewards = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadCollectionReward, Rewards);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}

		public Instance Build()
		{
			return new RewardCollection(this);
		}
	}
}
