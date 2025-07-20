using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.Transaction
{
	public class TransactionUpdateData
	{
		public UpdateType Type { get; set; }

		public int TransactionId { get; set; }

		public int InstanceId { get; set; }

		public Vector3 startPosition { get; set; }

		public bool fromGlass { get; set; }

		public int taxonomyId { get; set; }

		public int craftableXPEarned { get; set; }

		public IList<QuantityItem> Inputs { get; set; }

		public IList<QuantityItem> Outputs { get; set; }

		public IList<Instance> NewItems { get; set; }

		public TransactionTarget Target { get; set; }

		public bool IsFromPremiumSource { get; set; }

		public bool IsNotForPlayerTraining { get; set; }

		public string Source { get; set; }

		public TransactionUpdateData()
		{
			Inputs = new List<QuantityItem>();
			Outputs = new List<QuantityItem>();
		}

		public void AddInput(int id, int quantity)
		{
			Inputs.Add(new QuantityItem(id, (uint)quantity));
		}

		public void AddOutput(int id, int quantity)
		{
			Outputs.Add(new QuantityItem(id, (uint)quantity));
		}
	}
}
