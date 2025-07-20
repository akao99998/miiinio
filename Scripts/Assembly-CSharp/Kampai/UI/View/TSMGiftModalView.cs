using System;
using Kampai.Game.Trigger;
using Kampai.Game.View;
using Kampai.Main;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class TSMGiftModalView : PopupMenuView
	{
		public LocalizeView Title;

		public LocalizeView CaptainMessage;

		public MinionSlotModal MinionSlot;

		public RectTransform itemPanel;

		public Signal<TriggerRewardDefinition> OnRewardCollected = new Signal<TriggerRewardDefinition>();

		private DummyCharacterObject dummyCharacterObject;

		private IGUIService guiService;

		public void InitializeView(TriggerInstance instance, IFancyUIService fancyUIService, IGUIService guiService, MoveAudioListenerSignal moveAudioListenerSignal)
		{
			Init();
			this.guiService = guiService;
			TriggerDefinition definition = instance.Definition;
			Title.gameObject.SetActive(true);
			Title.LocKey = definition.Title;
			CaptainMessage.gameObject.SetActive(true);
			CaptainMessage.LocKey = definition.Description;
			if (dummyCharacterObject == null)
			{
				DummyCharacterType type = DummyCharacterType.NamedCharacter;
				dummyCharacterObject = fancyUIService.CreateCharacter(type, DummyCharacterAnimationState.SelectedHappy, MinionSlot.transform, MinionSlot.VillainScale, MinionSlot.VillainPositionOffset, 40014);
				dummyCharacterObject.gameObject.SetActive(true);
				moveAudioListenerSignal.Dispatch(false, dummyCharacterObject.transform);
			}
			for (int i = 0; i < definition.rewards.Count; i++)
			{
				SetupItemPanels(instance, definition.rewards[i]);
			}
			Open();
		}

		public void Release()
		{
			if (dummyCharacterObject != null && dummyCharacterObject.gameObject != null)
			{
				UnityEngine.Object.Destroy(dummyCharacterObject.gameObject);
				dummyCharacterObject = null;
			}
			TSMItemPanelView[] componentsInChildren = itemPanel.GetComponentsInChildren<TSMItemPanelView>();
			if (componentsInChildren == null)
			{
				return;
			}
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (!(componentsInChildren[i] == null) && !(componentsInChildren[i].gameObject == null))
				{
					UnityEngine.Object.Destroy(componentsInChildren[i].gameObject);
				}
			}
		}

		public void SetupItemPanels(TriggerInstance instance, TriggerRewardDefinition reward)
		{
			if (reward != null && reward.type != TriggerRewardType.Identifier.Upsell)
			{
				IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.LoadUntrackedInstance, "cmp_TSM_SellingItems");
				GUIArguments args = iGUICommand.Args;
				args.Add(typeof(TriggerRewardDefinition), reward);
				args.Add(typeof(Transform), itemPanel);
				args.Add(typeof(Action<TriggerRewardDefinition>), new Action<TriggerRewardDefinition>(OnPurchasedCallback));
				args.Add(typeof(bool), instance.RecievedRewardIds.Contains(reward.ID));
				guiService.Execute(iGUICommand);
			}
		}

		private void OnPurchasedCallback(TriggerRewardDefinition rewardDefinition)
		{
			OnRewardCollected.Dispatch(rewardDefinition);
		}
	}
}
