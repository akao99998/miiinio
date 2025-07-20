using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game.Transaction;
using Kampai.Game.Trigger;
using Kampai.Util;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class TriggerService : ITriggerService
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("TriggerService") as IKampaiLogger;

		private TriggerInstance currentActiveTrigger;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public IPlayerDurationService playerDurationService { get; set; }

		[Inject]
		public TriggerFiredSignal triggerFiredSignal { get; set; }

		[Inject]
		public CreateTSMCharacterWithTriggerSignal createTSMCharacterWithTriggerSignal { get; set; }

		public TriggerInstance ActiveTrigger
		{
			get
			{
				if (currentActiveTrigger == null)
				{
					Initialize();
				}
				return currentActiveTrigger;
			}
		}

		~TriggerService()
		{
			if (triggerFiredSignal != null)
			{
				triggerFiredSignal.RemoveListener(TriggerFiredCallback);
			}
		}

		private void TriggerFiredCallback(TriggerInstance triggerInstance)
		{
			TSMCharacter byInstanceId = playerService.GetByInstanceId<TSMCharacter>(301);
			if (!byInstanceId.Created)
			{
				createTSMCharacterWithTriggerSignal.Dispatch();
			}
		}

		public TriggerInstance AddActiveTrigger(TriggerDefinition triggerDefinition)
		{
			currentActiveTrigger = playerService.AddTrigger(triggerDefinition);
			currentActiveTrigger.StartGameTime = -1;
			ProcessRewardTransactionFromTriggerCondition(currentActiveTrigger);
			return currentActiveTrigger;
		}

		public void ResetCurrentTrigger()
		{
			if (currentActiveTrigger != null)
			{
				playerService.RemoveTrigger(currentActiveTrigger);
				currentActiveTrigger = null;
			}
		}

		private void ProcessRewardTransactionFromTriggerCondition(TriggerInstance trigger)
		{
			TriggerDefinition definition = trigger.Definition;
			IList<TriggerRewardDefinition> rewards = definition.rewards;
			if (rewards == null || rewards.Count < 1)
			{
				logger.Error("Current Trigger {0} doesn't have any reward defined, ingoring...", definition.ID);
				return;
			}
			TriggerRewardDefinition triggerRewardDefinition = rewards[0];
			if (triggerRewardDefinition.transaction == null)
			{
				triggerRewardDefinition.transaction = new TransactionInstance();
			}
			if (triggerRewardDefinition.transaction.Outputs == null)
			{
				triggerRewardDefinition.transaction.Outputs = new List<QuantityItem>();
			}
			for (int i = 0; i < definition.conditions.Count; i++)
			{
				TriggerConditionDefinition triggerConditionDefinition = definition.conditions[i];
				TransactionDefinition dynamicTriggerTransaction = triggerConditionDefinition.GetDynamicTriggerTransaction(gameContext);
				if (dynamicTriggerTransaction != null && dynamicTriggerTransaction.GetInputCount() > 0)
				{
					IList<QuantityItem> inputs = dynamicTriggerTransaction.Inputs;
					for (int j = 0; j < inputs.Count; j++)
					{
						triggerRewardDefinition.transaction.Outputs.Add(inputs[j]);
					}
				}
			}
		}

		public void RemoveOldTriggers()
		{
			if (playerService == null)
			{
				return;
			}
			IList<TriggerInstance> triggers = playerService.GetTriggers();
			if (triggers == null)
			{
				return;
			}
			for (int i = 0; i < triggers.Count; i++)
			{
				TriggerInstance triggerInstance = triggers[i];
				if (triggerInstance != null && triggerInstance.StartGameTime != -1)
				{
					if (triggerInstance == currentActiveTrigger)
					{
						currentActiveTrigger = null;
					}
					if (playerDurationService.GetGameTimeDuration(triggerInstance) >= triggerInstance.Definition.cooldownSeconds)
					{
						playerService.RemoveTrigger(triggerInstance);
					}
				}
			}
		}

		public void Initialize()
		{
			triggerFiredSignal.AddListener(TriggerFiredCallback);
			IList<TriggerInstance> triggers = playerService.GetTriggers();
			for (int i = 0; i < triggers.Count; i++)
			{
				TriggerInstance triggerInstance = triggers[i];
				if (triggerInstance != null && triggerInstance.StartGameTime == -1)
				{
					currentActiveTrigger = triggerInstance;
					ProcessRewardTransactionFromTriggerCondition(currentActiveTrigger);
					break;
				}
			}
		}
	}
}
