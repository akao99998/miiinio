using System.Collections;
using Kampai.Common;
using Kampai.Game.Mignette;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game
{
	public class TouchInput : IInput
	{
		private Vector3 position;

		private int touchCount;

		private bool pressed;

		private bool isDeviceSamsung;

		private bool wasStylusActive;

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public IPickService pickService { get; set; }

		[Inject]
		public IMignetteService mignetteService { get; set; }

		[Inject]
		public DeviceInformation deviceInformation { get; set; }

		[PostConstruct]
		public void PostConstruct()
		{
			isDeviceSamsung = deviceInformation.IsSamsung();
			routineRunner.StartCoroutine(Update());
		}

		private IEnumerator Update()
		{
			yield return null;
			while (true)
			{
				position = Vector3.zero;
				touchCount = InputUtils.touchCount;
				if (touchCount > 0)
				{
					Touch touch = InputUtils.GetTouch(0);
					position = touch.position;
					if (touch.phase == TouchPhase.Began)
					{
						pressed = true;
						ScreenUtils.ToggleAutoRotation(false);
					}
					else if (touch.phase == TouchPhase.Ended)
					{
						pressed = false;
						ScreenUtils.ToggleAutoRotation(true);
					}
				}
				else if (isDeviceSamsung)
				{
					bool isStylusActive = Input.GetMouseButton(0);
					if (isStylusActive || wasStylusActive)
					{
						position = Input.mousePosition;
						touchCount = 1;
						pressed = (wasStylusActive = isStylusActive);
					}
				}
				pressed = pressed && touchCount > 0;
				pickService.OnGameInput(position, touchCount, pressed);
				mignetteService.OnGameInput(position, touchCount, pressed);
				yield return null;
			}
		}
	}
}
