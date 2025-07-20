using UnityEngine;

public class EdwardMinionHandsTinyBushViewObject : MonoBehaviour
{
	public GameObject ParticleVFXPrefab;

	public Animation ShakeEffectAnimation;

	private void OnTriggerEnter(Collider other)
	{
		GameObject gameObject = Object.Instantiate(ParticleVFXPrefab);
		gameObject.transform.SetParent(base.transform, false);
		Vector3 position = base.transform.position;
		position.y += 1f;
		gameObject.transform.position = position;
		Object.Destroy(gameObject, 5f);
		ShakeEffectAnimation.Play();
	}
}
