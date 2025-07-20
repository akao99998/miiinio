using System;
using System.IO;
using Newtonsoft.Json;
using strange.extensions.context.api;

namespace Kampai.Game.Trigger
{
	public class LandExpansionTriggerConditionDefinition : TriggerConditionDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1159;
			}
		}

		public int landExpansionId { get; set; }

		public bool isPurchased { get; set; }

		public override TriggerConditionType.Identifier type
		{
			get
			{
				return TriggerConditionType.Identifier.LandExpansion;
			}
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(landExpansionId);
			writer.Write(isPurchased);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			landExpansionId = reader.ReadInt32();
			isPurchased = reader.ReadBoolean();
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
					isPurchased = Convert.ToBoolean(reader.Value);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "LANDEXPANSIONID":
				reader.Read();
				landExpansionId = Convert.ToInt32(reader.Value);
				break;
			}
			return true;
		}

		public override string ToString()
		{
			return string.Format("{0}, Operator: {1}, Type: {2}, landExpansionId: {3}, isPurchased: {4}", GetType(), base.conditionOp, type, landExpansionId, isPurchased);
		}

		public override bool IsTriggered(ICrossContextCapable gameContext)
		{
			IPlayerService instance = gameContext.injectionBinder.GetInstance<IPlayerService>();
			PurchasedLandExpansion byInstanceId = instance.GetByInstanceId<PurchasedLandExpansion>(354);
			return isPurchased == byInstanceId.HasPurchased(landExpansionId);
		}
	}
}
