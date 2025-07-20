using UnityEngine;

public class WaterslideDiveTrigger : MonoBehaviour
{
	public bool StartRoll = true;

	private void OnTriggerEnter(Collider other)
	{
		PathAgent componentInParent = other.transform.GetComponentInParent<PathAgent>();
		if (componentInParent != null)
		{
			componentInParent.OnDiveTrigger(StartRoll);
		}
	}
}
