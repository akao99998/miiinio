using System;
using System.IO;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class RushTimeBandDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1135;
			}
		}

		public int TimeRemainingInSeconds { get; set; }

		public int PremiumCostConstruction { get; set; }

		public int PremiumCostBaseResource { get; set; }

		public int PremiumCostCraftable { get; set; }

		public int PremiumCostCooldowns { get; set; }

		public int PremiumCostLeisure { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(TimeRemainingInSeconds);
			writer.Write(PremiumCostConstruction);
			writer.Write(PremiumCostBaseResource);
			writer.Write(PremiumCostCraftable);
			writer.Write(PremiumCostCooldowns);
			writer.Write(PremiumCostLeisure);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			TimeRemainingInSeconds = reader.ReadInt32();
			PremiumCostConstruction = reader.ReadInt32();
			PremiumCostBaseResource = reader.ReadInt32();
			PremiumCostCraftable = reader.ReadInt32();
			PremiumCostCooldowns = reader.ReadInt32();
			PremiumCostLeisure = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "TIMEREMAININGINSECONDS":
				reader.Read();
				TimeRemainingInSeconds = Convert.ToInt32(reader.Value);
				break;
			case "PREMIUMCOSTCONSTRUCTION":
				reader.Read();
				PremiumCostConstruction = Convert.ToInt32(reader.Value);
				break;
			case "PREMIUMCOSTBASERESOURCE":
				reader.Read();
				PremiumCostBaseResource = Convert.ToInt32(reader.Value);
				break;
			case "PREMIUMCOSTCRAFTABLE":
				reader.Read();
				PremiumCostCraftable = Convert.ToInt32(reader.Value);
				break;
			case "PREMIUMCOSTCOOLDOWNS":
				reader.Read();
				PremiumCostCooldowns = Convert.ToInt32(reader.Value);
				break;
			case "PREMIUMCOSTLEISURE":
				reader.Read();
				PremiumCostLeisure = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public int GetCostForRushActionType(RushActionType rushActionType)
		{
			switch (rushActionType)
			{
			case RushActionType.CONSTRUCTION:
				return PremiumCostConstruction;
			case RushActionType.COOLDOWN:
				return PremiumCostCooldowns;
			case RushActionType.CRAFTING:
				return PremiumCostCraftable;
			case RushActionType.HARVESTING:
				return PremiumCostBaseResource;
			case RushActionType.LEISURE:
				return PremiumCostLeisure;
			default:
				return -1;
			}
		}
	}
}
