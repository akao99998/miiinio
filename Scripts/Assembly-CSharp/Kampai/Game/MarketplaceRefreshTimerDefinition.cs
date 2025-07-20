using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class MarketplaceRefreshTimerDefinition : Definition, IBuilder<Instance>
	{
		public override int TypeCode
		{
			get
			{
				return 1105;
			}
		}

		public int RefreshTimeSeconds { get; set; }

		public int RushCost { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(RefreshTimeSeconds);
			writer.Write(RushCost);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			RefreshTimeSeconds = reader.ReadInt32();
			RushCost = reader.ReadInt32();
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
					RushCost = Convert.ToInt32(reader.Value);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "REFRESHTIMESECONDS":
				reader.Read();
				RefreshTimeSeconds = Convert.ToInt32(reader.Value);
				break;
			}
			return true;
		}

		public Instance Build()
		{
			return new MarketplaceRefreshTimer(this);
		}
	}
}
