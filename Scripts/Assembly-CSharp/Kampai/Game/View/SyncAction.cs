using System.Collections.Generic;
using Kampai.Util;

namespace Kampai.Game.View
{
	public class SyncAction : KampaiAction
	{
		protected readonly ICollection<ActionableObject> syncObjects;

		public SyncAction(ICollection<ActionableObject> syncObjects, IKampaiLogger logger)
			: base(logger)
		{
			this.syncObjects = syncObjects;
		}

		public override void LateUpdate()
		{
			foreach (ActionableObject syncObject in syncObjects)
			{
				if (syncObject.currentAction != this)
				{
					return;
				}
			}
			base.Done = true;
		}
	}
}
