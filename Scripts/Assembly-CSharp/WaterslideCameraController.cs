using System;
using UnityEngine;

public class WaterslideCameraController : MonoBehaviour
{
	private Camera gameCamera;

	private Transform cameraTransform;

	private Action onTweenComplete;

	private Action onFOVTweenComplete;

	private GoTween currentAlignmentTween;

	private GoTween currentFOVTween;

	public void SetCamera(Camera c)
	{
		gameCamera = c;
		cameraTransform = c.transform;
	}

	public void AlignWithTransform(Transform t, float duration, GoEaseType easeType, Action onComplete = null)
	{
		if (currentAlignmentTween != null && currentAlignmentTween.state == GoTweenState.Running)
		{
			Go.removeTween(currentAlignmentTween);
		}
		GoTweenConfig goTweenConfig = new GoTweenConfig();
		PositionTweenProperty tweenProp = new PositionTweenProperty(t.position);
		goTweenConfig.addTweenProperty(tweenProp);
		goTweenConfig.easeType = easeType;
		RotationTweenProperty tweenProp2 = new RotationTweenProperty(t.rotation.eulerAngles);
		goTweenConfig.addTweenProperty(tweenProp2);
		onTweenComplete = onComplete;
		goTweenConfig.onComplete(OnTweenComplete);
		currentAlignmentTween = new GoTween(cameraTransform, duration, goTweenConfig);
		Go.addTween(currentAlignmentTween);
	}

	public void AlignWithFOV(float fov, float duration, GoEaseType easeType, Action onComplete = null)
	{
		if (currentFOVTween != null && currentFOVTween.state == GoTweenState.Running)
		{
			Go.removeTween(currentFOVTween);
		}
		GoTweenConfig goTweenConfig = new GoTweenConfig();
		FloatTweenProperty tweenProp = new FloatTweenProperty("fieldOfView", fov);
		goTweenConfig.addTweenProperty(tweenProp);
		goTweenConfig.easeType = easeType;
		onFOVTweenComplete = onComplete;
		goTweenConfig.onComplete(OnFOVTweenComplete);
		currentFOVTween = new GoTween(gameCamera, duration, goTweenConfig);
		Go.addTween(currentFOVTween);
	}

	private void OnFOVTweenComplete(AbstractGoTween obj)
	{
		if (onFOVTweenComplete != null)
		{
			onFOVTweenComplete();
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
