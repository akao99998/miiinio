using UnityEngine;
using UnityEngine.UI;
using strange.extensions.command.impl;

namespace Kampai.Tools.AnimationToolKit
{
	public class LoadToggleGroupCommand : Command
	{
		[Inject]
		public Canvas Canvas { get; set; }

		public override void Execute()
		{
			GameObject gameObject = new GameObject("Toggle Group");
			gameObject.transform.parent = Canvas.transform;
			RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
			rectTransform.anchoredPosition = Vector2.zero;
			ToggleGroup toggleGroup = gameObject.AddComponent<ToggleGroup>();
			toggleGroup.transform.position = Vector3.zero;
			base.injectionBinder.Bind<GameObject>().ToValue(gameObject).ToName(AnimationToolKitElement.TOGGLE_GROUP);
		}
	}
}
