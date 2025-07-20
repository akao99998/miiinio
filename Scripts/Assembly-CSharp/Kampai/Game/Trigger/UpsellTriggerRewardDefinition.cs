using System;
using System.IO;
using Newtonsoft.Json;
using strange.extensions.context.api;
using strange.extensions.injector.api;

namespace Kampai.Game.Trigger
{
	public class UpsellTriggerRewardDefinition : TriggerRewardDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1186;
			}
		}

		public int upsellId { get; set; }

		public override TriggerRewardType.Identifier type
		{
			get
			{
				return TriggerRewardType.Identifier.Upsell;
			}
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(upsellId);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			upsellId = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "UPSELLID":
				reader.Read();
				upsellId = Convert.ToInt32(reader.Value);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}

		public override void RewardPlayer(ICrossContextCapable context)
		{
			ICrossContextInjectionBinder injectionBinder = context.injectionBinder;
			if (injectionBinder != null)
			{
				ReconcileSalesSignal instance = injectionBinder.GetInstance<ReconcileSalesSignal>();
				if (instance != null)
				{
					instance.Dispatch(upsellId);
				}
			}
		}
	}
}
