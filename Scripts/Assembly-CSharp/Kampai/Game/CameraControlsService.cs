using System;
using UnityEngine;

namespace Kampai.Game
{
	public class CameraControlsService : ICameraControlsService
	{
		private Action<Vector3, int> controlEvent;

		public void RegisterListener(Action<Vector3, int> obj)
		{
			controlEvent = (Action<Vector3, int>)Delegate.Combine(controlEvent, obj);
		}

		public void UnregisterListener(Action<Vector3, int> obj)
		{
			controlEvent = (Action<Vector3, int>)Delegate.Remove(controlEvent, obj);
		}

		public void OnGameInput(Vector3 position, int input)
		{
			if (controlEvent != null)
			{
				controlEvent(position, input);
			}
		}
	}
}
