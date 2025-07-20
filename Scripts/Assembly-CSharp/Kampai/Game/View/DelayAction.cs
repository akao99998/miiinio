using System.Collections;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class DelayAction : KampaiAction
	{
		private ActionableObject obj;

		private float delay;

		public DelayAction(ActionableObject obj, float delay, IKampaiLogger logger)
			: base(logger)
		{
			this.obj = obj;
			this.delay = delay;
		}

		public override void Execute()
		{
			obj.StartCoroutine(Delay(delay));
		}

		private IEnumerator Delay(float t)
		{
			yield return new WaitForSeconds(t);
			base.Done = true;
		}

		public override string ToString()
		{
			return string.Format("{0} Delay:{1}", GetType().Name, delay);
		}
	}
}
