using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	[RequiresJsonConverter]
	public class ItemDefinition : TaxonomyDefinition, IBuilder<Instance>
	{
		public override int TypeCode
		{
			get
			{
				return 1089;
			}
		}

		public float BasePremiumCost { get; set; }

		public int BaseGrindCost { get; set; }

		public float TSMRewardMultipler { get; set; }

		public int PlayerTrainingDefinitionID { get; set; }

		public bool Storable { get; set; }

		public bool SellableForced { get; set; }

		public ItemDefinition()
		{
			Storable = true;
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(BasePremiumCost);
			writer.Write(BaseGrindCost);
			writer.Write(TSMRewardMultipler);
			writer.Write(PlayerTrainingDefinitionID);
			writer.Write(Storable);
			writer.Write(SellableForced);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			BasePremiumCost = reader.ReadSingle();
			BaseGrindCost = reader.ReadInt32();
			TSMRewardMultipler = reader.ReadSingle();
			PlayerTrainingDefinitionID = reader.ReadInt32();
			Storable = reader.ReadBoolean();
			SellableForced = reader.ReadBoolean();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "BASEPREMIUMCOST":
				reader.Read();
				BasePremiumCost = Convert.ToSingle(reader.Value);
				break;
			case "BASEGRINDCOST":
				reader.Read();
				BaseGrindCost = Convert.ToInt32(reader.Value);
				break;
			case "TSMREWARDMULTIPLER":
				reader.Read();
				TSMRewardMultipler = Convert.ToSingle(reader.Value);
				break;
			case "PLAYERTRAININGDEFINITIONID":
				reader.Read();
				PlayerTrainingDefinitionID = Convert.ToInt32(reader.Value);
				break;
			case "STORABLE":
				reader.Read();
				Storable = Convert.ToBoolean(reader.Value);
				break;
			case "SELLABLEFORCED":
				reader.Read();
				SellableForced = Convert.ToBoolean(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public virtual Instance Build()
		{
			return new Item(this);
		}
	}
}
