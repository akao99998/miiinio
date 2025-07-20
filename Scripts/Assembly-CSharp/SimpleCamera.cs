using UnityEngine;

public class SimpleCamera : MonoBehaviour
{
	private Plane groundPlane;

	private Vector3 currentPosition;

	private Vector3 hitPosition;

	private Vector3 velocity;

	public Vector2 MinBounds;

	public Vector2 MaxBounds;

	public float decayAmount = 0.925f;

	private void Start()
	{
		Application.targetFrameRate = 60;
		Vector3 inNormal = new Vector3(0f, 1f, 0f);
		Vector3 inPoint = new Vector3(0f, 0f, 0f);
		groundPlane = new Plane(inNormal, inPoint);
	}

	private void Update()
	{
		if (Application.isMobilePlatform && Input.touchCount > 0)
		{
			if (Input.GetTouch(0).phase == TouchPhase.Began)
			{
				hitPosition = GroundPlaneRaycast(Input.GetTouch(0).position);
				velocity = Vector3.zero;
			}
			else if (Input.GetTouch(0).phase == TouchPhase.Moved)
			{
				currentPosition = GroundPlaneRaycast(Input.GetTouch(0).position);
				velocity = hitPosition - currentPosition;
			}
		}
		else if (Application.isEditor)
		{
			if (Input.GetMouseButtonDown(0))
			{
				hitPosition = GroundPlaneRaycast(Input.mousePosition);
				velocity = Vector3.zero;
			}
			else if (Input.GetMouseButton(0))
			{
				currentPosition = GroundPlaneRaycast(Input.mousePosition);
				velocity = hitPosition - currentPosition;
			}
		}
		Transform transform = Camera.main.transform;
		transform.position = new Vector3(Mathf.Clamp(transform.position.x + velocity.x, MinBounds.x, MaxBounds.x), transform.position.y + velocity.y, Mathf.Clamp(transform.position.z + velocity.z, MinBounds.y, MaxBounds.y));
		velocity *= decayAmount;
	}

	private Vector3 GroundPlaneRaycast(Vector3 screenPosition)
	{
		Ray ray = Camera.main.ScreenPointToRay(screenPosition);
		float enter;
		groundPlane.Raycast(ray, out enter);
		return ray.GetPoint(enter);
	}
}
