using System.IO;
using Kampai.Util;
using Newtonsoft.Json;
using strange.extensions.context.api;

namespace Kampai.Game.Trigger
{
	public class SegmentTriggerConditionDefinition : TriggerConditionDefinition
	{
		public string Segment;

		public override int TypeCode
		{
			get
			{
				return 1172;
			}
		}

		public override TriggerConditionType.Identifier type
		{
			get
			{
				return TriggerConditionType.Identifier.Segment;
			}
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, Segment);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Segment = BinarySerializationUtil.ReadString(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "SEGMENT":
				reader.Read();
				Segment = ReaderUtil.ReadString(reader, converters);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}

		public override string ToString()
		{
			return string.Format("{0}, Operator: {1}, Type: {2}, Segment: {3}", GetType(), base.conditionOp, type, Segment);
		}

		public override bool IsTriggered(ICrossContextCapable gameContext)
		{
			IPlayerService instance = gameContext.injectionBinder.GetInstance<IPlayerService>();
			return instance.IsInSegment(Segment);
		}
	}
}
