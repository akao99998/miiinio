using UnityEngine;
using UnityEngine.UI;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class SocialPartyFillOrderView : PopupMenuView
	{
		public ButtonView leaveTeamButton;

		public ButtonView messageAlertButton;

		public ButtonView teamPanelButton;

		public ButtonView closeTeamButton;

		public GameObject SocialFillOrderButtonContainer;

		public GameObject SocialFillTeamPanel;

		public Animator teamPanelAnimator;

		public Text premiumRewardText;

		public Text grindRewardText;

		public Text leaveTeamButtonText;

		public Text teamTitle;

		public Text timeRemaining;

		public Text ordersRemaining;

		public Text teamOrderBoardText;

		public Text descriptionText;

		public Image progressBar;

		public Text teamButtonText;

		public Text questTitle;

		public Signal<bool> FillOrderSignal = new Signal<bool>();

		public override void Init()
		{
			base.Init();
			base.Open();
		}

		internal void OpenTeam()
		{
			teamPanelAnimator.Play("Open");
		}

		internal void CloseTeam()
		{
			teamPanelAnimator.Play("Close");
		}
	}
}
