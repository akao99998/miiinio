using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	public class CancelablePathAction : PathAction
	{
		private Signal cancelPathActionSignal;

		private float maximumDeviationFromDestination;

		public CancelablePathAction(Signal cancelPathActionSignal, float maximumDeviationFromDestination, ActionableObject obj, IList<Vector3> path, float time, IKampaiLogger logger)
			: base(obj, path, time, logger)
		{
			this.cancelPathActionSignal = cancelPathActionSignal;
			this.maximumDeviationFromDestination = maximumDeviationFromDestination;
		}

		protected override void PathFinished()
		{
			Vector3 a = path[path.Count - 1];
			Vector3 position = obj.transform.position;
			float num = Vector3.Distance(a, position);
			if (num > maximumDeviationFromDestination)
			{
				cancelPathActionSignal.Dispatch();
			}
			else
			{
				base.PathFinished();
			}
		}
	}
}
