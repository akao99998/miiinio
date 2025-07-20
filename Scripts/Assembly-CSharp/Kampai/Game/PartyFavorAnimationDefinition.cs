using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class PartyFavorAnimationDefinition : AnimationDefinition
	{
		public const int INFINITE_MINIONS = 4;

		public override int TypeCode
		{
			get
			{
				return 1113;
			}
		}

		public int AnimationID { get; set; }

		public int FootprintID { get; set; }

		public string Prefab { get; set; }

		public int ItemID { get; set; }

		public int UnlockId { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(AnimationID);
			writer.Write(FootprintID);
			BinarySerializationUtil.WriteString(writer, Prefab);
			writer.Write(ItemID);
			writer.Write(UnlockId);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			AnimationID = reader.ReadInt32();
			FootprintID = reader.ReadInt32();
			Prefab = BinarySerializationUtil.ReadString(reader);
			ItemID = reader.ReadInt32();
			UnlockId = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "ANIMATIONID":
				reader.Read();
				AnimationID = Convert.ToInt32(reader.Value);
				break;
			case "FOOTPRINTID":
				reader.Read();
				FootprintID = Convert.ToInt32(reader.Value);
				break;
			case "PREFAB":
				reader.Read();
				Prefab = ReaderUtil.ReadString(reader, converters);
				break;
			case "ITEMID":
				reader.Read();
				ItemID = Convert.ToInt32(reader.Value);
				break;
			case "UNLOCKID":
				reader.Read();
				UnlockId = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
