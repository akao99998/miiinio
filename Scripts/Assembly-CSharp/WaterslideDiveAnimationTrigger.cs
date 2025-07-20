using UnityEngine;

public class WaterslideDiveAnimationTrigger : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		PathAgent componentInParent = other.transform.GetComponentInParent<PathAgent>();
		if (componentInParent != null)
		{
			componentInParent.OnPlayDiveAnimation();
		}
	}
}
