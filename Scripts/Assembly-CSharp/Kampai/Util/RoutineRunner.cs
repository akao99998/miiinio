using System;
using System.Collections;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.Util
{
	public class RoutineRunner : IRoutineRunner
	{
		private RoutineRunnerBehaviour mb;

		[Inject(ContextKeys.CONTEXT_VIEW)]
		public GameObject contextView { get; set; }

		[Inject]
		public IInvokerService invoker { get; set; }

		[PostConstruct]
		public void PostConstruct()
		{
			mb = contextView.GetComponent<RoutineRunnerBehaviour>();
			if (mb == null)
			{
				mb = contextView.AddComponent<RoutineRunnerBehaviour>();
			}
		}

		private bool IsBehaviourAlive()
		{
			return mb != null;
		}

		public Coroutine StartCoroutine(IEnumerator method)
		{
			return (!IsBehaviourAlive()) ? null : mb.StartCoroutine(method);
		}

		public void StopCoroutine(IEnumerator method)
		{
			if (IsBehaviourAlive())
			{
				mb.StopCoroutine(method);
			}
		}

		public void StartTimer(string timerID, float time, Action onComplete)
		{
			if (IsBehaviourAlive())
			{
				mb.StartTimer(timerID, time, onComplete);
			}
		}

		public void StopTimer(string timerID)
		{
			if (IsBehaviourAlive())
			{
				mb.StopTimer(timerID);
			}
		}

		public AsyncRoutineResult StartAsyncTask(Action task, Action onComplete)
		{
			return AsyncRoutineResultImpl.Start(invoker, task, onComplete);
		}

		public AsyncRoutineResult StartAsyncConditionTask(ContidionTask task, Action onComplete = null)
		{
			return AsyncRoutineResultImpl.Start(invoker, task, onComplete);
		}
	}
}
