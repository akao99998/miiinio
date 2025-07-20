using Kampai.Game.View;
using UnityEngine;

namespace Kampai.Game.Mignette.BalloonBarrage.View
{
	public class BalloonBarrageTargetAnimatorViewObject : MonoBehaviour
	{
		public enum MinionAndBalloonStates
		{
			None = 0,
			Floating = 1,
			Falling = 2,
			WalkingOff = 3
		}

		private const string WALK_OFF_ANIM_STATE = "Base Layer.WalkOff";

		private const string IDLE_ANIM_STATE = "Base Layer.Idle";

		private const string WAVE_ANIM_TRIGGER_NAME = "OnWave";

		private const string FLYING_ANIM_TRIGGER_NAME = "OnFlying";

		private const string HIT_ANIM_TRIGGER_NAME = "OnMinionHit";

		private const string FALLING_ANIM_TRIGGER_NAME = "OnFalling";

		private const string BOUNCE_ANIM_TRIGGER_NAME = "OnBounce";

		private const float WAVE_TIMER_MIN = 3f;

		private const float WAVE_TIMER_MAX = 10f;

		public MinionObject MinionToAnimate;

		public Animator BalloonAnimator;

		public SkinnedMeshRenderer BasketRenderer;

		public int Score = 1;

		public GameObject[] Mangoes;

		public GameObject BasketTarget;

		public Renderer BasketTargetRenderer;

		public Material[] TargetMaterials;

		public float WalkoffAnimDelay = 3f;

		public float WalkoffZValue = 4f;

		public GameObject BlobShadow;

		private bool isAFlyer;

		private MinionAndBalloonStates MinionAndBalloonState;

		private float zMinValue;

		private float zMaxValue;

		private bool directionLeft;

		private float targetSpeed = 10f;

		private float currentSpeed = 10f;

		private float initialY;

		private float velocityY;

		private bool needsReset;

		private GameObject mangoSplatGameObject;

		private float splatTimer;

		private float waveTimer;

		public BalloonBarrageBuildingViewObject MyParentBuildingViewObject;

		public BalloonBarrageGameController MyParentGameControllerObject;

		public bool HoldPosition = true;

		private void Start()
		{
			initialY = base.transform.position.y;
			velocityY = 3f;
			waveTimer = Random.Range(3f, 10f);
		}

		public void SyncAnimationStates(string stateName)
		{
			BalloonAnimator.Play(Animator.StringToHash(stateName));
			MinionToAnimate.PlayAnimation(Animator.StringToHash(stateName), 0, 0f);
		}

		public void SetAnimTriggers(string newTrigger)
		{
			MinionToAnimate.SetAnimTrigger(newTrigger);
			BalloonAnimator.SetTrigger(newTrigger);
		}

		public bool IsInState(string stateName)
		{
			return MinionToAnimate.IsInAnimatorState(Animator.StringToHash(stateName));
		}

		public void AddTarget(int materialIndex)
		{
			BasketTarget.SetActive(true);
			BasketTargetRenderer.material = TargetMaterials[materialIndex];
		}

		public void ShowModel(bool show)
		{
			MinionToAnimate.EnableRenderers(show);
			Renderer[] componentsInChildren = BalloonAnimator.gameObject.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				renderer.enabled = show;
			}
			MinionToAnimate.EnableBlobShadow(false);
			BlobShadow.SetActive(show);
		}

		public void ShowMango(bool show)
		{
			if (show)
			{
				GameObject[] mangoes = Mangoes;
				foreach (GameObject gameObject in mangoes)
				{
					if (!gameObject.activeSelf)
					{
						gameObject.SetActive(true);
						BasketTarget.SetActive(false);
						break;
					}
				}
			}
			else
			{
				GameObject[] mangoes2 = Mangoes;
				foreach (GameObject gameObject2 in mangoes2)
				{
					gameObject2.SetActive(false);
				}
				BasketTarget.SetActive(true);
			}
		}

		public void Update()
		{
			if (MinionToAnimate != null)
			{
				MinionToAnimate.transform.position = base.transform.position;
				MinionToAnimate.transform.rotation = base.transform.rotation;
			}
			if (isAFlyer)
			{
				UpdateFlyingMinion();
			}
		}

		public bool IsAFlyer()
		{
			return isAFlyer;
		}

		private void UpdateFlyingMinion()
		{
			if (needsReset)
			{
				needsReset = false;
				Vector3 position = base.transform.position;
				position.y = initialY;
				base.transform.position = position;
				base.transform.forward = MyParentGameControllerObject.FloatingMinionLocator.forward;
				splatTimer = 0f;
			}
			if (splatTimer >= 0f)
			{
				splatTimer -= Time.deltaTime;
				if (splatTimer <= 1f && mangoSplatGameObject != null)
				{
					mangoSplatGameObject.transform.localScale = Vector3.one * splatTimer;
				}
			}
			else if (mangoSplatGameObject != null && mangoSplatGameObject.activeSelf)
			{
				mangoSplatGameObject.SetActive(false);
			}
			waveTimer -= Time.deltaTime;
			if (waveTimer <= 0f)
			{
				SetAnimTriggers("OnWave");
				waveTimer = Random.Range(3f, 10f);
			}
			switch (MinionAndBalloonState)
			{
			case MinionAndBalloonStates.Floating:
				UpdateForFlying();
				break;
			case MinionAndBalloonStates.Falling:
			{
				Vector3 position2 = base.transform.position;
				position2.y += Time.deltaTime * velocityY;
				velocityY -= Time.deltaTime * 16f;
				if (position2.y <= 0.1f)
				{
					PlayBounceAnimation();
				}
				base.transform.position = position2;
				break;
			}
			case MinionAndBalloonStates.WalkingOff:
				UpdateForWalkingOff();
				break;
			}
		}

		public void UpdateForWalkingOff()
		{
			if (IsInState("Base Layer.WalkOff"))
			{
				Vector3 position = base.transform.position;
				position.z += Time.deltaTime * 2f;
				base.transform.position = position;
				if (position.z >= zMaxValue + WalkoffZValue)
				{
					directionLeft = false;
					PlayFlyingAnimation();
				}
				base.transform.position = base.transform.position;
				base.transform.rotation = Quaternion.Slerp(base.transform.rotation, Quaternion.LookRotation(Vector3.forward), Time.deltaTime * 2f);
			}
		}

		public void UpdateForFlying()
		{
			if (HoldPosition)
			{
				return;
			}
			Vector3 position = base.transform.position;
			if (directionLeft)
			{
				currentSpeed = Mathf.MoveTowards(currentSpeed, 0f - targetSpeed, Time.deltaTime * 5f);
				position.z += Time.deltaTime * currentSpeed;
				if (position.z <= zMinValue)
				{
					directionLeft = false;
				}
			}
			else
			{
				currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, Time.deltaTime * 5f);
				position.z += Time.deltaTime * currentSpeed;
				if (position.z >= zMaxValue)
				{
					directionLeft = true;
				}
			}
			base.transform.position = position;
		}

		public bool CanBeHit()
		{
			if (MinionAndBalloonState == MinionAndBalloonStates.Floating && !HoldPosition)
			{
				return true;
			}
			return false;
		}

		public void CleanUp()
		{
			Object.Destroy(mangoSplatGameObject);
			MinionAndBalloonState = MinionAndBalloonStates.None;
		}

		private void StartSplat()
		{
			CreateVfxAtLocation(MyParentBuildingViewObject.MangoHitBodyVFXPrefab, base.transform.position);
			splatTimer = 3f;
			mangoSplatGameObject.SetActive(true);
			mangoSplatGameObject.transform.localScale = Vector3.one;
		}

		public void MinionWasHit()
		{
			StartSplat();
			SetAnimTriggers("OnMinionHit");
		}

		public void PlayBounceAnimation()
		{
			MinionAndBalloonState = MinionAndBalloonStates.WalkingOff;
			SetAnimTriggers("OnBounce");
			CreateVfxAtLocation(MyParentBuildingViewObject.MinionHitGroundVFXPrefab, base.transform.position);
		}

		private void CreateVfxAtLocation(GameObject vfxPrefab, Vector3 pos)
		{
			GameObject gameObject = Object.Instantiate(vfxPrefab);
			gameObject.transform.SetParent(base.transform, false);
			gameObject.transform.position = pos;
			Object.Destroy(gameObject, 5f);
		}

		public void PlayFallAnimation(Vector3 impactPoint)
		{
			velocityY = 3f;
			CreateVfxAtLocation(MyParentBuildingViewObject.BalloonPopVFXPrefab, impactPoint);
			MinionAndBalloonState = MinionAndBalloonStates.Falling;
			SetAnimTriggers("OnFalling");
		}

		public void PlayFlyingAnimation()
		{
			needsReset = true;
			MinionAndBalloonState = MinionAndBalloonStates.Floating;
			SetAnimTriggers("OnFlying");
		}

		public void StartFloating(float zMin, float zMax, bool lookLeft, float speed)
		{
			isAFlyer = true;
			SyncAnimationStates("Base Layer.Idle");
			zMinValue = zMin;
			zMaxValue = zMax;
			directionLeft = lookLeft;
			targetSpeed = speed;
			currentSpeed = targetSpeed;
			if (directionLeft)
			{
				currentSpeed *= -1f;
			}
			Vector3 position = base.transform.position;
			if (directionLeft)
			{
				position.z = zMax + 4f;
			}
			else
			{
				position.z = zMin - 4f;
			}
			base.transform.position = position;
			PlayFlyingAnimation();
			GameObject gameObject = MinionToAnimate.gameObject.FindChild("minion:head_jnt");
			mangoSplatGameObject = Object.Instantiate(MyParentBuildingViewObject.MinionFaceSplatVFXPrefab);
			mangoSplatGameObject.transform.SetParent(gameObject.transform, false);
		}
	}
}
