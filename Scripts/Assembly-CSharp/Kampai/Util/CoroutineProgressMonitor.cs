using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Kampai.Util
{
	public class CoroutineProgressMonitor : ICoroutineProgressMonitor
	{
		private sealed class Task
		{
			public List<IEnumerator> CoroutinesStack = new List<IEnumerator>(2);

			public string Tag = string.Empty;

			public object Current;

			public void Reset(IEnumerator coroutine, string tag)
			{
				CoroutinesStack.Add(coroutine);
				Tag = tag;
			}

			public bool NextStep()
			{
				int num = CoroutinesStack.Count - 1;
				IEnumerator enumerator = CoroutinesStack[num];
				if (enumerator.MoveNext())
				{
					Current = enumerator.Current;
					return true;
				}
				Current = null;
				CoroutinesStack.RemoveAt(num);
				return num > 0 && NextStep();
			}
		}

		private const float allowedTime = 3f;

		private static object _waitForPreviousTaskToComplete = new object();

		private static object _waitForNextFrame = new object();

		private List<Task> tasksPool = new List<Task>(4);

		private Queue<Task> tasksQueue = new Queue<Task>(4);

		private Queue<Task> waitQueue = new Queue<Task>(1);

		private List<Task> tasksWaitingForNextFrame = new List<Task>(1);

		private Stopwatch timer = Stopwatch.StartNew();

		private long spentTime;

		private bool updateRegistered;

		[Inject]
		public IUpdateRunner updateRunner { get; set; }

		public object waitForPreviousTaskToComplete
		{
			get
			{
				return _waitForPreviousTaskToComplete;
			}
		}

		public object waitForNextFrame
		{
			get
			{
				return _waitForNextFrame;
			}
		}

		public bool HasRunningTasks()
		{
			return GetRunningTasksCount() > 0;
		}

		public int GetRunningTasksCount()
		{
			return tasksQueue.Count + waitQueue.Count + tasksWaitingForNextFrame.Count;
		}

		public void StartTask(IEnumerator enumerator, string tag)
		{
			if (!updateRegistered)
			{
				updateRunner.Subscribe(Update);
				updateRegistered = true;
			}
			Task newTask = GetNewTask();
			newTask.Reset(enumerator, tag);
			spentTime += IntegrateTask(newTask);
		}

		private Task GetNewTask()
		{
			if (tasksPool.Count == 0)
			{
				return new Task();
			}
			int index = tasksPool.Count - 1;
			Task result = tasksPool[index];
			tasksPool.RemoveAt(index);
			return result;
		}

		private void ReleaseTask(Task task)
		{
			tasksPool.Add(task);
		}

		private void Update()
		{
			float unscaledTime = Time.unscaledTime;
			int num = 64;
			for (int i = 0; i < tasksWaitingForNextFrame.Count; i++)
			{
				tasksQueue.Enqueue(tasksWaitingForNextFrame[i]);
			}
			tasksWaitingForNextFrame.Clear();
			while (Time.realtimeSinceStartup - unscaledTime < 3f && num > 0 && tasksQueue.Count > 0)
			{
				spentTime += Integrate();
				num--;
			}
			spentTime = 0L;
			if (!HasRunningTasks())
			{
				updateRegistered = false;
				updateRunner.Unsubscribe(Update);
			}
		}

		private long IntegrateTask(Task task)
		{
			long elapsedMilliseconds = timer.ElapsedMilliseconds;
			if (!task.NextStep())
			{
				ReleaseTask(task);
			}
			else
			{
				object current = task.Current;
				if (current == _waitForPreviousTaskToComplete)
				{
					waitQueue.Enqueue(task);
				}
				else if (current == _waitForNextFrame)
				{
					tasksWaitingForNextFrame.Add(task);
				}
				else if (current != null)
				{
					IEnumerator enumerator = current as IEnumerator;
					if (enumerator != null)
					{
						task.CoroutinesStack.Add(enumerator);
						IntegrateTask(task);
					}
				}
				else
				{
					tasksQueue.Enqueue(task);
				}
			}
			return timer.ElapsedMilliseconds - elapsedMilliseconds;
		}

		private long Integrate()
		{
			if (tasksQueue.Count == 0)
			{
				return 0L;
			}
			long result = IntegrateTask(tasksQueue.Dequeue());
			if (tasksQueue.Count == 0 && waitQueue.Count > 0)
			{
				tasksWaitingForNextFrame.Add(waitQueue.Dequeue());
			}
			return result;
		}
	}
}
