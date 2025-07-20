using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class EnableVillainIslandCollidersCommand : Command
	{
		[Inject]
		public bool enable { get; set; }

		public override void Execute()
		{
			GameObject gameObject = GameObject.Find("Terrain_PreFab/Terrain_VillainIsland");
			if (gameObject != null)
			{
				VillainIslandLocation component = gameObject.GetComponent<VillainIslandLocation>();
				if (component != null)
				{
					component.EnableColliders(enable);
				}
			}
		}
	}
}
