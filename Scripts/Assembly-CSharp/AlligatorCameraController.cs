using System;
using Kampai.Game.Mignette.AlligatorSkiing.View;
using UnityEngine;

public class AlligatorCameraController : MonoBehaviour
{
	private Action onTweenComplete;

	private GoTween currentTween;

	private AlligatorSkiingMignetteManagerView parentView;

	public void Start()
	{
		parentView = UnityEngine.Object.FindObjectOfType<AlligatorSkiingMignetteManagerView>();
	}

	public void AlignWithTransform(Transform t, float duration, GoEaseType easeType, Action onComplete = null)
	{
		if (!parentView.isGameOver)
		{
			if (currentTween != null && currentTween.state == GoTweenState.Running)
			{
				Go.removeTween(currentTween);
			}
			GoTweenConfig goTweenConfig = new GoTweenConfig();
			PositionTweenProperty tweenProp = new PositionTweenProperty(t.localPosition, false, true);
			goTweenConfig.addTweenProperty(tweenProp);
			goTweenConfig.easeType = easeType;
			RotationTweenProperty tweenProp2 = new RotationTweenProperty(t.localRotation.eulerAngles, false, true);
			goTweenConfig.addTweenProperty(tweenProp2);
			onTweenComplete = onComplete;
			goTweenConfig.onComplete(OnTweenComplete);
			currentTween = new GoTween(parentView.mignetteCamera.transform, duration, goTweenConfig);
			Go.addTween(currentTween);
		}
	}

	private void OnTweenComplete(AbstractGoTween obj)
	{
		if (onTweenComplete != null)
		{
			onTweenComplete();
		}
	}
}
