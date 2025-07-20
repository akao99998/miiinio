using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class OpenRewardedAdDailyRewardPickerModalCommand : Command
	{
		private const int TIER_COUNT = 3;

		public IKampaiLogger logger = LogManager.GetClassLogger("OpenRewardedAdDailyRewardPickerModalCommand") as IKampaiLogger;

		[Inject]
		public OnTheGlassDailyRewardDefinition onTheGlassDailyRewardDefinition { get; set; }

		[Inject]
		public AdPlacementInstance adPlacementInstance { get; set; }

		[Inject]
		public IRandomService randomService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public CloseAllOtherMenuSignal closeAllOtherMenuSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal sfxSignal { get; set; }

		public override void Execute()
		{
			closeAllOtherMenuSignal.Dispatch(null);
			QuantityItem[] array = PickRewardVariants(onTheGlassDailyRewardDefinition);
			if (array != null)
			{
				LoadRewardPickerUI(array);
				sfxSignal.Dispatch("Play_menu_popUp_01");
			}
		}

		public QuantityItem[] PickRewardVariants(OnTheGlassDailyRewardDefinition definition)
		{
			IList<IngredientsItemDefinition> list = GenerateCategory("Craftable");
			if (list.Count <= 0)
			{
				logger.Error("No items are unlocked yet, so there should be no daily reward avaiable.");
				return null;
			}
			QuantityItem[] array = new QuantityItem[3];
			OnTheGlassDailyRewardTier onTheGlassDailyRewardTier = PickTier(definition);
			array[0] = GetRewardOfTier(onTheGlassDailyRewardTier, list);
			OnTheGlassDailyRewardTier[] array2 = new OnTheGlassDailyRewardTier[3]
			{
				definition.RewardTiers.Tier1,
				definition.RewardTiers.Tier2,
				definition.RewardTiers.Tier3
			};
			int num = 1;
			OnTheGlassDailyRewardTier[] array3 = array2;
			foreach (OnTheGlassDailyRewardTier onTheGlassDailyRewardTier2 in array3)
			{
				if (!object.ReferenceEquals(onTheGlassDailyRewardTier2, onTheGlassDailyRewardTier))
				{
					array[num] = GetRewardOfTier(onTheGlassDailyRewardTier2, list);
					num++;
				}
			}
			if (randomService.NextBoolean())
			{
				QuantityItem quantityItem = array[1];
				array[1] = array[2];
				array[2] = quantityItem;
			}
			return array;
		}

		private OnTheGlassDailyRewardTier PickTier(OnTheGlassDailyRewardDefinition definition)
		{
			RewardTiers rewardTiers = definition.RewardTiers;
			int num = rewardTiers.Tier1.Weight + rewardTiers.Tier2.Weight + rewardTiers.Tier3.Weight;
			int num2 = randomService.NextInt(0, num);
			OnTheGlassDailyRewardTier result = rewardTiers.Tier1;
			if (num2 < rewardTiers.Tier1.Weight)
			{
				result = rewardTiers.Tier1;
			}
			else if (num2 < rewardTiers.Tier1.Weight + rewardTiers.Tier2.Weight)
			{
				result = rewardTiers.Tier2;
			}
			else if (num2 < num)
			{
				result = rewardTiers.Tier3;
			}
			return result;
		}

		private QuantityItem GetRewardOfTier(OnTheGlassDailyRewardTier tier, IList<IngredientsItemDefinition> unlockedCraftables)
		{
			WeightedDefinition weightedDefinition = new WeightedDefinition();
			List<WeightedQuantityItem> list2 = (List<WeightedQuantityItem>)(weightedDefinition.Entities = new List<WeightedQuantityItem>());
			foreach (WeightedQuantityItem entity in tier.PredefinedRewards.Entities)
			{
				list2.Add(entity);
			}
			int craftableRewardMinTier = tier.CraftableRewardMinTier;
			List<IngredientsItemDefinition> list3 = new List<IngredientsItemDefinition>();
			while (craftableRewardMinTier >= 0 && list3.Count == 0)
			{
				list3.AddRange(FilterByMinTier(unlockedCraftables, craftableRewardMinTier));
			}
			foreach (IngredientsItemDefinition item2 in list3)
			{
				uint quantity = (uint)randomService.NextInt(1, tier.CraftableRewardMaxQuantity + 1);
				WeightedQuantityItem weightedQuantityItem = new WeightedQuantityItem();
				weightedQuantityItem.ID = item2.ID;
				weightedQuantityItem.Quantity = quantity;
				weightedQuantityItem.Weight = (uint)tier.CraftableRewardWeight;
				WeightedQuantityItem item = weightedQuantityItem;
				list2.Add(item);
			}
			if (weightedDefinition.Entities.Count > 0)
			{
				WeightedInstance weightedInstance = new WeightedInstance(weightedDefinition);
				return weightedInstance.NextPick(randomService);
			}
			return null;
		}

		private IList<IngredientsItemDefinition> GenerateCategory(string category)
		{
			IList<IngredientsItemDefinition> list = new List<IngredientsItemDefinition>();
			IList<IngredientsItemDefinition> unlockedDefsByType = playerService.GetUnlockedDefsByType<IngredientsItemDefinition>();
			for (int i = 0; i < unlockedDefsByType.Count; i++)
			{
				if (category.Equals(unlockedDefsByType[i].TaxonomySpecific))
				{
					list.Add(unlockedDefsByType[i]);
				}
			}
			return list;
		}

		private IList<IngredientsItemDefinition> FilterByMinTier(IList<IngredientsItemDefinition> items, int minTier)
		{
			IList<IngredientsItemDefinition> list = new List<IngredientsItemDefinition>();
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i].Tier >= minTier)
				{
					list.Add(items[i]);
				}
			}
			return items;
		}

		private void LoadRewardPickerUI(QuantityItem[] rewards)
		{
			string text = "popup_RewardedAdPickDailyReward";
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Queue, text);
			iGUICommand.skrimScreen = "RewardedAdPickReward";
			iGUICommand.disableSkrimButton = true;
			iGUICommand.darkSkrim = true;
			GUIArguments args = iGUICommand.Args;
			args.Add(text);
			args.Add(typeof(QuantityItem[]), rewards);
			args.Add(typeof(AdPlacementInstance), adPlacementInstance);
			guiService.Execute(iGUICommand);
		}
	}
}
