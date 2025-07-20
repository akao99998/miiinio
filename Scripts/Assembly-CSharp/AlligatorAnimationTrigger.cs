using UnityEngine;

public class AlligatorAnimationTrigger : MonoBehaviour
{
	public string AnimationStateName = string.Empty;

	public void OnTriggerEnter(Collider other)
	{
		AlligatorAgent component = other.GetComponent<AlligatorAgent>();
		if (component != null)
		{
			component.TrySetAnimationState(AnimationStateName);
		}
	}
}
