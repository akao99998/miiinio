using System;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class GenerateNewMasterPlanCommand : Command
	{
		[Inject]
		public Boxed<Action> newMasterPlanGeneratedCallback { get; set; }

		[Inject]
		public IMasterPlanService masterPlanService { get; set; }

		public override void Execute()
		{
			masterPlanService.CreateNewMasterPlan();
			if (newMasterPlanGeneratedCallback.Value != null)
			{
				newMasterPlanGeneratedCallback.Value();
			}
		}
	}
}
