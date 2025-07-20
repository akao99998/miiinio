using System;
using System.IO;
using Newtonsoft.Json;
using strange.extensions.context.api;

namespace Kampai.Game.Trigger
{
	public class CaptainTeaseTriggerRewardDefinition : TriggerRewardDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1177;
			}
		}

		public int PendingRewardDefinitionID { get; set; }

		public override TriggerRewardType.Identifier type
		{
			get
			{
				return TriggerRewardType.Identifier.CaptainTease;
			}
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(PendingRewardDefinitionID);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			PendingRewardDefinitionID = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "PENDINGREWARDDEFINITIONID":
				reader.Read();
				PendingRewardDefinitionID = Convert.ToInt32(reader.Value);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}

		public override void RewardPlayer(ICrossContextCapable context)
		{
		}
	}
}
