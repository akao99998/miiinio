using System;
using System.IO;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class DynamicIngredientsDefinition : IngredientsItemDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1092;
			}
		}

		public int QuestDefinitionUnlockId { get; set; }

		public int CraftingBuildingId { get; set; }

		public bool Depreciated { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(QuestDefinitionUnlockId);
			writer.Write(CraftingBuildingId);
			writer.Write(Depreciated);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			QuestDefinitionUnlockId = reader.ReadInt32();
			CraftingBuildingId = reader.ReadInt32();
			Depreciated = reader.ReadBoolean();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "QUESTDEFINITIONUNLOCKID":
				reader.Read();
				QuestDefinitionUnlockId = Convert.ToInt32(reader.Value);
				break;
			case "CRAFTINGBUILDINGID":
				reader.Read();
				CraftingBuildingId = Convert.ToInt32(reader.Value);
				break;
			case "DEPRECIATED":
				reader.Read();
				Depreciated = Convert.ToBoolean(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
