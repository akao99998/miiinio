using UnityEngine;

public class MignetteShadowViewObject : MonoBehaviour
{
	private Quaternion initialQuat = Quaternion.identity;

	private void Start()
	{
		initialQuat = base.transform.rotation;
	}

	private void LateUpdate()
	{
		Vector3 position = base.transform.position;
		position.y = 0.01f;
		base.transform.position = position;
		base.transform.rotation = initialQuat;
	}
}
