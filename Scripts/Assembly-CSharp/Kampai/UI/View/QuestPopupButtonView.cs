using System.Collections.Generic;
using Kampai.Game;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class QuestPopupButtonView : ButtonView
	{
		public new Signal<List<DisplayableDefinition>> ClickedSignal = new Signal<List<DisplayableDefinition>>();

		public List<DisplayableDefinition> questRewards = new List<DisplayableDefinition>();

		public override void OnClickEvent()
		{
			if (PlaySoundOnClick)
			{
				base.playSFXSignal.Dispatch("Play_button_click_01");
			}
			ClickedSignal.Dispatch(questRewards);
		}
	}
}
