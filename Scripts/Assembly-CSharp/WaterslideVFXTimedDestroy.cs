using System.Collections;
using UnityEngine;

public class WaterslideVFXTimedDestroy : MonoBehaviour
{
	public float Duration;

	private void Start()
	{
		StartCoroutine(TimedDestroy());
	}

	private IEnumerator TimedDestroy()
	{
		yield return new WaitForSeconds(Duration);
		Object.Destroy(base.gameObject);
	}
}
