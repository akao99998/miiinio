using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class ToggleFPSGraphCommand : Command
	{
		[Inject(UIElement.CAMERA)]
		public Camera camera { get; set; }

		public override void Execute()
		{
			FPSGraphC component = camera.GetComponent<FPSGraphC>();
			if (component != null)
			{
				component.enabled = !component.enabled;
				return;
			}
			component = camera.gameObject.AddComponent<FPSGraphC>();
			component.showFPSNumber = true;
			component.showPerformanceOnClick = false;
		}
	}
}
