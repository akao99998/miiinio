using Kampai.Main;
using UnityEngine;

public class WaterslideSpinnerViewObject : MonoBehaviour
{
	public enum ParticleState
	{
		None = 0,
		Intro = 1,
		Spin = 2,
		Land = 3,
		Invalid = 4
	}

	private const string INTRO_ANIM_TRIGGER_NAME = "PlayIntro";

	private const string DIVE_ANIM_INT_NAME = "DiveSelected";

	public Animator SpinnerAnimator;

	private PlayGlobalSoundFXSignal playGlobalAudioSignal;

	public GameObject IntroVfxGameObject;

	public GameObject SpinVfxGameObject;

	public GameObject LandVfxGameObject;

	private ParticleState lastState = ParticleState.Invalid;

	public void Start()
	{
		if (lastState == ParticleState.Invalid)
		{
			SetParticleState(ParticleState.None);
		}
	}

	public void SetParticleState(ParticleState state)
	{
		if (lastState != state)
		{
			lastState = state;
			if (IntroVfxGameObject != null)
			{
				IntroVfxGameObject.SetActive(state == ParticleState.Intro);
			}
			if (SpinVfxGameObject != null)
			{
				SpinVfxGameObject.SetActive(state == ParticleState.Spin);
			}
			if (LandVfxGameObject != null)
			{
				LandVfxGameObject.SetActive(state == ParticleState.Land);
			}
		}
	}

	public void StartIntro(PlayGlobalSoundFXSignal audioSignal)
	{
		playGlobalAudioSignal = audioSignal;
		SpinnerAnimator.SetTrigger("PlayIntro");
		SetParticleState(ParticleState.Intro);
	}

	public void SelectDive(int diveIndex)
	{
		SetParticleState(ParticleState.Land);
		if (playGlobalAudioSignal != null)
		{
			playGlobalAudioSignal.Dispatch("Play_poseSelect_01");
		}
		SpinnerAnimator.SetInteger("DiveSelected", diveIndex);
	}

	public void OnSpinLoopAnimEvent()
	{
		if (lastState != ParticleState.Land)
		{
			SetParticleState(ParticleState.Spin);
		}
		if (playGlobalAudioSignal != null)
		{
			playGlobalAudioSignal.Dispatch("Play_poseShuffle_01");
		}
	}

	public float GetAnimationPct()
	{
		return SpinnerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f;
	}
}
