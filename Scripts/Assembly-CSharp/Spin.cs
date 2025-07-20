using UnityEngine;

public class Spin : MonoBehaviour
{
	public Vector3 amount;

	private void Update()
	{
		base.transform.Rotate(amount);
	}
}
