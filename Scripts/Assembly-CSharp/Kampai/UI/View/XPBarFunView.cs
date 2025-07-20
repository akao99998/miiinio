using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class XPBarFunView : strange.extensions.mediation.impl.View
	{
		public ParticleSystem XPStarVFX;

		public ParticleSystem XPImageVFX;

		public RectTransform FillImage;

		public Image FillBarBacking;

		public Text XPAmount;

		public Text LevelAmount;

		public Text AnnouncementText;

		[Header("FTUE Highlight")]
		public RectTransform FTUEHighlightMeter;

		public RectTransform FTUEHighlightXp;

		[Header("Party Time Highlight")]
		public RectTransform PartyTimeHighlightMeter;

		public RectTransform PartyTimeHighlightXp;

		public RectTransform InspirationLevelPanelHighlight;

		public GameObject inspirationMeterSeparator;

		public KampaiImage funIcon;

		[Header("Animation Variables")]
		public float pulseScale = 0.85f;

		public float pulseRate = 0.5f;

		internal bool expTweenAudio = true;

		internal bool expIsTweening;

		internal Signal animateXP = new Signal();

		private List<GameObject> dividers;

		private GoTween _pointsTextTween;

		private GoTween _pointsFillTween;

		public int expTweenCount { get; set; }

		public void Init(IPositionService positionService)
		{
			dividers = new List<GameObject>();
			positionService.AddHUDElementToAvoid(base.gameObject);
		}

		public void SetXP(uint xp, uint maxXPThisLevel)
		{
			if (maxXPThisLevel != 0)
			{
				TweenXPText(xp, maxXPThisLevel);
				TweenXPBar(xp, maxXPThisLevel);
			}
		}

		internal void TweenXPText(uint xp, uint maxXP)
		{
			if (xp > maxXP)
			{
				xp = maxXP;
			}
			if (_pointsTextTween != null)
			{
				_pointsTextTween.destroy();
			}
			_pointsTextTween = Go.to(this, 1f, new GoTweenConfig().intProp("expTweenCount", (int)xp).onUpdate(delegate
			{
				SetXPText((uint)expTweenCount, maxXP);
			}).onComplete(delegate
			{
				_pointsTextTween.destroy();
				_pointsTextTween = null;
			}));
		}

		internal void TweenXPBar(uint xp, uint maxXP, float speed = 1f)
		{
			if (xp >= maxXP)
			{
				if (FillImage.anchorMax.x >= 1f)
				{
					return;
				}
				xp = maxXP;
			}
			if (!AnnouncementText.gameObject.activeSelf)
			{
				if (_pointsFillTween != null)
				{
					_pointsFillTween.destroy();
				}
				_pointsFillTween = Go.to(FillImage, speed, new GoTweenConfig().vector2Prop("anchorMax", new Vector2((float)xp / (float)maxXP, 1f)).onComplete(delegate
				{
					expTweenAudio = false;
					_pointsFillTween.destroy();
					_pointsFillTween = null;
				}));
			}
		}

		internal void SetXPText(uint xp, uint maxXP)
		{
			XPAmount.text = string.Format("{0}/{1}", (int)xp, (int)maxXP);
		}

		public void SetLevel(List<int> pointsEachPartyNeeds, int level)
		{
			LevelAmount.text = level.ToString();
			ClearSegments();
			SetSegments(pointsEachPartyNeeds);
			ClearBar();
		}

		public void ClearBar()
		{
			expTweenCount = 0;
			FillImage.anchorMax = new Vector2(0f, 1f);
		}

		public void SetSegments(List<int> pointsPerEachParty)
		{
			float x = 0f;
			float num = pointsPerEachParty.Sum();
			float num2 = 0f;
			Transform parent = FillBarBacking.transform;
			for (int i = 0; i < pointsPerEachParty.Count; i++)
			{
				GameObject gameObject = Object.Instantiate(inspirationMeterSeparator);
				RectTransform component = gameObject.GetComponent<RectTransform>();
				component.SetParent(parent, false);
				component.SetAsFirstSibling();
				component.localScale = Vector2.one;
				component.sizeDelta = Vector2.zero;
				num2 += (float)pointsPerEachParty[i];
				component.anchorMin = new Vector2(x, 0f);
				x = num2 / num;
				component.anchorMax = new Vector2(x, 1f);
				dividers.Add(gameObject);
			}
		}

		public void ClearSegments()
		{
			foreach (GameObject divider in dividers)
			{
				Object.Destroy(divider);
			}
		}

		internal void PlayInitialVFX()
		{
			XPStarVFX.Play();
			if (!expIsTweening)
			{
				Go.to(FillBarBacking, 0.5f, new GoTweenConfig().colorProp("color", Color.white).setIterations(2, GoLoopType.PingPong).onBegin(delegate
				{
					expIsTweening = true;
					ShowPartyTimeHighlight(true);
				})
					.onComplete(delegate
					{
						expIsTweening = false;
						ShowPartyTimeHighlight(false);
					}));
			}
		}

		internal void PlayXPVFX()
		{
			XPStarVFX.Play();
		}

		public void ShowFTUEXP(bool show)
		{
			FTUEHighlightMeter.gameObject.SetActive(show);
			FTUEHighlightXp.gameObject.SetActive(show);
		}

		public void ShowPartyTimeHighlight(bool show)
		{
			PartyTimeHighlightMeter.gameObject.SetActive(show);
		}

		public void SetAnnouncementText(string newAnnouncementText)
		{
			AnnouncementText.text = newAnnouncementText;
			AnnouncementText.gameObject.SetActive(true);
			XPAmount.gameObject.SetActive(false);
			FillImage.gameObject.SetActive(false);
			ShowPartyTimeHighlight(true);
		}

		public void ClearAnnouncement()
		{
			AnnouncementText.gameObject.SetActive(false);
			XPAmount.gameObject.SetActive(true);
			FillImage.gameObject.SetActive(true);
			ShowPartyTimeHighlight(false);
		}
	}
}
