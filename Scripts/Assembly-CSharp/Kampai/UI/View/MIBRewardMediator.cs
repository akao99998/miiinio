using Kampai.Game;
using Kampai.Game.Transaction;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class MIBRewardMediator : Mediator
	{
		[Inject]
		public MIBRewardView view { get; set; }

		[Inject]
		public GrantMIBRewardsSignal grantMIBRewardsSignal { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrimSignal { get; set; }

		public override void OnRegister()
		{
			view.mibRewardAnimationCompleteSignal.AddListener(OnAnimationComplete);
		}

		public override void OnRemove()
		{
			view.mibRewardAnimationCompleteSignal.RemoveListener(OnAnimationComplete);
		}

		private void OnAnimationComplete(TransactionDefinition pickedTransactionDef, Vector3 tweenLocation)
		{
			grantMIBRewardsSignal.Dispatch(MIBRewardType.ON_RETURN, pickedTransactionDef, tweenLocation);
			hideSkrimSignal.Dispatch("MIBRewardScreenSkrim");
			guiService.Execute(GUIOperation.Unload, "screen_MessageInABottle");
		}
	}
}
