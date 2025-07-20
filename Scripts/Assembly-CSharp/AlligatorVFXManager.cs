using UnityEngine;

public class AlligatorVFXManager : MonoBehaviour
{
	public GameObject AlligatorWakeVFX;

	public Transform AlligatorWakeHardpoint;

	public GameObject MinionWakeVFX;

	public Transform MinionWakeHardpoint;

	public GameObject ObstacleImpactVFX;

	private GameObject alligatorWakeInstance;

	private GameObject minionWakeInstance;

	private void Start()
	{
		alligatorWakeInstance = Object.Instantiate(AlligatorWakeVFX);
		alligatorWakeInstance.transform.parent = AlligatorWakeHardpoint;
		alligatorWakeInstance.transform.localPosition = Vector3.zero;
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

	public void PlayImpactVfx(Vector3 pos)
	{
		GameObject gameObject = Object.Instantiate(ObstacleImpactVFX);
		gameObject.transform.position = pos;
		gameObject.SetActive(true);
	}

	public void DisplayAlligatorWake(bool display)
	{
		alligatorWakeInstance.SetActive(display);
	}
}
