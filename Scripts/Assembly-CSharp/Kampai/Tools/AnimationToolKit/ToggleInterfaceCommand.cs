using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Tools.AnimationToolKit
{
	public class ToggleInterfaceCommand : Command
	{
		[Inject]
		public Canvas canvas { get; set; }

		public override void Execute()
		{
			int childCount = canvas.transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Transform child = canvas.transform.GetChild(i);
				GameObject gameObject = child.gameObject;
				AnimationToolKitButtonView component = child.GetComponent<AnimationToolKitButtonView>();
				if (component == null || component.ButtonType != AnimationToolKitButtonType.ToggleInterface)
				{
					child.gameObject.SetActive(!gameObject.activeSelf);
				}
			}
		}
	}
}
