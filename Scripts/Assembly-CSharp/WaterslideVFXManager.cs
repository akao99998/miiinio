using UnityEngine;

public class WaterslideVFXManager : MonoBehaviour
{
	public GameObject MinionWakeVFX;

	public Transform MinionWakeHardpoint;

	public GameObject ObstacleImpactVFX;

	private GameObject minionWakeInstance;

	private void Start()
	{
		minionWakeInstance = Object.Instantiate(MinionWakeVFX);
		minionWakeInstance.transform.parent = MinionWakeHardpoint;
		minionWakeInstance.transform.localPosition = Vector3.zero;
	}

	public void DisplayMinionWake(bool display)
	{
		ParticleSystem[] componentsInChildren = minionWakeInstance.GetComponentsInChildren<ParticleSystem>();
		ParticleSystem[] array = componentsInChildren;
		foreach (ParticleSystem ps in array)
		{
			ps.SetEmissionEnabled(display);
		}
	}
}
