using Kampai.Game;

namespace Kampai.UI.View
{
	public interface IQuestWayFinderView : IWayFinderView, IWorldToGlassView
	{
		Quest CurrentActiveQuest { get; }

		void AddQuest(int questDefId);

		void RemoveQuest(int questDefId);

		bool IsNewQuestAvailable();

		bool IsQuestAvailable();

		bool IsTaskReady();

		bool IsQuestComplete();
	}
}
