using UnityEngine;

public class ToggleFPS : MonoBehaviour
{
	public void Toggle()
	{
		FPSGraphC component = Camera.main.GetComponent<FPSGraphC>();
		if (component != null)
		{
			component.enabled = !component.enabled;
			return;
		}
		component = Camera.main.gameObject.AddComponent<FPSGraphC>();
		component.showFPSNumber = true;
		component.showPerformanceOnClick = false;
	}
}
