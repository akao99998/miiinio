using Kampai.Common;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game.View
{
	public class CreateInnerTubeCommand : Command
	{
		[Inject]
		public int routeIndex { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public IRandomService randomService { get; set; }

		public override void Execute()
		{
			GameObject original = KampaiResources.Load("Unique_InnerTube_Prefab") as GameObject;
			GameObject gameObject = Object.Instantiate(original);
			InnerTubeObject component = gameObject.GetComponent<InnerTubeObject>();
			component.SetTubeNumberAndFloatAway(routeIndex, routineRunner, randomService);
		}
	}
}
