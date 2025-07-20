using System.Collections.Generic;
using System.Text;
using Kampai.Util;

namespace Kampai.Game.Transaction
{
	public class TransactionInstance : Identifiable
	{
		public int ID { get; set; }

		public IList<QuantityItem> Inputs { get; set; }

		public IList<QuantityItem> Outputs { get; set; }

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

		public TransactionDefinition ToDefinition()
		{
			TransactionDefinition transactionDefinition = new TransactionDefinition();
			transactionDefinition.ID = ID;
			transactionDefinition.Inputs = new List<QuantityItem>();
			transactionDefinition.Outputs = new List<QuantityItem>();
			if (Inputs != null)
			{
				foreach (QuantityItem input in Inputs)
				{
					transactionDefinition.Inputs.Add(new QuantityItem(input.ID, input.Quantity));
				}
			}
			if (Outputs != null)
			{
				foreach (QuantityItem output in Outputs)
				{
					transactionDefinition.Outputs.Add(new QuantityItem(output.ID, output.Quantity));
				}
			}
			return transactionDefinition;
		}
	}
}
