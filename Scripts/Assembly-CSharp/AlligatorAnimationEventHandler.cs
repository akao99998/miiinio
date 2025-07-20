using UnityEngine;

public class AlligatorAnimationEventHandler : MonoBehaviour
{
	public AlligatorAgent Agent;

	public void OnAlligatorBiteHam()
	{
		Agent.OnAlligatorBiteHam();
	}
}
