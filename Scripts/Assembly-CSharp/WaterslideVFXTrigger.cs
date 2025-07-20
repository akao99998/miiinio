using Kampai.Game.Mignette.WaterSlide.View;
using UnityEngine;

public class WaterslideVFXTrigger : MonoBehaviour
{
	public GameObject VFXGameObject;

	public Transform VFXParentTransform;

	public bool PlaySplashAudio;

	private WaterSlideMignetteManagerView parentView;

	public void Start()
	{
		parentView = Object.FindObjectOfType<WaterSlideMignetteManagerView>();
		DisableAudio();
	}

	private void OnTriggerEnter(Collider other)
	{
		PathAgent componentInParent = other.transform.GetComponentInParent<PathAgent>();
		if (componentInParent != null && VFXGameObject != null)
		{
			Transform transform = Object.Instantiate(VFXGameObject).transform;
			transform.SetParent(VFXParentTransform, false);
			if (PlaySplashAudio)
			{
				parentView.EnableWaterAudio(true);
				Invoke("DisableAudio", 1f);
			}
		}
	}

	private void DisableAudio()
	{
		parentView.EnableWaterAudio(false);
	}
}
