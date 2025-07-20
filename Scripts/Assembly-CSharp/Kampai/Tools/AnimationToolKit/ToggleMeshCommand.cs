using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Tools.AnimationToolKit
{
	public class ToggleMeshCommand : Command
	{
		[Inject(AnimationToolKitElement.MINIONS)]
		public GameObject MinionGroup { get; set; }

		[Inject(AnimationToolKitElement.CHARACTERS)]
		public GameObject CharacterGroup { get; set; }

		public override void Execute()
		{
			DisableGroup(MinionGroup.transform);
			DisableGroup(CharacterGroup.transform);
		}

		private void DisableGroup(Transform transform)
		{
			bool enabled = false;
			int childCount = transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Transform child = transform.GetChild(i);
				int childCount2 = child.childCount;
				for (int j = 0; j < childCount2; j++)
				{
					Transform child2 = child.GetChild(j);
					if (!child2.name.Contains("LOD"))
					{
						continue;
					}
					SkinnedMeshRenderer component = child2.GetComponent<SkinnedMeshRenderer>();
					if (!(component == null))
					{
						if (i == 0)
						{
							enabled = !component.enabled;
						}
						component.enabled = enabled;
					}
				}
			}
		}
	}
}
