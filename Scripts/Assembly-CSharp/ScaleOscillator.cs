using UnityEngine;

public class ScaleOscillator : MonoBehaviour
{
	public bool XAxis;

	public bool YAxis;

	public bool ZAxis;

	public Vector3 frequency;

	public Vector3 magnitude;

	private Vector3 scale;

	private Vector3 axis;

	private float elapsedTime;

	private void Start()
	{
		scale = base.transform.localScale;
		axis = new Vector3((!XAxis) ? 0f : 1f, (!YAxis) ? 0f : 1f, (!ZAxis) ? 0f : 1f);
	}

	private void FixedUpdate()
	{
		elapsedTime += Time.deltaTime;
		base.transform.localScale = new Vector3(scale.x + axis.x * Mathf.Sin(elapsedTime * frequency.x) * magnitude.x, scale.y + axis.y * Mathf.Sin(elapsedTime * frequency.y) * magnitude.y, scale.z + axis.z * Mathf.Sin(elapsedTime * frequency.z) * magnitude.z);
	}
}
