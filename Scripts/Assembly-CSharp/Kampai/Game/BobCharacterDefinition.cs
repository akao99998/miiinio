using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class BobCharacterDefinition : FrolicCharacterDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1075;
			}
		}

		public MinionAnimationDefinition CelebrateAnimation { get; set; }

		public MinionAnimationDefinition AttentionAnimation { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteObject(writer, CelebrateAnimation);
			BinarySerializationUtil.WriteObject(writer, AttentionAnimation);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			CelebrateAnimation = BinarySerializationUtil.ReadObject<MinionAnimationDefinition>(reader);
			AttentionAnimation = BinarySerializationUtil.ReadObject<MinionAnimationDefinition>(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			default:
			{
				int num;
				if (num == 1)
				{
					reader.Read();
					AttentionAnimation = FastJSONDeserializer.Deserialize<MinionAnimationDefinition>(reader, converters);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "CELEBRATEANIMATION":
				reader.Read();
				CelebrateAnimation = FastJSONDeserializer.Deserialize<MinionAnimationDefinition>(reader, converters);
				break;
			}
			return true;
		}

		public override Instance Build()
		{
			return new BobCharacter(this);
		}
	}
}
