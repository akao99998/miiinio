using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class BlackMarketBoardDefinition : AnimatingBuildingDefinition, ZoomableBuildingDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1035;
			}
		}

		public Vector3 zoomOffset { get; set; }

		public Vector3 zoomEulers { get; set; }

		public float zoomFOV { get; set; }

		public float TicketRepopTime { get; set; }

		public int RefillTime { get; set; }

		public IList<string> OrderNames { get; set; }

		public IList<BlackMarketBoardUnlockedOrderSlotDefinition> UnlockTicketSlots { get; set; }

		public IList<BlackMarketBoardSlotDefinition> MinMaxIngredients { get; set; }

		public IList<BlackMarketBoardMultiplierDefinition> LevelBandXP { get; set; }

		public int CharacterOrderChance { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteVector3(writer, zoomOffset);
			BinarySerializationUtil.WriteVector3(writer, zoomEulers);
			writer.Write(zoomFOV);
			writer.Write(TicketRepopTime);
			writer.Write(RefillTime);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteString, OrderNames);
			BinarySerializationUtil.WriteList(writer, UnlockTicketSlots);
			BinarySerializationUtil.WriteList(writer, MinMaxIngredients);
			BinarySerializationUtil.WriteList(writer, LevelBandXP);
			writer.Write(CharacterOrderChance);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			zoomOffset = BinarySerializationUtil.ReadVector3(reader);
			zoomEulers = BinarySerializationUtil.ReadVector3(reader);
			zoomFOV = reader.ReadSingle();
			TicketRepopTime = reader.ReadSingle();
			RefillTime = reader.ReadInt32();
			OrderNames = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadString, OrderNames);
			UnlockTicketSlots = BinarySerializationUtil.ReadList(reader, UnlockTicketSlots);
			MinMaxIngredients = BinarySerializationUtil.ReadList(reader, MinMaxIngredients);
			LevelBandXP = BinarySerializationUtil.ReadList(reader, LevelBandXP);
			CharacterOrderChance = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "ZOOMOFFSET":
				reader.Read();
				zoomOffset = ReaderUtil.ReadVector3(reader, converters);
				break;
			case "ZOOMEULERS":
				reader.Read();
				zoomEulers = ReaderUtil.ReadVector3(reader, converters);
				break;
			case "ZOOMFOV":
				reader.Read();
				zoomFOV = Convert.ToSingle(reader.Value);
				break;
			case "TICKETREPOPTIME":
				reader.Read();
				TicketRepopTime = Convert.ToSingle(reader.Value);
				break;
			case "REFILLTIME":
				reader.Read();
				RefillTime = Convert.ToInt32(reader.Value);
				break;
			case "ORDERNAMES":
				reader.Read();
				OrderNames = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadString, OrderNames);
				break;
			case "UNLOCKTICKETSLOTS":
				reader.Read();
				UnlockTicketSlots = ReaderUtil.PopulateList(reader, converters, UnlockTicketSlots);
				break;
			case "MINMAXINGREDIENTS":
				reader.Read();
				MinMaxIngredients = ReaderUtil.PopulateList(reader, converters, MinMaxIngredients);
				break;
			case "LEVELBANDXP":
				reader.Read();
				LevelBandXP = ReaderUtil.PopulateList(reader, converters, LevelBandXP);
				break;
			case "CHARACTERORDERCHANCE":
				reader.Read();
				CharacterOrderChance = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public override Building BuildBuilding()
		{
			return new OrderBoard(this);
		}
	}
}
