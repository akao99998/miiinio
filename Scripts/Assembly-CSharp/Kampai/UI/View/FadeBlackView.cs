using System;
using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class FadeBlackView : KampaiView
	{
		public Image FadeBlackImage;

		private IList<Action> actionQueue;

		public float alpha
		{
			get
			{
				return FadeBlackImage.color.a;
			}
			set
			{
				FadeBlackImage.color = new Color(0f, 0f, 0f, value);
			}
		}

		public void Fade(bool fadeIn, IList<Action> actionQueue)
		{
			this.actionQueue = actionQueue;
			if (fadeIn)
			{
				FadeIn();
			}
			else
			{
				FadeOut();
			}
		}

		private void FadeIn()
		{
			FadeBlackImage.enabled = true;
			GoTweenConfig goTweenConfig = new GoTweenConfig();
			goTweenConfig.floatProp("alpha", 1f, true);
			Go.to(this, 0.25f, goTweenConfig).setOnCompleteHandler(FadeInComplete);
		}

		private void FadeOut()
		{
			GoTweenConfig goTweenConfig = new GoTweenConfig();
			goTweenConfig.floatProp("alpha", -1f, true);
			Go.to(this, 0.25f, goTweenConfig).setOnCompleteHandler(FadeOutComplete);
		}

		private void FadeInComplete(AbstractGoTween tween)
		{
			ProcessActions();
		}

		private void FadeOutComplete(AbstractGoTween tween)
		{
			FadeBlackImage.enabled = false;
			ProcessActions();
		}

		private void ProcessActions()
		{
			if (actionQueue == null)
			{
				return;
			}
			foreach (Action item in actionQueue)
			{
				item();
			}
			actionQueue.Clear();
		}
	}
}
