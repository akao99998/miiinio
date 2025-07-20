using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class UIRemovedCommand : Command
	{
		[Inject]
		public GameObject obj { get; set; }

		[Inject]
		public UIModel model { get; set; }

		public override void Execute()
		{
			model.RemoveUI(obj.GetInstanceID());
		}
	}
}
