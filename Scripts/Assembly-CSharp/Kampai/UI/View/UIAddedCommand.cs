using System;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class UIAddedCommand : Command
	{
		[Inject]
		public GameObject obj { get; set; }

		[Inject]
		public Action action { get; set; }

		[Inject]
		public UIModel model { get; set; }

		public override void Execute()
		{
			if (obj.activeInHierarchy)
			{
				model.AddUI(obj.GetInstanceID(), action);
			}
		}
	}
}
