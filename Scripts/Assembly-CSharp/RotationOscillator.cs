using UnityEngine;

public class RotationOscillator : MonoBehaviour
{
	public bool XAxis;

	public bool YAxis;

	public bool ZAxis;

	public Vector3 frequency;

	public Vector3 magnitude;

	private Quaternion rotation;

	private Vector3 axis;

	private float elapsedTime;

	private void Start()
	{
		rotation = base.transform.localRotation;
		axis = new Vector3((!XAxis) ? 0f : 1f, (!YAxis) ? 0f : 1f, (!ZAxis) ? 0f : 1f);
	}

	private void FixedUpdate()
	{
		elapsedTime += Time.deltaTime;
		base.transform.localRotation = new Quaternion(rotation.x + axis.x * Mathf.Sin(elapsedTime * frequency.x) * magnitude.x, rotation.y + axis.y * Mathf.Sin(elapsedTime * frequency.y) * magnitude.y, rotation.z + axis.z * Mathf.Sin(elapsedTime * frequency.z) * magnitude.z, 1f);
	}
}
