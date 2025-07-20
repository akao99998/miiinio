using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class MarketplaceDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1102;
			}
		}

		public IList<MarketplaceItemDefinition> itemDefinitions { get; set; }

		public IList<MarketplaceSaleSlotDefinition> slotDefinitions { get; set; }

		public IList<Vector3> buyTimeSpline { get; set; }

		public MarketplaceRefreshTimerDefinition refreshTimerDefinition { get; set; }

		public int MaxSellQuantity { get; set; }

		public int MaxDropQuantity { get; set; }

		public float VariabilityBuyTimePercent { get; set; }

		public int CraftableWeight { get; set; }

		public int BaseResourceWeight { get; set; }

		public int DropWeight { get; set; }

		public int TotalSaleAds { get; set; }

		public int TotalBuyAds { get; set; }

		public int StartingBuyAds { get; set; }

		public int StandardSlots { get; set; }

		public int FacebookSlots { get; set; }

		public int MaxPremiumSlots { get; set; }

		public int PremiumInitialCost { get; set; }

		public int PremiumIncrementCost { get; set; }

		public int DeleteSaleCost { get; set; }

		public int LevelGate { get; set; }

		public int SaleCancellationCost { get; set; }

		public KampaiColor SellPriceUpTextColor { get; set; }

		public KampaiColor SellPriceUpBackgroundColor { get; set; }

		public KampaiColor SellPriceDownTextColor { get; set; }

		public KampaiColor SellPriceDownBackgroundColor { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteList(writer, itemDefinitions);
			BinarySerializationUtil.WriteList(writer, slotDefinitions);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteVector3, buyTimeSpline);
			BinarySerializationUtil.WriteObject(writer, refreshTimerDefinition);
			writer.Write(MaxSellQuantity);
			writer.Write(MaxDropQuantity);
			writer.Write(VariabilityBuyTimePercent);
			writer.Write(CraftableWeight);
			writer.Write(BaseResourceWeight);
			writer.Write(DropWeight);
			writer.Write(TotalSaleAds);
			writer.Write(TotalBuyAds);
			writer.Write(StartingBuyAds);
			writer.Write(StandardSlots);
			writer.Write(FacebookSlots);
			writer.Write(MaxPremiumSlots);
			writer.Write(PremiumInitialCost);
			writer.Write(PremiumIncrementCost);
			writer.Write(DeleteSaleCost);
			writer.Write(LevelGate);
			writer.Write(SaleCancellationCost);
			BinarySerializationUtil.WriteKampaiColor(writer, SellPriceUpTextColor);
			BinarySerializationUtil.WriteKampaiColor(writer, SellPriceUpBackgroundColor);
			BinarySerializationUtil.WriteKampaiColor(writer, SellPriceDownTextColor);
			BinarySerializationUtil.WriteKampaiColor(writer, SellPriceDownBackgroundColor);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			itemDefinitions = BinarySerializationUtil.ReadList(reader, itemDefinitions);
			slotDefinitions = BinarySerializationUtil.ReadList(reader, slotDefinitions);
			buyTimeSpline = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadVector3, buyTimeSpline);
			refreshTimerDefinition = BinarySerializationUtil.ReadObject<MarketplaceRefreshTimerDefinition>(reader);
			MaxSellQuantity = reader.ReadInt32();
			MaxDropQuantity = reader.ReadInt32();
			VariabilityBuyTimePercent = reader.ReadSingle();
			CraftableWeight = reader.ReadInt32();
			BaseResourceWeight = reader.ReadInt32();
			DropWeight = reader.ReadInt32();
			TotalSaleAds = reader.ReadInt32();
			TotalBuyAds = reader.ReadInt32();
			StartingBuyAds = reader.ReadInt32();
			StandardSlots = reader.ReadInt32();
			FacebookSlots = reader.ReadInt32();
			MaxPremiumSlots = reader.ReadInt32();
			PremiumInitialCost = reader.ReadInt32();
			PremiumIncrementCost = reader.ReadInt32();
			DeleteSaleCost = reader.ReadInt32();
			LevelGate = reader.ReadInt32();
			SaleCancellationCost = reader.ReadInt32();
			SellPriceUpTextColor = BinarySerializationUtil.ReadKampaiColor(reader);
			SellPriceUpBackgroundColor = BinarySerializationUtil.ReadKampaiColor(reader);
			SellPriceDownTextColor = BinarySerializationUtil.ReadKampaiColor(reader);
			SellPriceDownBackgroundColor = BinarySerializationUtil.ReadKampaiColor(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "ITEMDEFINITIONS":
				reader.Read();
				itemDefinitions = ReaderUtil.PopulateList(reader, converters, itemDefinitions);
				break;
			case "SLOTDEFINITIONS":
				reader.Read();
				slotDefinitions = ReaderUtil.PopulateList(reader, converters, slotDefinitions);
				break;
			case "BUYTIMESPLINE":
				reader.Read();
				buyTimeSpline = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadVector3, buyTimeSpline);
				break;
			case "REFRESHTIMERDEFINITION":
				reader.Read();
				refreshTimerDefinition = FastJSONDeserializer.Deserialize<MarketplaceRefreshTimerDefinition>(reader, converters);
				break;
			case "MAXSELLQUANTITY":
				reader.Read();
				MaxSellQuantity = Convert.ToInt32(reader.Value);
				break;
			case "MAXDROPQUANTITY":
				reader.Read();
				MaxDropQuantity = Convert.ToInt32(reader.Value);
				break;
			case "VARIABILITYBUYTIMEPERCENT":
				reader.Read();
				VariabilityBuyTimePercent = Convert.ToSingle(reader.Value);
				break;
			case "CRAFTABLEWEIGHT":
				reader.Read();
				CraftableWeight = Convert.ToInt32(reader.Value);
				break;
			case "BASERESOURCEWEIGHT":
				reader.Read();
				BaseResourceWeight = Convert.ToInt32(reader.Value);
				break;
			case "DROPWEIGHT":
				reader.Read();
				DropWeight = Convert.ToInt32(reader.Value);
				break;
			case "TOTALSALEADS":
				reader.Read();
				TotalSaleAds = Convert.ToInt32(reader.Value);
				break;
			case "TOTALBUYADS":
				reader.Read();
				TotalBuyAds = Convert.ToInt32(reader.Value);
				break;
			case "STARTINGBUYADS":
				reader.Read();
				StartingBuyAds = Convert.ToInt32(reader.Value);
				break;
			case "STANDARDSLOTS":
				reader.Read();
				StandardSlots = Convert.ToInt32(reader.Value);
				break;
			case "FACEBOOKSLOTS":
				reader.Read();
				FacebookSlots = Convert.ToInt32(reader.Value);
				break;
			case "MAXPREMIUMSLOTS":
				reader.Read();
				MaxPremiumSlots = Convert.ToInt32(reader.Value);
				break;
			case "PREMIUMINITIALCOST":
				reader.Read();
				PremiumInitialCost = Convert.ToInt32(reader.Value);
				break;
			case "PREMIUMINCREMENTCOST":
				reader.Read();
				PremiumIncrementCost = Convert.ToInt32(reader.Value);
				break;
			case "DELETESALECOST":
				reader.Read();
				DeleteSaleCost = Convert.ToInt32(reader.Value);
				break;
			case "LEVELGATE":
				reader.Read();
				LevelGate = Convert.ToInt32(reader.Value);
				break;
			case "SALECANCELLATIONCOST":
				reader.Read();
				SaleCancellationCost = Convert.ToInt32(reader.Value);
				break;
			case "SELLPRICEUPTEXTCOLOR":
				reader.Read();
				SellPriceUpTextColor = ReaderUtil.ReadKampaiColor(reader, converters);
				break;
			case "SELLPRICEUPBACKGROUNDCOLOR":
				reader.Read();
				SellPriceUpBackgroundColor = ReaderUtil.ReadKampaiColor(reader, converters);
				break;
			case "SELLPRICEDOWNTEXTCOLOR":
				reader.Read();
				SellPriceDownTextColor = ReaderUtil.ReadKampaiColor(reader, converters);
				break;
			case "SELLPRICEDOWNBACKGROUNDCOLOR":
				reader.Read();
				SellPriceDownBackgroundColor = ReaderUtil.ReadKampaiColor(reader, converters);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
