using System.Collections.Generic;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class QuestRewardPopupView : strange.extensions.mediation.impl.View
	{
		public ButtonView ConfirmButton;

		public Animator animator;

		public RectTransform rewardsList;

		private bool isOpened;

		internal void Init(List<DisplayableDefinition> itemDefs, ILocalizationService localService)
		{
			foreach (Transform rewards in rewardsList)
			{
				Object.Destroy(rewards.gameObject);
			}
			GameObject original = KampaiResources.Load("cmp_QuestPopupSlider") as GameObject;
			for (int i = 0; i < itemDefs.Count; i++)
			{
				GameObject gameObject = Object.Instantiate(original);
				QuestRewardSliderView component = gameObject.GetComponent<QuestRewardSliderView>();
				component.icon.sprite = UIUtils.LoadSpriteFromPath(itemDefs[i].Image);
				component.icon.maskSprite = UIUtils.LoadSpriteFromPath(itemDefs[i].Mask);
				component.description.text = localService.GetString(itemDefs[i].LocalizedKey);
				gameObject.transform.SetParent(rewardsList, false);
			}
		}

		internal void PlayAnim(bool open)
		{
			if (open)
			{
				animator.Play("anim_RewardsPopup_init");
				isOpened = true;
			}
			else if (isOpened)
			{
				animator.Play("anim_RewardsPopup_close");
				isOpened = false;
			}
		}
	}
}
