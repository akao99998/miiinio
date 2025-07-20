using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class KevinCharacterDefinition : FrolicCharacterDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1079;
			}
		}

		public string WelcomeHutStateMachine { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, WelcomeHutStateMachine);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			WelcomeHutStateMachine = BinarySerializationUtil.ReadString(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "WELCOMEHUTSTATEMACHINE":
				reader.Read();
				WelcomeHutStateMachine = ReaderUtil.ReadString(reader, converters);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}

		public override Instance Build()
		{
			return new KevinCharacter(this);
		}
	}
}
