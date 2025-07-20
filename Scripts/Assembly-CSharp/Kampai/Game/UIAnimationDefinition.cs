using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class UIAnimationDefinition : AnimationDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1084;
			}
		}

		public string AnimationClipName { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, AnimationClipName);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			AnimationClipName = BinarySerializationUtil.ReadString(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "ANIMATIONCLIPNAME":
				reader.Read();
				AnimationClipName = ReaderUtil.ReadString(reader, converters);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}
	}
}
