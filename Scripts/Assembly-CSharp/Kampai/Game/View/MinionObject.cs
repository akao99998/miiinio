using System.Collections.Generic;
using Kampai.Util;
using Kampai.Util.AI;
using UnityEngine;

namespace Kampai.Game.View
{
	public class MinionObject : CharacterObject
	{
		public enum MinionGachaState
		{
			Inactive = 0,
			Active = 1,
			Deviant = 2,
			IndividualTap = 3
		}

		private const float importScale = 0.01f;

		protected Agent agent;

		protected float defaultMaxSpeed;

		protected bool isInParty;

		protected Vector3 partyLocation;

		protected float partyRadius;

		protected SteerToTether steerToTether;

		protected bool isInMinionParty;

		protected float partyRestMin;

		protected float partyRestMax;

		protected float partyRestTimer;

		private Minion minion;

		[Inject]
		public CameraUtils cameraUtils { get; set; }

		[Inject]
		public MinionIdleNotifier minionIdleNotifier { get; set; }

		[Inject]
		public MinionPartyAnimationSignal minionPartyAnimationSignal { get; set; }

		public bool isTemporaryMinion { get; set; }

		public MinionGachaState GachaState { get; set; }

		public int Level
		{
			get
			{
				return (minion != null) ? minion.Level : 0;
			}
		}

		public override void Init(Character character, IKampaiLogger logger)
		{
			base.Init(character, logger);
			isTemporaryMinion = false;
			minion = character as Minion;
			if (minion == null)
			{
				logger.Error("Init Minion {0}:{1} Failed!", character.Name, character.ID);
			}
			else
			{
				SetMinionShape();
				agent = base.gameObject.GetComponent<Agent>();
				defaultMaxSpeed = agent.MaxSpeed;
			}
		}

		public Minion GetMinion()
		{
			return minion;
		}

		public override void OnDefinitionsHotSwap(IDefinitionService definitionService)
		{
			base.OnDefinitionsHotSwap(definitionService);
			minion.OnDefinitionHotSwap(definitionService.Get(base.DefinitionID));
			SetMinionShape();
		}

		private void SetMinionShape()
		{
			if (!minion.HasPrestige)
			{
				SetEyes(base.transform, minion.Definition.Eyes);
				SetBody(base.transform, minion.Definition.Body);
				SetHair(base.transform, minion.Definition.Hair);
			}
		}

		public override void LateUpdate()
		{
			base.LateUpdate();
			if (!(animators[0].runtimeAnimatorController != null) || !animatorControllersAreEqual || shelvedQueue != null)
			{
				return;
			}
			if (actionQueue.Count == 0 && !(currentAction is GotoSideWalkAction))
			{
				SetAnimBool("isMoving", agent.MaxSpeed > 0.0001f);
				SetAnimFloat("speed", agent.Speed);
			}
			if (isTemporaryMinion)
			{
				return;
			}
			if (isInMinionParty)
			{
				if ((Vector3.Distance(base.gameObject.transform.position, partyLocation) <= partyRadius + 1f) ? true : false)
				{
					partyRestTimer -= Time.deltaTime;
					if (partyRestTimer <= 0f && actionQueue.Count == 0)
					{
						minionPartyAnimationSignal.Dispatch(ID);
						partyRestTimer = Random.Range(partyRestMin, partyRestMax);
						return;
					}
				}
				SetTether(partyLocation, partyRadius);
			}
			else if (isInParty)
			{
				SetTether(partyLocation, partyRadius);
			}
			else
			{
				SetTether(cameraUtils.CameraCenterRaycast(), 20f);
			}
		}

		public override void Idle()
		{
			if (minionIdleNotifier != null)
			{
				minionIdleNotifier.MinionIdle(this);
				base.currentAction = null;
			}
		}

		public void EnterParty(Vector3 partyLocation, float radius)
		{
			isInParty = true;
			this.partyLocation = partyLocation;
			partyRadius = radius;
		}

		public void LeaveParty()
		{
			isInParty = false;
		}

		public void EnterMinionParty(Vector3 partyLocation, float radius, float restMin, float restMax)
		{
			isInMinionParty = true;
			this.partyLocation = partyLocation;
			partyRadius = radius;
			partyRestMin = restMin;
			partyRestMax = restMax;
		}

		public void LeaveMinionParty()
		{
			isInMinionParty = false;
		}

		public void SetTether(Vector3 tether, float dist)
		{
			if (steerToTether == null)
			{
				steerToTether = GetComponent<SteerToTether>();
			}
			if (steerToTether != null)
			{
				steerToTether.Tether = tether;
				steerToTether.MaxDist = dist;
			}
		}

		public void Wander()
		{
			agent.MaxSpeed = defaultMaxSpeed;
			SetAnimController(defaultController);
		}

		public void SeekTarget(Vector3 pos, float threshold)
		{
			SteerCharacterToSeek component = GetComponent<SteerCharacterToSeek>();
			component.Target = pos;
			component.Threshold = threshold;
			component.enabled = true;
		}

		public Agent GetAgent()
		{
			return agent;
		}

		public override void ExecuteAction(KampaiAction action)
		{
			agent.MaxSpeed = 0f;
			base.ExecuteAction(action);
		}

		public static void SetEyes(Transform t, uint count)
		{
			if (count != 1)
			{
				Transform transform = t.gameObject.FindChild("minion:eyeMain_jnt").transform;
				GameObject gameObject = t.gameObject.FindChild("minion:browMorph_jnt");
				transform.localRotation = Quaternion.identity;
				if (gameObject != null)
				{
					gameObject.transform.localScale = new Vector3(1.65f, 0.4f, 1f);
				}
			}
		}

		public static Dictionary<string, TransformLite> GetMinionTransforms(Transform t)
		{
			Dictionary<string, TransformLite> dictionary = new Dictionary<string, TransformLite>();
			string[] array = new string[16]
			{
				"minion:eyeMain_jnt", "minion:browMorph_jnt", "minion:neckStretch_jnt", "minion:L_shoulderOffset_jnt", "minion:R_shoulderOffset_jnt", "minion:pelvisOffset_jnt", "minion:mouth_base_jnt", "minion:neckOffset_jnt", "minion:headOffset_jnt", "minion:spineOffset_jnt",
				"minion:mouthOffset_jnt", "minion:L_hipOffset_jnt", "minion:R_hipOffset_jnt", "minion:hatScale_jnt", "minion:hair1_jnt", "minion:hair2_jnt"
			};
			string[] array2 = array;
			foreach (string key in array2)
			{
				GameObject gameObject = t.gameObject.FindChild(key);
				if (gameObject != null)
				{
					Transform transform = gameObject.transform;
					dictionary.Add(key, new TransformLite(transform.localPosition, transform.localRotation, transform.localScale));
				}
			}
			return dictionary;
		}

		public static void SetTransforms(Transform t, Dictionary<string, TransformLite> xforms)
		{
			foreach (string key in xforms.Keys)
			{
				GameObject gameObject = t.gameObject.FindChild(key);
				if (gameObject != null)
				{
					Transform transform = gameObject.transform;
					transform.localPosition = xforms[key].position;
					transform.localRotation = xforms[key].rotation;
					transform.localScale = xforms[key].scale;
				}
			}
		}

		public static void SetBody(Transform t, MinionBody body)
		{
			if (body == MinionBody.NORMAL)
			{
				return;
			}
			Transform transform = t.gameObject.FindChild("minion:neckStretch_jnt").transform;
			Transform transform2 = t.gameObject.FindChild("minion:L_shoulderOffset_jnt").transform;
			Transform transform3 = t.gameObject.FindChild("minion:R_shoulderOffset_jnt").transform;
			Transform transform4 = t.gameObject.FindChild("minion:pelvisOffset_jnt").transform;
			Transform transform5 = t.gameObject.FindChild("minion:mouth_base_jnt").transform;
			Transform transform6 = t.gameObject.FindChild("minion:neckOffset_jnt").transform;
			Transform transform7 = t.gameObject.FindChild("minion:headOffset_jnt").transform;
			Transform transform8 = t.gameObject.FindChild("minion:spineOffset_jnt").transform;
			Transform transform9 = t.gameObject.FindChild("minion:mouthOffset_jnt").transform;
			Transform transform10 = t.gameObject.FindChild("minion:eyeMain_jnt").transform;
			GameObject gameObject = t.gameObject.FindChild("minion:browMorph_jnt");
			switch (body)
			{
			case MinionBody.TALL:
				transform4.localScale = new Vector3(0.825f, 1f, 0.9f);
				transform8.localScale = new Vector3(0.8f, 1f, 0.9f);
				transform5.localPosition = new Vector3(transform5.localPosition.x, 0.32999998f, transform5.localPosition.z);
				transform5.localScale = new Vector3(0.65f, 0.9f, 0.7f);
				transform.localPosition = new Vector3(transform.localPosition.x, 0.3875f, transform.localPosition.z);
				transform6.localPosition = new Vector3(transform6.localPosition.x, transform6.localPosition.y, 0.225f);
				transform6.localScale = new Vector3(0.8f, transform6.localScale.y, 0.85f);
				transform7.localPosition = new Vector3(transform7.localPosition.x, transform7.localPosition.y, 0.19999999f);
				transform7.localScale = new Vector3(0.82f, 1f, 0.8f);
				transform10.localScale = new Vector3(1.1f, 0.9f, 1.15f);
				transform10.localPosition = new Vector3(transform10.localPosition.x, transform10.localPosition.y, -0.147f);
				transform3.localPosition = new Vector3(0.205f, transform3.localPosition.y, transform3.localPosition.z);
				transform2.localPosition = new Vector3(-0.205f, transform2.localPosition.y, transform2.localPosition.z);
				transform9.localPosition = new Vector3(transform9.localPosition.x, 0.32999998f, transform9.localPosition.z);
				if (gameObject != null)
				{
					gameObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
				}
				break;
			case MinionBody.WIDE:
			{
				Transform transform11 = t.gameObject.FindChild("minion:L_hipOffset_jnt").transform;
				Transform transform12 = t.gameObject.FindChild("minion:R_hipOffset_jnt").transform;
				Transform transform13 = t.gameObject.FindChild("minion:hatScale_jnt").transform;
				transform4.localScale = new Vector3(1.3f, 1f, 1.33f);
				transform8.localScale = new Vector3(1.3f, 1.35f, 1.3f);
				transform.localPosition = new Vector3(transform.localPosition.x, 0.17181998f, transform.localPosition.z);
				transform6.localPosition = new Vector3(transform6.localPosition.x, transform6.localPosition.y, 0.25108f);
				transform6.localScale = new Vector3(1.3f, 1.35f, 1.3f);
				transform7.localPosition = new Vector3(transform7.localPosition.x, transform7.localPosition.y, 0.25108f);
				transform7.localScale = new Vector3(1.3f, 0.9f, 1.3f);
				transform10.localScale = new Vector3(20f / 27f, 1f, 20f / 27f);
				transform10.localPosition = new Vector3(transform10.localPosition.x, transform10.localPosition.y, -0.099999994f);
				transform3.localPosition = new Vector3(0.32f, transform3.localPosition.y, -0.03f);
				transform2.localPosition = new Vector3(-0.32f, transform2.localPosition.y, -0.03f);
				transform12.localPosition = new Vector3(transform12.localPosition.x, transform12.localPosition.y, -0.089999996f);
				transform11.localPosition = new Vector3(transform11.localPosition.x, transform11.localPosition.y, -0.089999996f);
				transform5.localScale = new Vector3(1.2f, 0.8f, 1f);
				transform5.localPosition = new Vector3(transform5.localPosition.x, 0.22711f, transform5.localPosition.z);
				transform13.localScale = new Vector3(1f, 1.444f, 1f);
				break;
			}
			case MinionBody.BOB:
				break;
			}
		}

		public static void SetHair(Transform t, MinionHair hair)
		{
			GameObject gameObject = t.gameObject.FindChild("minion:hair1_jnt");
			GameObject gameObject2 = t.gameObject.FindChild("minion:hair2_jnt");
			switch (hair)
			{
			case MinionHair.PARTED:
				if (gameObject != null)
				{
					Transform transform7 = gameObject.transform;
					transform7.localPosition = new Vector3(transform7.localPosition.x, 0.12364999f, transform7.localPosition.z);
					transform7.localEulerAngles = new Vector3(0f, 0f, 0f);
					transform7.localScale = new Vector3(0.5f, 0.5f, 0.5f);
				}
				if (gameObject2 != null)
				{
					Transform transform8 = gameObject2.transform;
					transform8.localPosition = new Vector3(transform8.localPosition.x, 0.12364999f, transform8.localPosition.z);
					transform8.localEulerAngles = new Vector3(0f, 0f, 0f);
					transform8.localScale = new Vector3(1f, 1f, 1f);
				}
				break;
			case MinionHair.BALD:
				if (gameObject != null)
				{
					Transform transform5 = gameObject.transform;
					transform5.localPosition = new Vector3(transform5.localPosition.x, 0.12364999f, transform5.localPosition.z);
					transform5.localEulerAngles = new Vector3(0f, 0f, 0f);
					transform5.localScale = new Vector3(0.5f, 0.5f, 0.5f);
				}
				if (gameObject2 != null)
				{
					Transform transform6 = gameObject2.transform;
					transform6.localPosition = new Vector3(transform6.localPosition.x, 0.12364999f, transform6.localPosition.z);
					transform6.localEulerAngles = new Vector3(0f, 0f, 0f);
					transform6.localScale = new Vector3(0.5f, 0.5f, 0.5f);
				}
				break;
			case MinionHair.SPROUT:
				if (gameObject != null)
				{
					Transform transform3 = gameObject.transform;
					transform3.localPosition = new Vector3(transform3.localPosition.x, 0.15365f, transform3.localPosition.z);
					transform3.localEulerAngles = new Vector3(0f, 180f, 180f);
					transform3.localScale = new Vector3(0.5f, 1f, 0.5f);
				}
				if (gameObject2 != null)
				{
					Transform transform4 = gameObject2.transform;
					transform4.localPosition = new Vector3(transform4.localPosition.x, 0.12364999f, transform4.localPosition.z);
					transform4.localEulerAngles = new Vector3(0f, 0f, 0f);
					transform4.localScale = new Vector3(0.5f, 0.5f, 0.5f);
				}
				break;
			case MinionHair.WILD:
				if (gameObject != null)
				{
					Transform transform9 = gameObject.transform;
					transform9.localPosition = new Vector3(transform9.localPosition.x, 0.15365f, transform9.localPosition.z);
					transform9.localEulerAngles = new Vector3(0f, 0f, 0f);
					transform9.localScale = new Vector3(1f, 1f, 1f);
				}
				if (gameObject2 != null)
				{
					Transform transform10 = gameObject2.transform;
					transform10.localPosition = new Vector3(transform10.localPosition.x, 0.12364999f, transform10.localPosition.z);
					transform10.localEulerAngles = new Vector3(0f, 0f, 0f);
					transform10.localScale = new Vector3(0.5f, 0.5f, 0.5f);
				}
				break;
			case MinionHair.SPIKE:
				if (gameObject != null)
				{
					Transform transform = gameObject.transform;
					transform.localPosition = new Vector3(transform.localPosition.x, 0.12364999f, transform.localPosition.z);
					transform.localEulerAngles = new Vector3(0f, 0f, 0f);
					transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
				}
				if (gameObject2 != null)
				{
					Transform transform2 = gameObject2.transform;
					transform2.localPosition = new Vector3(transform2.localPosition.x, 0.12364999f, transform2.localPosition.z);
					transform2.localEulerAngles = new Vector3(0f, 180f, 180f);
					transform2.localScale = new Vector3(1f, 1f, 1f);
				}
				break;
			}
		}

		public void DisableSelection()
		{
			foreach (Transform item in base.transform)
			{
				Renderer component = item.GetComponent<Renderer>();
				if (component != null && item.name.StartsWith("selectIcon"))
				{
					logger.Debug("Disable Selection");
					component.enabled = false;
				}
			}
		}
	}
}
