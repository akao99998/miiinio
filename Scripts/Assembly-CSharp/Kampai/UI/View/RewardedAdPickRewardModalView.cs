using System.Collections;
using Kampai.Common;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class RewardedAdPickRewardModalView : PopupMenuView
	{
		public Text Headline;

		public Button CollectButton;

		public Text CollectButtonButtonText;

		public Button[] Boxes;

		private ILocalizationService localService;

		private IDefinitionService definitionService;

		private IRandomService randomService;

		internal void Init(ILocalizationService localService, IDefinitionService definitionService, IRandomService randomService)
		{
			base.Init();
			this.localService = localService;
			this.definitionService = definitionService;
			this.randomService = randomService;
			Localize();
			base.Open();
		}

		private void Localize()
		{
			Headline.text = localService.GetString("RewardAdPickRewardHeadline");
			CollectButtonButtonText.text = localService.GetString("Collect");
		}

		internal void Select(int index, QuantityItem[] items)
		{
			int num = 0;
			Button[] boxes = Boxes;
			foreach (Button button in boxes)
			{
				if (num++ == index)
				{
					button.interactable = false;
				}
				else
				{
					button.enabled = false;
				}
			}
			ItemDefinition rewardItem = definitionService.Get<ItemDefinition>(items[0].ID);
			int quantity = (int)items[0].Quantity;
			AddReward(Boxes[index].transform, rewardItem, quantity, true);
			StartCoroutine(ShowUnselectedBoxes(index, items));
		}

		private IEnumerator ShowUnselectedBoxes(int index, QuantityItem[] items)
		{
			yield return new WaitForSeconds(1f);
			int j = 0;
			int[] itemsIndices = new int[2] { 1, 1 };
			itemsIndices[randomService.NextInt(2)]++;
			for (int i = 0; i < 3; i++)
			{
				if (i != index)
				{
					Button box = Boxes[i];
					box.enabled = true;
					box.interactable = false;
					int itemIndex = itemsIndices[j];
					AddReward(rewardItem: definitionService.Get<ItemDefinition>(items[itemIndex].ID), rewardAmount: (int)items[itemIndex].Quantity, parent: Boxes[i].transform, playVFX: false);
					j++;
				}
			}
			yield return new WaitForSeconds(1f);
			CollectButton.gameObject.SetActive(true);
		}

		private void AddReward(Transform parent, ItemDefinition rewardItem, int rewardAmount, bool playVFX)
		{
			GameObject original = KampaiResources.Load<GameObject>("rewardedAdReward");
			GameObject gameObject = Object.Instantiate(original);
			gameObject.transform.SetParent(parent, false);
			RewardedAdRewardView component = gameObject.GetComponent<RewardedAdRewardView>();
			component.Init(rewardItem, rewardAmount, playVFX);
		}
	}
}
