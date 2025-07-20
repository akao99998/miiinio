using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class BuildMenuView : KampaiView
	{
		public ButtonView MenuButton;

		public RectTransform BackGround;

		public GameObject Root;

		public TabMenuView TabMenu;

		public StoreBadgeView StoreBadge;

		public RectTransform Backing;

		public RectTransform BackingGlow;

		internal bool isOpen;

		private Animator animator;

		internal void Init()
		{
			animator = GetComponent<Animator>();
			MenuButton.PlaySoundOnClick = false;
		}

		public void MoveMenu()
		{
			MoveMenu(!isOpen);
		}

		internal void MoveMenu(bool show)
		{
			animator.SetBool("OnOpen", show);
			isOpen = show;
			ToggleBadgeCounterVisibility(isOpen);
		}

		internal void IncreaseBadgeCounter()
		{
			StoreBadge.IncreaseBadgeCounter();
		}

		internal void ToggleBadgeCounterVisibility(bool isHide)
		{
			StoreBadge.ToggleBadgeCounterVisibility(isHide);
		}

		internal void RemoveUnlockBadge(int count)
		{
			StoreBadge.RemoveUnlockBadge(count);
		}

		internal void SetUnlockBadge(int count)
		{
			StoreBadge.SetNewUnlockCounter(count);
		}

		internal void SetBadgeCount(int count)
		{
			StoreBadge.SetBadgeCount(count);
		}

		internal void Toggle(bool show)
		{
			animator.SetBool("OnHide", !show);
		}

		internal bool IsHiding()
		{
			return animator.GetBool("OnHide");
		}

		public void SetBuildMenuButtonEnabled(bool isEnabled)
		{
			if (MenuButton != null && Backing != null && BackingGlow != null)
			{
				MenuButton.gameObject.SetActive(isEnabled);
				Backing.gameObject.SetActive(isEnabled);
				BackingGlow.gameObject.SetActive(isEnabled);
				StoreBadge.EnableBadge(isEnabled);
			}
		}

		internal void DisableBuildButton(bool disable)
		{
			MenuButton.GetComponent<Button>().enabled = !disable;
		}
	}
}
