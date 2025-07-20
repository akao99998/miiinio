using System.Collections.Generic;
using System.IO;
using System.Text;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game.Transaction
{
	[RequiresJsonConverter]
	public class TransactionDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1007;
			}
		}

		public IList<QuantityItem> Inputs { get; set; }

		public IList<QuantityItem> Outputs { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteList(writer, Inputs);
			BinarySerializationUtil.WriteList(writer, Outputs);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Inputs = BinarySerializationUtil.ReadList(reader, Inputs);
			Outputs = BinarySerializationUtil.ReadList(reader, Outputs);
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
					Outputs = ReaderUtil.PopulateList(reader, converters, Outputs);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "INPUTS":
				reader.Read();
				Inputs = ReaderUtil.PopulateList(reader, converters, Inputs);
				break;
			}
			return true;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.ToString());
			stringBuilder.Append(" [I=");
			AppendQuantityItems(Inputs, stringBuilder);
			stringBuilder.Append("][O=");
			AppendQuantityItems(Outputs, stringBuilder);
			stringBuilder.Append("]");
			return stringBuilder.ToString();
		}

		private void AppendQuantityItems(ICollection<QuantityItem> group, StringBuilder sb)
		{
			bool flag = false;
			if (group == null)
			{
				return;
			}
			foreach (QuantityItem item in group)
			{
				if (flag)
				{
					sb.Append(" ");
				}
				sb.Append(item.ID);
				sb.Append("@");
				sb.Append(item.Quantity);
				flag = true;
			}
		}

		public TransactionInstance ToInstance()
		{
			TransactionInstance transactionInstance = new TransactionInstance();
			transactionInstance.ID = ID;
			transactionInstance.Inputs = new List<QuantityItem>();
			transactionInstance.Outputs = new List<QuantityItem>();
			if (Inputs != null)
			{
				foreach (QuantityItem input in Inputs)
				{
					transactionInstance.Inputs.Add(new QuantityItem(input.ID, input.Quantity));
				}
			}
			if (Outputs != null)
			{
				foreach (QuantityItem output in Outputs)
				{
					transactionInstance.Outputs.Add(new QuantityItem(output.ID, output.Quantity));
				}
			}
			return transactionInstance;
		}

		public TransactionDefinition CopyTransaction()
		{
			TransactionDefinition transactionDefinition = new TransactionDefinition();
			transactionDefinition.ID = ID;
			transactionDefinition.LocalizedKey = base.LocalizedKey;
			transactionDefinition.Disabled = base.Disabled;
			if (Inputs != null)
			{
				transactionDefinition.Inputs = new List<QuantityItem>(Inputs.Count);
				foreach (QuantityItem input in Inputs)
				{
					transactionDefinition.Inputs.Add(new QuantityItem(input.ID, input.Quantity));
				}
			}
			if (Outputs != null)
			{
				transactionDefinition.Outputs = new List<QuantityItem>(Outputs.Count);
				foreach (QuantityItem output in Outputs)
				{
					transactionDefinition.Outputs.Add(new QuantityItem(output.ID, output.Quantity));
				}
			}
			return transactionDefinition;
		}
	}
}
