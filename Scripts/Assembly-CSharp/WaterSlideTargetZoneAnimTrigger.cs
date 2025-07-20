using UnityEngine;

public class WaterSlideTargetZoneAnimTrigger : MonoBehaviour
{
	public Animator landingZoneAnim;

	public Animator unicornAnim;

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other != null)
		{
			landingZoneAnim.Play("splash");
			unicornAnim.Play("splash");
		}
	}
}
