using UnityEngine;

public class TransformLite
{
	public Vector3 position;

	public Vector3 scale;

	public Quaternion rotation;

	public TransformLite(Vector3 position, Quaternion rotation, Vector3 scale)
	{
		this.position = position;
		this.rotation = rotation;
		this.scale = scale;
	}
}
