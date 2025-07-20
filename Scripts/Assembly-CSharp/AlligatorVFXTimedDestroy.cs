using System.Collections;
using UnityEngine;

public class AlligatorVFXTimedDestroy : MonoBehaviour
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
