using Kampai.Game;
using Kampai.Game.View;
using UnityEngine;

public class AnimationTest : MonoBehaviour
{
	private Animator animator;

	public PhilCelebrateSignal celebrateSignal;

	public PhilGetAttentionSignal getAttentionSignal;

	public PhilPlayIntroSignal playIntroSignal;

	public PhilSitAtBarSignal sitAtBarSignal;

	public PhilActivateSignal activateSignal;

	public AnimatePhilSignal animatePhilSignal;

	public PhilGoToTikiBarSignal philGoToTikiBarSignal;

	public PhilEnableTikiBarControllerSignal enableTikiBarControllerSignal;

	private bool isGettingAttention;

	private bool isSittingAtBar;

	private bool isActive;

	private void Update()
	{
		if (!(animator != null))
		{
			GameObject gameObject = GameObject.Find("Phil");
			if ((bool)gameObject)
			{
				animator = gameObject.GetComponent<Animator>();
				PhilMediator component = gameObject.GetComponent<PhilMediator>();
				celebrateSignal = component.celebrateSignal;
				getAttentionSignal = component.getAttentionSignal;
				playIntroSignal = component.playIntroSignal;
				sitAtBarSignal = component.sitAtBarSignal;
				activateSignal = component.activateSignal;
				enableTikiBarControllerSignal = component.enableTikiBarControllerSignal;
			}
		}
	}

	private void OnGUI()
	{
		if (GUI.Button(new Rect(10f, 10f, 100f, 75f), "Celebrate"))
		{
			celebrateSignal.Dispatch();
		}
		if (GUI.Button(new Rect(10f, 95f, 100f, 75f), "Get Attention"))
		{
			isGettingAttention = !isGettingAttention;
			getAttentionSignal.Dispatch(isGettingAttention);
		}
		if (GUI.Button(new Rect(10f, 180f, 100f, 75f), "Play Intro"))
		{
			playIntroSignal.Dispatch(false);
		}
		if (GUI.Button(new Rect(10f, 265f, 100f, 75f), "Sit at Bar"))
		{
			isSittingAtBar = !isSittingAtBar;
			sitAtBarSignal.Dispatch(isSittingAtBar);
		}
		if (GUI.Button(new Rect(10f, 345f, 100f, 75f), "Activate"))
		{
			isActive = !isActive;
			activateSignal.Dispatch(isActive);
		}
		if (GUI.Button(new Rect(10f, 425f, 100f, 75f), "Enable TikiBar Controller"))
		{
			enableTikiBarControllerSignal.Dispatch();
		}
	}
}
