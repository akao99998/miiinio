using Kampai.Common;
using Kampai.Game.Transaction;

namespace Kampai.Game
{
	public class MIBService : IMIBService
	{
		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IRandomService randomService { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistanceService { get; set; }

		public TransactionDefinition PickWeightedTransaction(int weightedDefId)
		{
			WeightedDefinition definition = null;
			if (!definitionService.TryGet<WeightedDefinition>(weightedDefId, out definition))
			{
				return null;
			}
			WeightedInstance weightedInstance = playerService.GetWeightedInstance(weightedDefId, definition);
			int iD = weightedInstance.NextPick(randomService).ID;
			return definitionService.Get<TransactionDefinition>(iD);
		}

		public ItemDefinition GetItemDefinition(TransactionDefinition transactionDef)
		{
			if (transactionDef == null || transactionDef.Outputs.Count <= 0)
			{
				return null;
			}
			return definitionService.Get<ItemDefinition>(transactionDef.Outputs[0].ID);
		}

		public ItemDefinition[] GetItemDefinitions(int weightedDefId)
		{
			WeightedDefinition weightedDefinition = definitionService.Get<WeightedDefinition>(weightedDefId);
			ItemDefinition[] array = new ItemDefinition[weightedDefinition.Entities.Count];
			for (int i = 0; i < weightedDefinition.Entities.Count; i++)
			{
				TransactionDefinition transactionDef = definitionService.Get<TransactionDefinition>(weightedDefinition.Entities[i].ID);
				array[i] = GetItemDefinition(transactionDef);
			}
			return array;
		}

		public bool IsUserReturning()
		{
			return localPersistanceService.HasKeyPlayer("MIBPlacementSelected") && localPersistanceService.GetDataIntPlayer("MIBPlacementSelected") == 1;
		}

		public void SetReturningKey()
		{
			localPersistanceService.PutDataIntPlayer("MIBPlacementSelected", 1);
		}

		public void ClearReturningKey()
		{
			localPersistanceService.DeleteKeyPlayer("MIBPlacementSelected");
		}
	}
}
