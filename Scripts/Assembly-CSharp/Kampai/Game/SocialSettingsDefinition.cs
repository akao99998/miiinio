using System;
using System.IO;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class SocialSettingsDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1140;
			}
		}

		public bool ShowFacebookConnectPopup { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(ShowFacebookConnectPopup);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			ShowFacebookConnectPopup = reader.ReadBoolean();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "SHOWFACEBOOKCONNECTPOPUP":
				reader.Read();
				ShowFacebookConnectPopup = Convert.ToBoolean(reader.Value);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}
	}
}
