using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class SocialPartyFBConnectView : PopupMenuView
	{
		public ButtonView connectButton;

		public ButtonView quitButton;

		public Text connectButtonText;

		public Text txtHeadline;

		public Text txtDescription;

		public List<Animator> objectiveAnimators;

		public float animationDelay = 0.5f;

		public override void Init()
		{
			base.Init();
			base.Open();
			StartCoroutine(DisplayObjectives());
		}

		private IEnumerator DisplayObjectives()
		{
			yield return new WaitForSeconds(animationDelay);
			for (int i = 0; i < objectiveAnimators.Count; i++)
			{
				objectiveAnimators[i].Play("Open");
				yield return new WaitForSeconds(animationDelay);
			}
		}
	}
}
