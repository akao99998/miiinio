using System;
using System.IO;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class TimedQuestDefinition : QuestDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1134;
			}
		}

		public int Duration { get; set; }

		public int PushNoteWarningTime { get; set; }

		public bool Repeat { get; set; }

		public TimedQuestDefinition()
		{
			Duration = 720;
			PushNoteWarningTime = 540;
			Repeat = false;
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(Duration);
			writer.Write(PushNoteWarningTime);
			writer.Write(Repeat);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Duration = reader.ReadInt32();
			PushNoteWarningTime = reader.ReadInt32();
			Repeat = reader.ReadBoolean();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "DURATION":
				reader.Read();
				Duration = Convert.ToInt32(reader.Value);
				break;
			case "PUSHNOTEWARNINGTIME":
				reader.Read();
				PushNoteWarningTime = Convert.ToInt32(reader.Value);
				break;
			case "REPEAT":
				reader.Read();
				Repeat = Convert.ToBoolean(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
