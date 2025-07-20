using System;
using UnityEngine;

namespace Kampai.Game
{
	public interface ICameraControlsService
	{
		void RegisterListener(Action<Vector3, int> obj);

		void UnregisterListener(Action<Vector3, int> obj);

		void OnGameInput(Vector3 position, int input);
	}
}
