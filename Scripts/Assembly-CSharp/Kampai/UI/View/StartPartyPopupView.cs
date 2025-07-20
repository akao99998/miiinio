using UnityEngine;

namespace Kampai.UI.View
{
	public class StartPartyPopupView : PopupMenuView
	{
		public ButtonView accept;

		public RectTransform congratsHeaderTransform;

		public override void Init()
		{
			base.Init();
			base.Open();
		}

		internal void PulseStartButton()
		{
			Animator component = accept.GetComponent<Animator>();
			if (!(component == null))
			{
				component.SetBool("Pulse", true);
			}
		}

		internal void CenterHeader()
		{
			congratsHeaderTransform.anchoredPosition = new Vector2(0.5f, 0.5f);
		}
	}
}
