using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public abstract class TaskableBuildingDefinition : AnimatingBuildingDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1049;
			}
		}

		public IList<SlotUnlock> SlotUnlocks { get; set; }

		public int DefaultSlots { get; set; }

		public int RushCost { get; set; }

		public string ModalDescription { get; set; }

		public int GagID { get; set; }

		public Vector3 HarvestableIconOffset { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteSlotUnlock, SlotUnlocks);
			writer.Write(DefaultSlots);
			writer.Write(RushCost);
			BinarySerializationUtil.WriteString(writer, ModalDescription);
			writer.Write(GagID);
			BinarySerializationUtil.WriteVector3(writer, HarvestableIconOffset);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			SlotUnlocks = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadSlotUnlock, SlotUnlocks);
			DefaultSlots = reader.ReadInt32();
			RushCost = reader.ReadInt32();
			ModalDescription = BinarySerializationUtil.ReadString(reader);
			GagID = reader.ReadInt32();
			HarvestableIconOffset = BinarySerializationUtil.ReadVector3(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "SLOTUNLOCKS":
				reader.Read();
				SlotUnlocks = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadSlotUnlock, SlotUnlocks);
				break;
			case "DEFAULTSLOTS":
				reader.Read();
				DefaultSlots = Convert.ToInt32(reader.Value);
				break;
			case "RUSHCOST":
				reader.Read();
				RushCost = Convert.ToInt32(reader.Value);
				break;
			case "MODALDESCRIPTION":
				reader.Read();
				ModalDescription = ReaderUtil.ReadString(reader, converters);
				break;
			case "GAGID":
				reader.Read();
				GagID = Convert.ToInt32(reader.Value);
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
	}
}
