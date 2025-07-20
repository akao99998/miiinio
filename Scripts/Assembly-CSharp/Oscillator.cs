using UnityEngine;

public class Oscillator : MonoBehaviour
{
	public bool XAxis;

	public bool YAxis;

	public bool ZAxis;

	public Vector3 frequency;

	public Vector3 magnitude;

	private Vector3 pos;

	private Vector3 axis;

	private float elapsedTime;

	private void Start()
	{
		pos = base.transform.localPosition;
		axis = new Vector3((!XAxis) ? 0f : 1f, (!YAxis) ? 0f : 1f, (!ZAxis) ? 0f : 1f);
	}

	private void FixedUpdate()
	{
		elapsedTime += Time.deltaTime;
		base.transform.localPosition = new Vector3(pos.x + axis.x * Mathf.Sin(elapsedTime * frequency.x) * magnitude.x, pos.y + axis.y * Mathf.Sin(elapsedTime * frequency.y) * magnitude.y, pos.z + axis.z * Mathf.Sin(elapsedTime * frequency.z) * magnitude.z);
	}
}
