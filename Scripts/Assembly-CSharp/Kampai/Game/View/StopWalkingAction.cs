using System.Collections.Generic;
using Kampai.Util;

namespace Kampai.Game.View
{
	public class StopWalkingAction : SetAnimatorAction
	{
		private static readonly Dictionary<string, object> stopWalkingArgs = new Dictionary<string, object>();

		public StopWalkingAction(ActionableObject obj, IKampaiLogger logger)
			: base(obj, null, logger, stopWalkingArgs)
		{
			if (stopWalkingArgs.Count == 0)
			{
				stopWalkingArgs.Add("isMoving", false);
			}
		}

		public override void Execute()
		{
			if (obj.GetDefaultAnimControllerName().Equals(obj.GetCurrentAnimControllerName()))
			{
				base.Execute();
			}
			base.Done = true;
		}
	}
}
