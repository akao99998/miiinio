using UnityEngine;
using UnityEngine.UI;
using strange.extensions.mediation.impl;

namespace Kampai.Util.AnimatorStateInfo
{
	public class AnimatorStateView : View
	{
		private Animator animator;

		private Text stateText;

		public void Initialize(Animator animator)
		{
			this.animator = animator;
			stateText = GetComponent<Text>();
			RectTransform component = GetComponent<RectTransform>();
			component.localScale = Vector3.one;
		}

		internal int? GetNameHash()
		{
			if (animator == null)
			{
				return null;
			}
			return animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
		}

		internal void UpdatePosition()
		{
			if (!(animator == null) && !(base.transform == null))
			{
				RectTransform component = GetComponent<RectTransform>();
				component.anchoredPosition = Camera.main.WorldToScreenPoint(animator.transform.position);
			}
		}

		internal void UpdateStateName(string stateName)
		{
			if (!(stateText == null))
			{
				if (animator == null)
				{
					stateText.text = string.Empty;
				}
				else
				{
					stateText.text = stateName;
				}
			}
		}
	}
}
