using System;
using System.IO;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class RecipeDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1046;
			}
		}

		public int ItemID { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(ItemID);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			ItemID = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "ITEMID":
				reader.Read();
				ItemID = Convert.ToInt32(reader.Value);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}
	}
}
