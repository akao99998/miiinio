using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;
using strange.extensions.context.api;

namespace Kampai.Game.Trigger
{
	public class HelpButtonTriggerConditionDefinition : TriggerConditionDefinition
	{
		public enum HelpButtonTriggerMode
		{
			CLICKS = 0,
			TIME_SINCE_LAST_CLICK = 1
		}

		public override int TypeCode
		{
			get
			{
				return 1157;
			}
		}

		public HelpButtonTriggerMode mode { get; set; }

		public int tipDefinitionId { get; set; }

		public int quantity { get; set; }

		public override TriggerConditionType.Identifier type
		{
			get
			{
				return TriggerConditionType.Identifier.HelpButton;
			}
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteEnum(writer, mode);
			writer.Write(tipDefinitionId);
			writer.Write(quantity);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			mode = BinarySerializationUtil.ReadEnum<HelpButtonTriggerMode>(reader);
			tipDefinitionId = reader.ReadInt32();
			quantity = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "MODE":
				reader.Read();
				mode = ReaderUtil.ReadEnum<HelpButtonTriggerMode>(reader);
				break;
			case "TIPDEFINITIONID":
				reader.Read();
				tipDefinitionId = Convert.ToInt32(reader.Value);
				break;
			case "QUANTITY":
				reader.Read();
				quantity = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public override bool IsTriggered(ICrossContextCapable gameContext)
		{
			IHelpTipTrackingService instance = gameContext.injectionBinder.GetInstance<IHelpTipTrackingService>();
			switch (mode)
			{
			case HelpButtonTriggerMode.CLICKS:
				return TestOperator(quantity, instance.GetHelpTipShowCount(tipDefinitionId));
			case HelpButtonTriggerMode.TIME_SINCE_LAST_CLICK:
				return TestOperator(quantity, instance.GetSecondsSinceHelpTipShown(tipDefinitionId));
			default:
				return false;
			}
		}
	}
}
