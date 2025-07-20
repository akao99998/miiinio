using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	[AddComponentMenu("UI/KampaiButton")]
	public class KampaiButton : Button
	{
		public void ChangeToNormalState()
		{
			DoStateTransition(SelectionState.Normal, false);
		}
	}
}
