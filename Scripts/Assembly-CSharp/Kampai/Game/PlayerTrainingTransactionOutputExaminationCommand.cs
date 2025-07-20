using Kampai.Game.Transaction;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class PlayerTrainingTransactionOutputExaminationCommand : Command
	{
		[Inject]
		public TransactionUpdateData updateData { get; set; }

		[Inject]
		public DisplayPlayerTrainingSignal displaySignal { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		public override void Execute()
		{
			if (updateData.NewItems != null && updateData.NewItems.Count > 0)
			{
				foreach (Instance newItem in updateData.NewItems)
				{
					if (newItem.Definition == null)
					{
						continue;
					}
					DropItemDefinition dropItemDefinition = newItem.Definition as DropItemDefinition;
					if (dropItemDefinition != null)
					{
						int type = 0;
						switch (dropItemDefinition.dropType)
						{
						case DropType.DEBRIS:
							type = 19000000;
							break;
						case DropType.LAND_EXPAND:
							type = 19000001;
							break;
						case DropType.STORAGE:
							type = 19000002;
							break;
						}
						displaySignal.Dispatch(type, false, new Signal<bool>());
						return;
					}
				}
			}
			if (updateData.Outputs == null)
			{
				return;
			}
			foreach (QuantityItem output in updateData.Outputs)
			{
				ItemDefinition definition;
				if (definitionService.TryGet<ItemDefinition>(output.ID, out definition) && definition != null && definition.PlayerTrainingDefinitionID > 0 && !updateData.IsNotForPlayerTraining)
				{
					displaySignal.Dispatch(definition.PlayerTrainingDefinitionID, false, new Signal<bool>());
					break;
				}
			}
		}
	}
}
