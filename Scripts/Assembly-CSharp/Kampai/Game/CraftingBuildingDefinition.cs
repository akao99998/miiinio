using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class CraftingBuildingDefinition : AnimatingBuildingDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1045;
			}
		}

		public IList<RecipeDefinition> RecipeDefinitions { get; set; }

		public int InitialSlots { get; set; }

		public int MaxQueueSlots { get; set; }

		public int SlotCost { get; set; }

		public int SlotIncrementalCost { get; set; }

		public Vector3 HarvestableIconOffset { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteList(writer, RecipeDefinitions);
			writer.Write(InitialSlots);
			writer.Write(MaxQueueSlots);
			writer.Write(SlotCost);
			writer.Write(SlotIncrementalCost);
			BinarySerializationUtil.WriteVector3(writer, HarvestableIconOffset);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			RecipeDefinitions = BinarySerializationUtil.ReadList(reader, RecipeDefinitions);
			InitialSlots = reader.ReadInt32();
			MaxQueueSlots = reader.ReadInt32();
			SlotCost = reader.ReadInt32();
			SlotIncrementalCost = reader.ReadInt32();
			HarvestableIconOffset = BinarySerializationUtil.ReadVector3(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "RECIPEDEFINITIONS":
				reader.Read();
				RecipeDefinitions = ReaderUtil.PopulateList(reader, converters, RecipeDefinitions);
				break;
			case "INITIALSLOTS":
				reader.Read();
				InitialSlots = Convert.ToInt32(reader.Value);
				break;
			case "MAXQUEUESLOTS":
				reader.Read();
				MaxQueueSlots = Convert.ToInt32(reader.Value);
				break;
			case "SLOTCOST":
				reader.Read();
				SlotCost = Convert.ToInt32(reader.Value);
				break;
			case "SLOTINCREMENTALCOST":
				reader.Read();
				SlotIncrementalCost = Convert.ToInt32(reader.Value);
				break;
			case "HARVESTABLEICONOFFSET":
				reader.Read();
				HarvestableIconOffset = ReaderUtil.ReadVector3(reader, converters);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public override Building BuildBuilding()
		{
			return new CraftingBuilding(this);
		}
	}
}
