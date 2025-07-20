using UnityEngine;

namespace Kampai.UI.View
{
	public class FaceCameraView : MonoBehaviour
	{
		private Camera faceCamera;

		private Vector3 yFlip = new Vector3(0f, 180f, 0f);

		public void Start()
		{
			faceCamera = Camera.main;
		}

		public void Update()
		{
			base.gameObject.transform.LookAt(faceCamera.transform, faceCamera.transform.up);
			base.gameObject.transform.Rotate(yFlip);
		}
	}
}
