using System.Collections;

namespace Kampai.Util
{
	public interface ICoroutineProgressMonitor
	{
		object waitForPreviousTaskToComplete { get; }

		object waitForNextFrame { get; }

		bool HasRunningTasks();

		int GetRunningTasksCount();

		void StartTask(IEnumerator enumerator, string tag);
	}
}
