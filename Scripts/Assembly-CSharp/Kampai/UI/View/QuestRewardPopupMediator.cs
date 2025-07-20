using System.Collections.Generic;
using Kampai.Game;
using Kampai.Main;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class QuestRewardPopupMediator : Mediator
	{
		[Inject]
		public QuestRewardPopupView view { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public QuestRewardPopupContentsSignal popupContentsSignal { get; set; }

		[Inject]
		public HideRewardDisplaySignal hideRewardDisplaySignal { get; set; }

		public override void OnRegister()
		{
			view.ConfirmButton.ClickedSignal.AddListener(Close);
			popupContentsSignal.AddListener(PopulateView);
			hideRewardDisplaySignal.AddListener(Close);
			ToggleOpen(false);
		}

		public override void OnRemove()
		{
			view.ConfirmButton.ClickedSignal.RemoveListener(Close);
			popupContentsSignal.RemoveListener(PopulateView);
			hideRewardDisplaySignal.RemoveListener(Close);
		}

		private void ToggleOpen(bool open)
		{
			view.PlayAnim(open);
		}

		private void PopulateView(List<DisplayableDefinition> itemDefs)
		{
			ToggleOpen(true);
			view.Init(itemDefs, localService);
		}

		private void Close()
		{
			ToggleOpen(false);
		}
	}
}
