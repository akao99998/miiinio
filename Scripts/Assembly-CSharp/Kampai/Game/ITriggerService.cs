using Kampai.Game.Trigger;

namespace Kampai.Game
{
	public interface ITriggerService
	{
		TriggerInstance ActiveTrigger { get; }

		TriggerInstance AddActiveTrigger(TriggerDefinition triggerDefinition);

		void RemoveOldTriggers();

		void ResetCurrentTrigger();

		void Initialize();
	}
}
