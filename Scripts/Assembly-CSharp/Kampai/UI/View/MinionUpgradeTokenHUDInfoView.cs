using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	[RequireComponent(typeof(Animator))]
	public class MinionUpgradeTokenHUDInfoView : KampaiView
	{
		public Text tokenText;

		internal Animator animator;

		internal void Init()
		{
			animator = GetComponent<Animator>();
		}

		internal void SetText(string text)
		{
			tokenText.text = text;
		}
	}
}
