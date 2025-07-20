using UnityEngine;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	public class EnvironmentAudioEmitterView : strange.extensions.mediation.impl.View
	{
		public string AudioName;

		public float FadeDuration = 2f;

		private Camera mainCamera;

		private Transform m_transform;

		internal readonly Signal<bool> OnTargetVisible = new Signal<bool>();

		private bool volcanoWasVisible;

		internal void Init(Camera mainCamera)
		{
			this.mainCamera = mainCamera;
			m_transform = base.transform;
			OnTargetVisible.Dispatch(false);
		}

		private void Update()
		{
			if (!(mainCamera == null))
			{
				Vector3 vector = mainCamera.WorldToViewportPoint(m_transform.position);
				Vector3 vector2 = new Vector3(Mathf.Clamp01(vector.x), Mathf.Clamp01(vector.y), vector.z);
				bool flag = vector == vector2;
				if (flag != volcanoWasVisible)
				{
					OnTargetVisible.Dispatch(flag);
					volcanoWasVisible = flag;
				}
			}
		}
	}
}
