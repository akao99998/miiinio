using System.Collections;
using Kampai.Main;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class QuestMediator : Mediator
	{
		[Inject]
		public QuestView view { get; set; }

		[Inject]
		public UpdateQuestPanelWithNewQuestSignal updateQuestPanelSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal globalSFX { get; set; }

		[Inject]
		public QuestUIModel questUIModel { get; set; }

		[Inject]
		public IFancyUIService fancyUIService { get; set; }

		public override void OnRegister()
		{
			view.button.ClickedSignal.AddListener(Clicked);
			view.Init(fancyUIService);
		}

		public override void OnRemove()
		{
			view.button.ClickedSignal.RemoveListener(Clicked);
		}

		private void Clicked()
		{
			if (questUIModel.lastSelectedQuestID != view.quest.ID)
			{
				StartCoroutine(WaitAFrame());
			}
		}

		private IEnumerator WaitAFrame()
		{
			yield return null;
			globalSFX.Dispatch("Play_button_click_01");
			updateQuestPanelSignal.Dispatch(view.quest.ID);
		}
	}
}
