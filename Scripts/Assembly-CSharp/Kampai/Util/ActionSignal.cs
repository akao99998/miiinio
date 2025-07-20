using System;
using strange.extensions.signal.impl;

namespace Kampai.Util
{
	public class ActionSignal : Signal
	{
		private Action action;

		private bool onlyFireOnce;

		private bool alreadyFired;

		public ActionSignal(Action action, bool onlyFireOnce = false)
		{
			this.action = action;
			this.onlyFireOnce = onlyFireOnce;
			AddListener(PerformAction);
		}

		private void PerformAction()
		{
			if (!onlyFireOnce || !alreadyFired)
			{
				action();
			}
		}
	}
}
