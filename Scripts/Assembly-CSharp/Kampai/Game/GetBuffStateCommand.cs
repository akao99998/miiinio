using System;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class GetBuffStateCommand : Command
	{
		[Inject]
		public BuffType buffType { get; set; }

		[Inject]
		public Action<float> callback { get; set; }

		[Inject]
		public IGuestOfHonorService gohService { get; set; }

		public override void Execute()
		{
			float currentBuffMultiplierForBuffType = gohService.GetCurrentBuffMultiplierForBuffType(buffType);
			callback(currentBuffMultiplierForBuffType);
		}
	}
}
