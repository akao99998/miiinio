using Kampai.Game.Mignette.WaterSlide.View;
using UnityEngine;

public class WaterslideCameraTrigger : MonoBehaviour
{
	public Transform CameraTransform;

	public float FieldOfView = -1f;

	public float Delay;

	public float TransitionDuration = 1f;

	public GoEaseType EaseType = GoEaseType.QuadInOut;

	private PathAgent pathAgent;

	private WaterSlideMignetteManagerView parentView;

	public void Start()
	{
		parentView = Object.FindObjectOfType<WaterSlideMignetteManagerView>();
	}

	private void OnTriggerEnter(Collider other)
	{
		PathAgent pathAgent = null;
		Transform parent = other.transform;
		while (parent.parent != null && pathAgent == null)
		{
			parent = parent.parent;
			pathAgent = parent.gameObject.GetComponent<PathAgent>();
		}
		if (pathAgent != null)
		{
			this.pathAgent = pathAgent;
			Invoke("MoveCamera", Delay);
		}
	}

	public void MoveCamera()
	{
		if (!parentView.isGameOver && pathAgent != null)
		{
			pathAgent.OnCameraOverride(CameraTransform, FieldOfView, TransitionDuration, EaseType);
			pathAgent = null;
		}
	}
}
