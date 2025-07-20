using System.Collections;
using System.Collections.Generic;
using Kampai.Common;
using Kampai.Util;
using Kampai.Util.Graphics;
using UnityEngine;

namespace Kampai.Game.View
{
	public class InnerTubeObject : MonoBehaviour
	{
		public List<Vector3> tubeStartLocations;

		public List<float> tubeRotations;

		public List<float> tubeWaitBeforeFadeoutSeconds;

		public List<float> tubeFadeoutSeconds;

		public List<Renderer> availableRenderers;

		public List<Texture> availableTextures;

		public Animator tubeAnimator;

		private float waitSeconds = 15f;

		private float fadeSeconds = 5f;

		private GoTween tubeTween;

		private float m_fadeAlpha;

		private MaterialModifier materialModifier;

		public float FadeAlpha
		{
			get
			{
				return materialModifier.GetFadeAlpha();
			}
			set
			{
				materialModifier.SetFadeAlpha(value);
			}
		}

		private void Awake()
		{
		}

		public void SetTubeNumberAndFloatAway(int routeNumber, IRoutineRunner routineRunner, IRandomService randomService)
		{
			int index = 0;
			if (routeNumber < tubeStartLocations.Count && routeNumber < tubeRotations.Count && routeNumber < tubeWaitBeforeFadeoutSeconds.Count && routeNumber < tubeFadeoutSeconds.Count)
			{
				index = routeNumber;
			}
			waitSeconds = tubeWaitBeforeFadeoutSeconds[index];
			fadeSeconds = tubeFadeoutSeconds[index];
			base.gameObject.transform.position = tubeStartLocations[index];
			base.gameObject.transform.rotation = Quaternion.AngleAxis(tubeRotations[index], Vector3.up);
			StartAnimation(routineRunner, randomService);
		}

		public void StartAnimation(IRoutineRunner rout, IRandomService rand)
		{
			int index = rand.NextInt(availableTextures.Count);
			availableRenderers[0].material.mainTexture = availableTextures[index];
			tubeAnimator.applyRootMotion = true;
			tubeAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
			tubeAnimator.Play("FloatAway");
			FadeTubeOut(rout);
		}

		private void FadeTubeOut(IRoutineRunner rout)
		{
			if (tubeTween != null && tubeTween.isValid())
			{
				tubeTween.destroy();
			}
			materialModifier = materialModifier ?? new MaterialModifier(availableRenderers);
			materialModifier.SetFadeAlpha(1f);
			tubeTween = Go.to(this, fadeSeconds, new GoTweenConfig().setDelay(waitSeconds).floatProp("FadeAlpha", 0f).onComplete(delegate(AbstractGoTween tubeTweenArg)
			{
				tubeTweenArg.destroy();
				if (rout != null)
				{
					rout.StartCoroutine(Suicide());
				}
			}));
		}

		private IEnumerator Suicide()
		{
			yield return new WaitForSeconds(1f);
			Object.Destroy(base.gameObject);
		}
	}
}
