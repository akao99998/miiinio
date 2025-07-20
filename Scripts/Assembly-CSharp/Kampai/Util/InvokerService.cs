using System;
using System.Collections.Generic;
using System.Threading;

namespace Kampai.Util
{
	public class InvokerService : IInvokerService
	{
		private Queue<Action> work = new Queue<Action>();

		private Mutex mutex = new Mutex(false);

		public void Add(Action a)
		{
			try
			{
				mutex.WaitOne();
				work.Enqueue(a);
			}
			finally
			{
				mutex.ReleaseMutex();
			}
		}

		public void Update()
		{
			if (work.Count <= 0)
			{
				return;
			}
			try
			{
				mutex.WaitOne();
				while (work.Count > 0)
				{
					Action action = work.Dequeue();
					action();
				}
			}
			finally
			{
				mutex.ReleaseMutex();
			}
		}
	}
}
