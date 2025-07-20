using System;
using System.Collections;
using Kampai.Util;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	[RequireComponent(typeof(Animator))]
	public class DiscoGlobeView : strange.extensions.mediation.impl.View
	{
		public GameObject DiscoGlobeMesh;

		public GameObject Disco3DElementsParent;

		public GameObject EffectsParentObject;

		public GameObject CameraControlsPanel;

		private Animator animator;

		protected override void Start()
		{
			if (Application.isPlaying)
			{
				base.Start();
			}
			animator = GetComponent<Animator>();
		}

		internal void ShowDiscoBallAwesomeness()
		{
			Disco3DElementsParent.SetActive(true);
			if (animator != null)
			{
				animator.Play("anim_DiscoIntro");
			}
		}

		public void AnimationDoneCallback(string animationState, Action callback)
		{
			StartCoroutine(CheckAnimationComplete(animationState, callback));
		}

		public bool IsAnimationPlaying(string animationState)
		{
			AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
			return (currentAnimatorStateInfo.IsName(animationState) && currentAnimatorStateInfo.normalizedTime < 1f) || currentAnimatorStateInfo.loop || currentAnimatorStateInfo.normalizedTime < 1f;
		}

		internal IEnumerator CheckAnimationComplete(string animationState, Action callback)
		{
			yield return null;
			while (animator != null && IsAnimationPlaying(animationState))
			{
				yield return null;
			}
			if (callback != null)
			{
				callback();
			}
		}

		internal void RemoveDiscoBallAwesomeness(Action onFinishCallback)
		{
			if (animator != null)
			{
				animator.Play("anim_DiscoOutro");
				AnimationDoneCallback("anim_DiscoOutro", onFinishCallback);
			}
			else if (onFinishCallback != null)
			{
				onFinishCallback();
			}
		}

		internal void DisplayEffects(bool display)
		{
			EffectsParentObject.SetActive(display);
		}

		internal void ShowCameraControlsPanel(bool display)
		{
			CameraControlsPanel.SetActive(display);
			if (display)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(KampaiResources.Load<GameObject>("cmp_CameraControls"));
				gameObject.transform.SetParent(CameraControlsPanel.transform, false);
			}
		}

		internal void DisplayDisco3DElements(bool display)
		{
			Disco3DElementsParent.SetActive(display);
		}
	}
}
