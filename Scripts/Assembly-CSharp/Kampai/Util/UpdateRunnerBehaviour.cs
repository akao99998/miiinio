using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kampai.Util
{
	public class UpdateRunnerBehaviour : MonoBehaviour
	{
		private List<Action> actionList = new List<Action>();

		private Action updateAction;

		public void Subscribe(Action action)
		{
			updateAction = (Action)Delegate.Combine(updateAction, action);
			if (!actionList.Contains(action))
			{
				actionList.Add(action);
			}
		}

		public void Unsubscribe(Action action)
		{
			updateAction = (Action)Delegate.Remove(updateAction, action);
			if (actionList.Contains(action))
			{
				actionList.Remove(action);
			}
		}

		private void Update()
		{
			if (updateAction != null)
			{
				updateAction();
			}
		}

		private void OnDestroy()
		{
			foreach (Action action in actionList)
			{
				updateAction = (Action)Delegate.Remove(updateAction, action);
			}
		}
	}
}
