using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class PopulationBenefitDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1122;
			}
		}

		public int numMinionsRequired { get; set; }

		public int minionLevelRequired { get; set; }

		public int transactionDefinitionID { get; set; }

		public string benefitDescriptionLocalizedKey { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(numMinionsRequired);
			writer.Write(minionLevelRequired);
			writer.Write(transactionDefinitionID);
			BinarySerializationUtil.WriteString(writer, benefitDescriptionLocalizedKey);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			numMinionsRequired = reader.ReadInt32();
			minionLevelRequired = reader.ReadInt32();
			transactionDefinitionID = reader.ReadInt32();
			benefitDescriptionLocalizedKey = BinarySerializationUtil.ReadString(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "NUMMINIONSREQUIRED":
				reader.Read();
				numMinionsRequired = Convert.ToInt32(reader.Value);
				break;
			case "MINIONLEVELREQUIRED":
				reader.Read();
				minionLevelRequired = Convert.ToInt32(reader.Value);
				break;
			case "TRANSACTIONDEFINITIONID":
				reader.Read();
				transactionDefinitionID = Convert.ToInt32(reader.Value);
				break;
			case "BENEFITDESCRIPTIONLOCALIZEDKEY":
				reader.Read();
				benefitDescriptionLocalizedKey = ReaderUtil.ReadString(reader, converters);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
