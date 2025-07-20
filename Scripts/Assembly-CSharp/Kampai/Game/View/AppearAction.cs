using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class AppearAction : KampaiAction
	{
		private ActionableObject obj;

		private Vector3 position;

		public AppearAction(ActionableObject obj, Vector3 position, IKampaiLogger logger)
			: base(logger)
		{
			this.obj = obj;
			this.position = position;
		}

		public override void Execute()
		{
			obj.transform.position = position;
			base.Done = true;
		}
	}
}
