using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class TeleportAction : KampaiAction
	{
		private ActionableObject obj;

		private Vector3 position;

		private Vector3 eulerAngles;

		public TeleportAction(ActionableObject obj, Vector3 position, Vector3 eulerAngles, IKampaiLogger logger)
			: base(logger)
		{
			this.obj = obj;
			this.position = position;
			this.eulerAngles = eulerAngles;
		}

		public override void Execute()
		{
			Transform transform = obj.transform;
			transform.position = position;
			transform.eulerAngles = eulerAngles;
			base.Done = true;
		}
	}
}
