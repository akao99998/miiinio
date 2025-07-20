using System;
using System.IO;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class TaskLevelBandDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1146;
			}
		}

		public int MinLevel { get; set; }

		public float FillOrderTaskMinMultiplier { get; set; }

		public float FillOrderTaskMaxMultiplier { get; set; }

		public float GiveTaskMinMultiplier { get; set; }

		public float GiveTaskMaxMultiplier { get; set; }

		public int GiveTaskMinQuantity { get; set; }

		public int XpReward { get; set; }

		public float MinXpMultiplier { get; set; }

		public float MaxXpMultiplier { get; set; }

		public int GrindReward { get; set; }

		public float MinGrindMultiplier { get; set; }

		public float MaxGrindMultiplier { get; set; }

		public int DropOdds { get; set; }

		public int PickWeightsId { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(MinLevel);
			writer.Write(FillOrderTaskMinMultiplier);
			writer.Write(FillOrderTaskMaxMultiplier);
			writer.Write(GiveTaskMinMultiplier);
			writer.Write(GiveTaskMaxMultiplier);
			writer.Write(GiveTaskMinQuantity);
			writer.Write(XpReward);
			writer.Write(MinXpMultiplier);
			writer.Write(MaxXpMultiplier);
			writer.Write(GrindReward);
			writer.Write(MinGrindMultiplier);
			writer.Write(MaxGrindMultiplier);
			writer.Write(DropOdds);
			writer.Write(PickWeightsId);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			MinLevel = reader.ReadInt32();
			FillOrderTaskMinMultiplier = reader.ReadSingle();
			FillOrderTaskMaxMultiplier = reader.ReadSingle();
			GiveTaskMinMultiplier = reader.ReadSingle();
			GiveTaskMaxMultiplier = reader.ReadSingle();
			GiveTaskMinQuantity = reader.ReadInt32();
			XpReward = reader.ReadInt32();
			MinXpMultiplier = reader.ReadSingle();
			MaxXpMultiplier = reader.ReadSingle();
			GrindReward = reader.ReadInt32();
			MinGrindMultiplier = reader.ReadSingle();
			MaxGrindMultiplier = reader.ReadSingle();
			DropOdds = reader.ReadInt32();
			PickWeightsId = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "MINLEVEL":
				reader.Read();
				MinLevel = Convert.ToInt32(reader.Value);
				break;
			case "FILLORDERTASKMINMULTIPLIER":
				reader.Read();
				FillOrderTaskMinMultiplier = Convert.ToSingle(reader.Value);
				break;
			case "FILLORDERTASKMAXMULTIPLIER":
				reader.Read();
				FillOrderTaskMaxMultiplier = Convert.ToSingle(reader.Value);
				break;
			case "GIVETASKMINMULTIPLIER":
				reader.Read();
				GiveTaskMinMultiplier = Convert.ToSingle(reader.Value);
				break;
			case "GIVETASKMAXMULTIPLIER":
				reader.Read();
				GiveTaskMaxMultiplier = Convert.ToSingle(reader.Value);
				break;
			case "GIVETASKMINQUANTITY":
				reader.Read();
				GiveTaskMinQuantity = Convert.ToInt32(reader.Value);
				break;
			case "XPREWARD":
				reader.Read();
				XpReward = Convert.ToInt32(reader.Value);
				break;
			case "MINXPMULTIPLIER":
				reader.Read();
				MinXpMultiplier = Convert.ToSingle(reader.Value);
				break;
			case "MAXXPMULTIPLIER":
				reader.Read();
				MaxXpMultiplier = Convert.ToSingle(reader.Value);
				break;
			case "GRINDREWARD":
				reader.Read();
				GrindReward = Convert.ToInt32(reader.Value);
				break;
			case "MINGRINDMULTIPLIER":
				reader.Read();
				MinGrindMultiplier = Convert.ToSingle(reader.Value);
				break;
			case "MAXGRINDMULTIPLIER":
				reader.Read();
				MaxGrindMultiplier = Convert.ToSingle(reader.Value);
				break;
			case "DROPODDS":
				reader.Read();
				DropOdds = Convert.ToInt32(reader.Value);
				break;
			case "PICKWEIGHTSID":
				reader.Read();
				PickWeightsId = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
