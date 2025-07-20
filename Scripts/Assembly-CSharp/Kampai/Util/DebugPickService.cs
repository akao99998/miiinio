using Kampai.Main;
using Kampai.UI.View;
using UnityEngine;

namespace Kampai.Util
{
	public class DebugPickService
	{
		private RaycastHit hit;

		private Vector3 inputPosition;

		private GameObject startHitObject;

		private bool prevPress;

		[Inject(MainElement.CAMERA)]
		public Camera camera { get; set; }

		[Inject]
		public ShowDebugVisualizerSignal showDebugVisualizerSignal { get; set; }

		public void OnGameInput(Vector3 inputPosition, int input, bool pressed)
		{
			this.inputPosition = inputPosition;
			if (!prevPress && pressed)
			{
				TouchStart();
			}
			else if (prevPress && pressed)
			{
				TouchHold();
			}
			else if (prevPress && !pressed)
			{
				TouchEnd();
			}
			prevPress = pressed;
		}

		private void TouchStart()
		{
			Ray ray = camera.ScreenPointToRay(inputPosition);
			if (Physics.Raycast(ray, out hit))
			{
				startHitObject = hit.collider.gameObject;
				showDebugVisualizerSignal.Dispatch(startHitObject, -1, 0f);
			}
			if (startHitObject != null)
			{
				switch (startHitObject.layer)
				{
				case 8:
				case 9:
				case 11:
				case 12:
				case 14:
				case 15:
				case 17:
					break;
				case 10:
				case 13:
				case 16:
					break;
				}
			}
		}

		private void TouchHold()
		{
		}

		private void TouchEnd()
		{
			startHitObject = null;
		}
	}
}
