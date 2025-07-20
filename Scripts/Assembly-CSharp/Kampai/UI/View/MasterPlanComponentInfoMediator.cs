using System.Collections.Generic;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;

namespace Kampai.UI.View
{
	public class MasterPlanComponentInfoMediator : KampaiMediator
	{
		private MasterPlanDefinition planDefinition;

		private MasterPlanComponentDefinition componentDefinition;

		private MasterPlanComponent componentInstance;

		[Inject]
		public MasterPlanComponentInfoView view { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public RefreshFromIndexSignal refreshFromIndexSignal { get; set; }

		public override void Initialize(GUIArguments args)
		{
			planDefinition = args.Get<MasterPlanDefinition>();
			view.Init(args.Get<MasterPlanComponentInfoView.ItemType>(), args.Get<int>());
		}

		public override void OnRegister()
		{
			base.OnRegister();
			view.updateItemsSignal.AddListener(UpdateItemCallback);
			view.setTaskIconSignal.AddListener(SetTaskRequiresIcon);
			view.setRewardIconSignal.AddListener(SetRewardIcon);
			refreshFromIndexSignal.AddListener(view.Refresh);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			view.updateItemsSignal.RemoveListener(UpdateItemCallback);
			view.setTaskIconSignal.RemoveListener(SetTaskRequiresIcon);
			view.setRewardIconSignal.RemoveListener(SetRewardIcon);
			refreshFromIndexSignal.RemoveListener(view.Refresh);
		}

		public void UpdateItemCallback()
		{
			if (planDefinition != null)
			{
				componentDefinition = definitionService.Get<MasterPlanComponentDefinition>(planDefinition.ComponentDefinitionIDs[view.index]);
				componentInstance = playerService.GetFirstInstanceByDefinitionId<MasterPlanComponent>(componentDefinition.ID);
				switch (view.itemType)
				{
				case MasterPlanComponentInfoView.ItemType.Requires:
					UpdateRequiresIcons();
					break;
				case MasterPlanComponentInfoView.ItemType.Rewards:
					UpdateRewardsIcons();
					break;
				}
			}
		}

		private void UpdateRewardsIcons()
		{
			List<QuantityItem> list = new List<QuantityItem>();
			list.Add(new QuantityItem(componentInstance.reward.Definition.rewardItemId, componentInstance.reward.Definition.rewardQuantity));
			List<QuantityItem> list2 = list;
			if (componentInstance.reward.Definition.premiumReward != 0)
			{
				list2.Add(new QuantityItem(1, componentInstance.reward.Definition.premiumReward));
			}
			if (componentInstance.reward.Definition.grindReward != 0)
			{
				list2.Add(new QuantityItem(0, componentInstance.reward.Definition.grindReward));
			}
			list2.Sort((QuantityItem x, QuantityItem y) => x.ID.CompareTo(y.ID));
			view.SetupRewardIcons(list2);
		}

		private void UpdateRequiresIcons()
		{
			view.SetupTasksIcons(componentInstance.tasks);
		}

		private void SetRewardIcon(QuantityItem rewardItem, KampaiImage icon)
		{
			if (rewardItem != null)
			{
				DisplayableDefinition itemDefinitions = definitionService.Get<DisplayableDefinition>(rewardItem.ID);
				UIUtils.SetItemIcon(icon, itemDefinitions);
			}
		}

		private void SetTaskRequiresIcon(MasterPlanComponentTask task, KampaiImage icon)
		{
			task.Definition.GetTaskImage(icon, localizationService, definitionService, string.Empty);
		}
	}
}
