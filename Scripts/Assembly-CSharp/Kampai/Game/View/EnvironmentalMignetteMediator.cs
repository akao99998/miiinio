using Kampai.Common;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.Game.View
{
	public class EnvironmentalMignetteMediator : EventMediator
	{
		private const float RANGE_FOR_MINION_REACTION = 10f;

		[Inject]
		public MinionReactInRadiusSignal minionReactInRadiusSignal { get; set; }

		[Inject]
		public EnvironmentalMignetteView view { get; set; }

		[Inject]
		public EnvironmentalMignetteTappedSignal environmentalMignetteTappedSignal { get; set; }

		public override void OnRegister()
		{
			environmentalMignetteTappedSignal.AddListener(OnTappedSignal);
		}

		public override void OnRemove()
		{
			environmentalMignetteTappedSignal.RemoveListener(OnTappedSignal);
		}

		public void OnTappedSignal(GameObject tappedGO)
		{
			view.AnimateEnvironmentalMignette(tappedGO);
			minionReactInRadiusSignal.Dispatch(10f, tappedGO.transform.position);
		}
	}
}
