using UnityEngine;

namespace Kampai.Game.Mignette.ButterflyCatch.View
{
	public class ButterflyCatchMignetteBeeAnimEvent : MonoBehaviour
	{
		public ButterflyCatchButterflyViewObject beeViewObject;

		public void PlayStingFX()
		{
			if (beeViewObject != null)
			{
				beeViewObject.BeeFireStingFX();
			}
		}

		public void CompleteStingAnim()
		{
			if (beeViewObject != null)
			{
				beeViewObject.CompleteStingAnim();
			}
		}
	}
}
