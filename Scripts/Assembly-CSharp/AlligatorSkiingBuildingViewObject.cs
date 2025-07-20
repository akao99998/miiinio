using System;
using Kampai.Game.Mignette.View;
using Kampai.Main;
using Kampai.Util.Audio;
using UnityEngine;

public class AlligatorSkiingBuildingViewObject : MignetteBuildingViewObject
{
	public Transform IntroCamera;

	public GameObject[] ObjectsOnDuringCooldown;

	public GameObject[] ObjectsRandomOnDuringCooldown;

	public Vector2 minMaxRandomObjs = new Vector2(4f, 7f);

	public GameObject[] ObjectsOffDuringCooldown;

	public GameObject NeonFishGameObject;

	public float FishClickStartDelay = 0.1f;

	public float FishClickTime = 1f;

	private float fishClickTimer;

	private float fishClickDelayTimer;

	private bool isInCooldown;

	private PlayLocalAudioSignal myLocalAudioSignal;

	public void Start()
	{
		base.gameObject.AddComponent<MignetteBuildingCooldownView>();
	}

	public override void ResetCooldownView(PlayLocalAudioSignal localAudioSignal)
	{
		myLocalAudioSignal = localAudioSignal;
		SetCooldownObjects(localAudioSignal, false);
	}

	private void SetCooldownObjects(PlayLocalAudioSignal localAudioSignal, bool inCooldown)
	{
		isInCooldown = inCooldown;
		GameObject[] objectsOnDuringCooldown = ObjectsOnDuringCooldown;
		foreach (GameObject gameObject in objectsOnDuringCooldown)
		{
			gameObject.SetActive(inCooldown);
		}
		if (ObjectsRandomOnDuringCooldown.Length > 0)
		{
			int num = UnityEngine.Random.Range((int)minMaxRandomObjs.x, (int)minMaxRandomObjs.y);
			int num2 = UnityEngine.Random.Range(0, ObjectsRandomOnDuringCooldown.Length);
			for (int j = num2; j < num; j++)
			{
				if (CheckAvailable() && j == num - 1)
				{
					RandomAnimPlayer component = ObjectsRandomOnDuringCooldown[num2].GetComponent<RandomAnimPlayer>();
					if (component != null)
					{
						component.followObj = FindRandomMinionHead();
						component.following = true;
					}
				}
				int num3 = UnityEngine.Random.Range(0, ObjectsRandomOnDuringCooldown.Length);
				if (j == num3)
				{
					num3 = UnityEngine.Random.Range(0, ObjectsRandomOnDuringCooldown.Length);
				}
				ObjectsRandomOnDuringCooldown[num3].SetActive(inCooldown);
			}
		}
		GameObject[] objectsOffDuringCooldown = ObjectsOffDuringCooldown;
		foreach (GameObject gameObject2 in objectsOffDuringCooldown)
		{
			gameObject2.SetActive(!inCooldown);
		}
		if (!(NeonFishGameObject != null))
		{
			return;
		}
		if (!inCooldown)
		{
			if (localAudioSignal != null)
			{
				localAudioSignal.Dispatch(GetAudioEmitter.Get(NeonFishGameObject, "NeonSignBuzz"), "Play_fish_neonBuzz_01", null);
			}
			return;
		}
		CustomFMOD_StudioEventEmitter component2 = NeonFishGameObject.GetComponent<CustomFMOD_StudioEventEmitter>();
		if (component2 != null)
		{
			component2.Stop();
		}
	}

	public override void UpdateCooldownView(PlayLocalAudioSignal localAudioSignal, int buildingData, float pctDone)
	{
		if (pctDone < 1f)
		{
			SetCooldownObjects(localAudioSignal, true);
		}
		else
		{
			SetCooldownObjects(localAudioSignal, false);
		}
	}

	public virtual void Update()
	{
		if (!(NeonFishGameObject != null) || isInCooldown)
		{
			return;
		}
		if (fishClickDelayTimer < FishClickStartDelay)
		{
			fishClickDelayTimer += Time.deltaTime;
			return;
		}
		fishClickTimer += Time.deltaTime;
		if (fishClickTimer >= FishClickTime)
		{
			fishClickTimer = 0f;
			if (myLocalAudioSignal != null)
			{
				myLocalAudioSignal.Dispatch(GetAudioEmitter.Get(NeonFishGameObject, "NeonSignCrackle"), "Play_fish_neonCrackle_01", null);
			}
		}
	}

	private bool CheckAvailable()
	{
		bool result = false;
		DateTime today = DateTime.Today;
		if (today.Day == 1 && today.Month == 4)
		{
			result = true;
		}
		return result;
	}

	private GameObject FindRandomMinionHead()
	{
		GameObject result = null;
		GameObject gameObject = GameObject.Find("Minions");
		if (gameObject != null)
		{
			Animator[] componentsInChildren = gameObject.GetComponentsInChildren<Animator>();
			if (componentsInChildren.Length > 0)
			{
				int num = UnityEngine.Random.Range(0, componentsInChildren.Length - 1);
				string text = componentsInChildren[num].gameObject.name;
				text += "/minion:ROOT/minion:pelvis_jnt/minion:spine_jnt/minion:neckStretch_jnt/minion:neck_jnt/minion:head_jnt";
				result = GameObject.Find(text);
			}
		}
		return result;
	}
}
