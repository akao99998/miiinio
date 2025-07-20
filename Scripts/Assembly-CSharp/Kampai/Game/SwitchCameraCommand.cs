using Kampai.Common;
using Kampai.Main;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	internal sealed class SwitchCameraCommand : Command
	{
		[Inject]
		public Camera camera { get; set; }

		[Inject(MainElement.CAMERA)]
		public Camera mainCamera { get; set; }

		[Inject]
		public PickControllerModel pickControllerModel { get; set; }

		[Inject(MainElement.UI_WORLDCANVAS)]
		public GameObject worldCanvas { get; set; }

		public override void Execute()
		{
			if (camera == mainCamera)
			{
				EnableUserInputToEnvironment();
			}
			else
			{
				DisableUserInputToEnvironment();
			}
		}

		private void DisableUserInputToEnvironment()
		{
			ToggleUserInputToEnvironment(false);
			Canvas component = worldCanvas.GetComponent<Canvas>();
			component.worldCamera = camera;
		}

		private void EnableUserInputToEnvironment()
		{
			ToggleUserInputToEnvironment(true);
			Canvas component = worldCanvas.GetComponent<Canvas>();
			component.worldCamera = mainCamera;
		}

		private void ToggleUserInputToEnvironment(bool enable)
		{
			pickControllerModel.ForceDisabled = !enable;
		}
	}
}
