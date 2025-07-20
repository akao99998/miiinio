using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class SelectedAction : KampaiAction
	{
		private Boxed<Vector3> RunLocation;

		private MinionMoveToSignal MoveSignal;

		private int MinionId;

		public SelectedAction(int minionId, Boxed<Vector3> runLocation, MinionMoveToSignal moveSignal, IKampaiLogger logger)
			: base(logger)
		{
			RunLocation = runLocation;
			MoveSignal = moveSignal;
			MinionId = minionId;
		}

		public override void Execute()
		{
			if (RunLocation != null)
			{
				MoveSignal.Dispatch(MinionId, RunLocation.Value, false);
			}
			else
			{
				base.Done = true;
			}
		}
	}
}
