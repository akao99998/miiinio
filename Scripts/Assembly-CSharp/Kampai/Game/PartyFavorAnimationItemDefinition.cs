using System;
using System.IO;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class PartyFavorAnimationItemDefinition : ItemDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1094;
			}
		}

		public int ReferencedDefinitionID { get; set; }

		public PartyFavorAnimationItemDefinition()
		{
			base.Storable = false;
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(ReferencedDefinitionID);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			ReferencedDefinitionID = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "REFERENCEDDEFINITIONID":
				reader.Read();
				ReferencedDefinitionID = Convert.ToInt32(reader.Value);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}
	}
}
