using UnityEngine;

namespace Kampai.Util
{
	public class MinionBlobShadowView : MonoBehaviour
	{
		private Transform pelvis;

		private Transform myTransform;

		private void Start()
		{
			myTransform = base.transform;
		}

		public void SetToTrack(Transform pelvis)
		{
			this.pelvis = pelvis;
		}

		private void Update()
		{
			if ((bool)pelvis)
			{
				Vector3 position = pelvis.position;
				myTransform.position = new Vector3(position.x, 0f, position.z);
			}
		}

		public void ManualUpdate()
		{
			Update();
		}
	}
}
