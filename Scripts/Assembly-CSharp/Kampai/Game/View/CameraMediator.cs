using UnityEngine;

namespace Kampai.Game.View
{
	public interface CameraMediator
	{
		void OnGameInput(Vector3 position, int input);

		void OnDisableBehaviour(int behaviour);

		void OnEnableBehaviour(int behaviour);
	}
}
