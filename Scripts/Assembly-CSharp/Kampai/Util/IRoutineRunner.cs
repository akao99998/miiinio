using System;
using System.Collections;
using UnityEngine;

namespace Kampai.Util
{
	public interface IRoutineRunner
	{
		Coroutine StartCoroutine(IEnumerator method);

		void StopCoroutine(IEnumerator method);

		void StartTimer(string timerID, float time, Action onComplete);

		void StopTimer(string timerID);

		AsyncRoutineResult StartAsyncTask(Action task, Action onComplete = null);

		AsyncRoutineResult StartAsyncConditionTask(ContidionTask task, Action onComplete = null);
	}
}
