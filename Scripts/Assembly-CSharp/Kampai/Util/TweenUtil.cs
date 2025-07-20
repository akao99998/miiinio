using UnityEngine;

namespace Kampai.Util
{
	public static class TweenUtil
	{
		public static GoTween Throb(Transform target, float scalar, float duration, out Vector3 originalScale, int interations = -1)
		{
			Vector3 endValue = new Vector3(target.localScale.x, target.localScale.y, target.localScale.z);
			endValue *= scalar;
			originalScale = target.localScale;
			ScaleTweenProperty tweenProp = new ScaleTweenProperty(endValue);
			GoTweenConfig goTweenConfig = new GoTweenConfig();
			goTweenConfig.loopType = GoLoopType.PingPong;
			goTweenConfig.addTweenProperty(tweenProp);
			goTweenConfig.iterations = interations;
			goTweenConfig.easeType = GoEaseType.SineInOut;
			GoTween goTween = new GoTween(target, duration, goTweenConfig);
			Go.addTween(goTween);
			goTween.play();
			return goTween;
		}

		public static GoTween Bounce(Transform target, Vector3 localOffset, float duration)
		{
			PositionTweenProperty tweenProp = new PositionTweenProperty(localOffset, true, true);
			GoTweenConfig goTweenConfig = new GoTweenConfig();
			goTweenConfig.loopType = GoLoopType.PingPong;
			goTweenConfig.addTweenProperty(tweenProp);
			goTweenConfig.iterations = -1;
			GoTween goTween = new GoTween(target, duration, goTweenConfig);
			Go.addTween(goTween);
			goTween.play();
			return goTween;
		}

		public static void Cleanup(ref GoTween tween)
		{
			if (tween != null)
			{
				tween.destroy();
			}
			tween = null;
		}

		public static void Cleanup(ref GoTweenChain chain)
		{
			if (chain != null)
			{
				chain.destroy();
			}
			chain = null;
		}
	}
}
