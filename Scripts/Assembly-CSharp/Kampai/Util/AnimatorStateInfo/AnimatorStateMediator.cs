using System.Collections.Generic;
using strange.extensions.mediation.impl;

namespace Kampai.Util.AnimatorStateInfo
{
	public class AnimatorStateMediator : Mediator
	{
		private const string unknownState = "Unknown";

		[Inject(UtilElement.ANIMATOR_STATE_DEBUG_INFO)]
		public Dictionary<int, string> animatorStateInfo { get; set; }

		[Inject]
		public AnimatorStateView view { get; set; }

		private void Update()
		{
			if (animatorStateInfo == null)
			{
				return;
			}
			view.UpdatePosition();
			int? nameHash = view.GetNameHash();
			if (!nameHash.HasValue)
			{
				view.UpdateStateName(string.Empty);
				return;
			}
			int value = nameHash.Value;
			if (!animatorStateInfo.ContainsKey(value))
			{
				view.UpdateStateName("Unknown");
			}
			else
			{
				view.UpdateStateName(animatorStateInfo[value]);
			}
		}
	}
}
