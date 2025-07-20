using System.Collections.Generic;
using Kampai.Common;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class SelectionCompleteCommand : Command
	{
		[Inject]
		public Vector3 centerPoint { get; set; }

		[Inject]
		public StartGroupGachaSignal startGroupGachaSignal { get; set; }

		[Inject]
		public PickControllerModel model { get; set; }

		public override void Execute()
		{
			if (model.SelectedMinions.Count > 0 && !model.HasPlayedGacha)
			{
				HashSet<int> minionIds = new HashSet<int>(model.SelectedMinions.Keys);
				startGroupGachaSignal.Dispatch(new MinionAnimationInstructions(minionIds, new Boxed<Vector3>(centerPoint)));
				model.HasPlayedGacha = true;
			}
		}
	}
}
