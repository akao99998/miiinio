using UnityEngine;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;

public class EnvironmentAudioManagerView : View
{
	public Signal<Vector3> checkHit = new Signal<Vector3>();

	public Camera mainCamera { get; set; }

	private void Update()
	{
		if (!(mainCamera == null))
		{
			Plane plane = new Plane(Vector3.up, Vector3.zero);
			Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
			float enter;
			if (plane.Raycast(ray, out enter))
			{
				Vector3 point = ray.GetPoint(enter);
				base.transform.position = point;
				checkHit.Dispatch(point);
			}
		}
	}
}
