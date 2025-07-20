using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Game.Transaction;
using Kampai.Util;
using Newtonsoft.Json;
using strange.extensions.context.api;

namespace Kampai.Game.Trigger
{
	[RequiresJsonConverter]
	public abstract class TriggerRewardDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1178;
			}
		}

		public int SKUId { get; set; }

		public string buttonText { get; set; }

		public TransactionInstance transaction { get; set; }

		public IList<TriggerRewardLayout> layoutElements { get; set; }

		public TriggerRewardLayout.Layout rewardLayout { get; set; }

		public abstract TriggerRewardType.Identifier type { get; }

		public bool IsDynamicReward
		{
			get
			{
				return transaction.GetOutputCount() == 0;
			}
		}

		public virtual bool IsFree
		{
			get
			{
				return transaction.GetInputCount() == 0 && SKUId == 0;
			}
		}

		public bool HasInputs
		{
			get
			{
				return transaction.GetInputCount() != 0;
			}
		}

		public bool IsCash
		{
			get
			{
				return transaction.GetInputCount() == 0 && SKUId != 0;
			}
		}

		public virtual bool IsUniquePerInstance
		{
			get
			{
				return true;
			}
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(SKUId);
			BinarySerializationUtil.WriteString(writer, buttonText);
			BinarySerializationUtil.WriteTransactionInstance(writer, transaction);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteTriggerRewardLayout, layoutElements);
			BinarySerializationUtil.WriteEnum(writer, rewardLayout);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			SKUId = reader.ReadInt32();
			buttonText = BinarySerializationUtil.ReadString(reader);
			transaction = BinarySerializationUtil.ReadTransactionInstance(reader);
			layoutElements = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadTriggerRewardLayout, layoutElements);
			rewardLayout = BinarySerializationUtil.ReadEnum<TriggerRewardLayout.Layout>(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "SKUID":
				reader.Read();
				SKUId = Convert.ToInt32(reader.Value);
				break;
			case "BUTTONTEXT":
				reader.Read();
				buttonText = ReaderUtil.ReadString(reader, converters);
				break;
			case "TRANSACTION":
				reader.Read();
				transaction = ReaderUtil.ReadTransactionInstance(reader, converters);
				break;
			case "LAYOUTELEMENTS":
				reader.Read();
				layoutElements = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadTriggerRewardLayout, layoutElements);
				break;
			case "REWARDLAYOUT":
				reader.Read();
				rewardLayout = ReaderUtil.ReadEnum<TriggerRewardLayout.Layout>(reader);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public abstract void RewardPlayer(ICrossContextCapable context);
	}
}
