using strange.extensions.mediation.impl;

namespace Kampai.Game.View
{
	public class VillainMediator : Mediator
	{
		[Inject]
		public VillainView view { get; set; }

		[Inject]
		public InitializeVillainSignal initVillainSignal { get; set; }

		[Inject]
		public DisplayVillainSignal displaySignal { get; set; }

		[Inject]
		public SetVillainLairAnimationTriggerSignal beginAnimationSignal { get; set; }

		[Inject]
		public SetMasterPlanRewardAnimatorSignal setAnimatorSingal { get; set; }

		public override void OnRegister()
		{
			initVillainSignal.AddListener(view.InitializeVillain);
			displaySignal.AddListener(view.DisplayVillain);
			beginAnimationSignal.AddListener(view.SetAnimTrigger);
			setAnimatorSingal.AddListener(view.SetMasterPlanRewardAnimation);
		}

		public override void OnRemove()
		{
			initVillainSignal.RemoveListener(view.InitializeVillain);
			displaySignal.RemoveListener(view.DisplayVillain);
			beginAnimationSignal.RemoveListener(view.SetAnimTrigger);
			setAnimatorSingal.RemoveListener(view.SetMasterPlanRewardAnimation);
		}
	}
}
