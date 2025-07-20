using Kampai.Game.View;
using UnityEngine;

namespace Kampai.Game.Mignette.EdwardMinionHands.View
{
	public class EdwardMinionHandsCuttingToolViewObject : MonoBehaviour
	{
		private const string IDLE_ANIM_STATE_NAME = "Base Layer.Idle";

		private const string IDLE_POLE_ANIM_STATE_NAME = "Base Layer.IdleWithPole";

		private const string CHAT_ANIM_LEFT_TRIGGER_NAME = "OnChatLeft01";

		private const string CHAT_ANIM_RIGHT_TRIGGER_NAME = "OnChatRight01";

		private const string RUN_ANIM_STATE_NAME = "Base Layer.Run";

		private const string RUN_POLE_ANIM_STATE_NAME = "Base Layer.RunningWithTool";

		private const string RUN_ANIM_BOOL_NAME = "IsRunning";

		private const string CHEER_ANIM_TRIGGER_NAME = "OnCheer";

		private const string CUT_ANIM_BOOL_NAME = "IsCutting";

		public Animator ToolAnimator;

		public GameObject[] PoleList;

		private ParticleSystem[] poleParticles;

		public MinionObject myMinionToUpdate;

		public EdwardMinionHandsMignetteManagerView myParentView;

		private Vector3 myInitialPos = Vector3.zero;

		private Quaternion myInitialRot = Quaternion.identity;

		private float minionSpeed = 1f;

		private Renderer[] renderers;

		private EdwardMinionHandsCollectableViewObject currentTargetCollectable;

		private bool poleIsShown;

		private void Start()
		{
			renderers = GetComponentsInChildren<Renderer>(true);
			int num = Random.Range(0, PoleList.Length);
			for (int i = 0; i < PoleList.Length; i++)
			{
				if (i == num)
				{
					PoleList[i].SetActive(true);
					poleParticles = PoleList[i].GetComponentsInChildren<ParticleSystem>();
				}
				else
				{
					PoleList[i].SetActive(false);
				}
			}
			ShowPole(false);
		}

		public void Setup(MinionObject minionToUpdate, float minionRunSpeed, EdwardMinionHandsMignetteManagerView parentView)
		{
			myMinionToUpdate = minionToUpdate;
			base.transform.position = myMinionToUpdate.transform.position;
			base.transform.rotation = myMinionToUpdate.transform.rotation;
			myInitialPos = myMinionToUpdate.transform.position;
			myInitialRot = myMinionToUpdate.transform.rotation;
			myParentView = parentView;
			minionSpeed = minionRunSpeed;
		}

		public void GoPickupDoober(EdwardMinionHandsCollectableViewObject collectableViewObject)
		{
			StopCutting();
			myMinionToUpdate.SetAnimBool("IsRunning", true);
			ToolAnimator.SetBool("IsRunning", true);
			myMinionToUpdate.PlayAnimation(Animator.StringToHash("Base Layer.Run"), 0, 0f);
			ToolAnimator.Play(Animator.StringToHash("Base Layer.RunningWithTool"), 0);
			currentTargetCollectable = collectableViewObject;
			EmitParticles(false);
		}

		private void EmitParticles(bool emit)
		{
			ParticleSystem[] array = poleParticles;
			foreach (ParticleSystem ps in array)
			{
				ps.SetEmissionEnabled(emit);
			}
		}

		public void Cheer()
		{
			EmitParticles(false);
			myMinionToUpdate.SetAnimTrigger("OnCheer");
			ToolAnimator.SetTrigger("OnCheer");
		}

		public void StartCutting()
		{
			ShowPole(true);
			myMinionToUpdate.SetAnimBool("IsCutting", true);
			myMinionToUpdate.SetAnimBool("IsRunning", false);
			ToolAnimator.SetBool("IsCutting", true);
			ToolAnimator.SetBool("IsRunning", false);
		}

		public void StopCutting()
		{
			myMinionToUpdate.SetAnimBool("IsCutting", false);
			ToolAnimator.SetBool("IsCutting", false);
		}

		public bool IsCollecting()
		{
			return currentTargetCollectable != null;
		}

		public bool IsMinionIdle()
		{
			return myMinionToUpdate.IsInAnimatorState(Animator.StringToHash("Base Layer.Idle"));
		}

		public bool IsMinionIdleWithPole()
		{
			return myMinionToUpdate.IsInAnimatorState(Animator.StringToHash("Base Layer.IdleWithPole"));
		}

		public void StartMinionChat(bool lookLeft)
		{
			if (lookLeft)
			{
				myMinionToUpdate.SetAnimTrigger("OnChatLeft01");
			}
			else
			{
				myMinionToUpdate.SetAnimTrigger("OnChatRight01");
			}
		}

		public void ClearCollectable()
		{
			currentTargetCollectable = null;
		}

		public void ShowPole(bool show)
		{
			Renderer[] array = renderers;
			foreach (Renderer renderer in array)
			{
				renderer.enabled = show;
			}
			if (show)
			{
				if (poleIsShown != show)
				{
					base.transform.localScale = Vector3.one * 0.0001f;
					EmitParticles(false);
				}
				else
				{
					EmitParticles(true);
				}
			}
			else
			{
				EmitParticles(false);
			}
			poleIsShown = show;
		}

		private void Update()
		{
			if (myParentView.IsPaused)
			{
				return;
			}
			if (myParentView.TimeElapsed >= myParentView.TotalEventTime)
			{
				myMinionToUpdate.SetAnimBool("IsRunning", false);
				myMinionToUpdate.SetAnimBool("IsCutting", false);
				ToolAnimator.SetBool("IsRunning", false);
				ToolAnimator.SetBool("IsCutting", false);
				return;
			}
			UpdateLocation();
			if (myMinionToUpdate != null)
			{
				myMinionToUpdate.transform.position = base.transform.position;
				myMinionToUpdate.transform.rotation = base.transform.rotation;
			}
			if (base.transform.localScale != Vector3.one)
			{
				base.transform.localScale = Vector3.MoveTowards(base.transform.localScale, Vector3.one, Time.deltaTime * 2f);
				if (base.transform.localScale == Vector3.one)
				{
					EmitParticles(true);
				}
			}
		}

		private void UpdateLocation()
		{
			if (!(myMinionToUpdate != null) || !myMinionToUpdate.IsInAnimatorState(Animator.StringToHash("Base Layer.Run")))
			{
				return;
			}
			if (currentTargetCollectable != null)
			{
				Vector3 vector = currentTargetCollectable.transform.position - base.transform.position;
				base.transform.forward = vector.normalized;
				base.transform.position = Vector3.MoveTowards(base.transform.position, currentTargetCollectable.transform.position, Time.deltaTime * minionSpeed);
				if (Vector3.Distance(base.transform.position, currentTargetCollectable.transform.position) < 0.1f)
				{
					myParentView.CollectableHasBeenCollected(currentTargetCollectable);
					currentTargetCollectable = null;
					Cheer();
				}
			}
			else if (Vector3.Distance(base.transform.position, myInitialPos) > 0f)
			{
				Vector3 vector2 = myInitialPos - base.transform.position;
				base.transform.forward = vector2.normalized;
				base.transform.position = Vector3.MoveTowards(base.transform.position, myInitialPos, Time.deltaTime * minionSpeed);
				if (Vector3.Distance(base.transform.position, myInitialPos) <= 0.1f)
				{
					StartCutting();
					base.transform.position = myInitialPos;
					base.transform.rotation = myInitialRot;
				}
			}
		}
	}
}
