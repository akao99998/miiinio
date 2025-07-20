using System;
using System.Threading;

namespace Kampai.Util
{
	internal sealed class AsyncRoutineResultImpl : AsyncRoutineResult
	{
		public static AsyncRoutineResultImpl Start(IInvokerService invoker, Action action, Action onComplete)
		{
			AsyncRoutineResultImpl result = new AsyncRoutineResultImpl();
			ThreadPool.QueueUserWorkItem(delegate
			{
				action();
				result.IsDone = true;
				if (onComplete != null)
				{
					invoker.Add(onComplete);
				}
			});
			return result;
		}

		public static AsyncRoutineResultImpl Start(IInvokerService invoker, ContidionTask task, Action onComplete)
		{
			AsyncRoutineResultImpl result = new AsyncRoutineResultImpl();
			ThreadPool.QueueUserWorkItem(delegate
			{
				bool flag = task();
				result.IsDone = true;
				if (flag && onComplete != null)
				{
					invoker.Add(onComplete);
				}
			});
			return result;
		}
	}
}
