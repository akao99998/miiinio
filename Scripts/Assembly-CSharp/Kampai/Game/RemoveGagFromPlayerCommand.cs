using System.Collections.Generic;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class RemoveGagFromPlayerCommand : Command
	{
		[Inject]
		public int id { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		public override void Execute()
		{
			List<Item> list = playerService.GetByDefinitionId<Item>(id) as List<Item>;
			if (list != null && list.Count > 0)
			{
				playerService.Remove(list[0]);
			}
		}
	}
}
