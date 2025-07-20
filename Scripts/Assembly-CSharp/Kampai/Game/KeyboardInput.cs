using System.Collections;
using Kampai.Common;
using Kampai.Game.Mignette;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game
{
	public class KeyboardInput : IInput
	{
		private bool previousState;

		private bool pressed;

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public IPickService pickService { get; set; }

		[Inject]
		public IMignetteService mignetteService { get; set; }

		[Inject]
		public DebugKeyHitSignal debugKeyHitSignal { get; set; }

		[PostConstruct]
		public void PostConstruct()
		{
			routineRunner.StartCoroutine(Update());
		}

		private IEnumerator Update()
		{
			while (true)
			{
				bool currentState = Input.GetMouseButton(0);
				int input = 0;
				if (currentState)
				{
					input |= 1;
				}
				if (Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) * Time.deltaTime > 0f)
				{
					input |= 2;
				}
				if (!previousState && currentState)
				{
					pressed = true;
				}
				else if (previousState && !currentState)
				{
					pressed = false;
				}
				pickService.OnGameInput(Input.mousePosition, input, pressed);
				mignetteService.OnGameInput(Input.mousePosition, input, pressed);
				previousState = currentState;
				yield return null;
			}
		}
	}
}
