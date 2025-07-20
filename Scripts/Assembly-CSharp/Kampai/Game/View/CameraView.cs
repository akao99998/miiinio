using UnityEngine;

namespace Kampai.Game.View
{
	public interface CameraView
	{
		Vector3 Velocity { get; set; }

		float DecayAmount { get; set; }

		void PerformBehaviour(CameraUtils cameraUtils);

		void CalculateBehaviour(Vector3 position);

		void ResetBehaviour();

		void Decay();
	}
}
