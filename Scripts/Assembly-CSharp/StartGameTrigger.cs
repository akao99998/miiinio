using UnityEngine;

public class StartGameTrigger : MonoBehaviour
{
	public void OnTriggerEnter(Collider other)
	{
		AlligatorAgent component = other.GetComponent<AlligatorAgent>();
		if (component != null)
		{
			component.OnStartGame();
		}
	}
}
