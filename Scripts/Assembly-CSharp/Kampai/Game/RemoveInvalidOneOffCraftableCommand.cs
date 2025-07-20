using System.Collections.Generic;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class RemoveInvalidOneOffCraftableCommand : Command
	{
		[Inject]
		public IPlayerService playerService { get; set; }

		public override void Execute()
		{
			IList<Instance> instancesByDefinition = playerService.GetInstancesByDefinition<DynamicIngredientsDefinition>();
			foreach (Instance item in instancesByDefinition)
			{
				if ((item.Definition as DynamicIngredientsDefinition).Depreciated)
				{
					playerService.Remove(item);
				}
			}
		}
	}
}
