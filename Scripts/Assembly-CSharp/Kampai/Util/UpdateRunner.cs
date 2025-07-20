using System;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.Util
{
	public class UpdateRunner : IUpdateRunner
	{
		private UpdateRunnerBehaviour ub;

		[Inject(ContextKeys.CONTEXT_VIEW)]
		public GameObject contextView { get; set; }

		[PostConstruct]
		public void PostConstruct()
		{
			ub = contextView.GetComponent<UpdateRunnerBehaviour>();
			if (ub == null)
			{
				ub = contextView.AddComponent<UpdateRunnerBehaviour>();
			}
		}

		public void Subscribe(Action action)
		{
			ub.Subscribe(action);
		}

		public void Unsubscribe(Action action)
		{
			ub.Unsubscribe(action);
		}
	}
}
