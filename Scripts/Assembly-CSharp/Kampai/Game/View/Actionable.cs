namespace Kampai.Game.View
{
	public interface Actionable
	{
		KampaiAction currentAction { get; }

		void EnqueueAction(KampaiAction action, bool clear = false);

		void InjectAction(KampaiAction action);

		void ReplaceCurrentAction(KampaiAction action);

		void ReplaceActionsOfType(KampaiAction action);

		void ExecuteAction(KampaiAction action);

		int GetActionQueueCount();

		void ClearActionQueue();

		KampaiAction GetNextAction();

		T GetAction<T>() where T : KampaiAction;

		void ShelveActionQueue();

		void UnshelveActionQueue();

		void LogActions();
	}
}
