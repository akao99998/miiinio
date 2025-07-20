using System;
using Kampai.Game;
using Kampai.Util;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class MinionBenefitView : KampaiView
	{
		private const string MINION_ABILITY_BAR_GLOW_MASK = "img_minion_glow_abilities_mask";

		private const string MINION_ABILITY_BAR_MASK = "img_minion_abilities_mask";

		public LocalizeView Ability;

		public KampaiImage AbilityImage;

		public Color disabledBarColor;

		public Color highlightedBarColor;

		public KampaiImage[] LevelBarImages;

		public GameObject ItemBackgroundPanel;

		public KampaiImage ItemBlueRingBackground;

		public float levelBarPulseScaler = 1.2f;

		public float levelBarPulseTime = 1.25f;

		private int currentLevel;

		private MinionBenefitLevelBandDefintion LevelBandDef;

		private Benefit m_category;

		private IDefinitionService definitionService;

		public int levelBarAnimInterations = 4;

		internal Signal triggerAbilityBarAudio = new Signal();

		public Benefit category
		{
			get
			{
				return m_category;
			}
			set
			{
				if (m_category != value)
				{
					m_category = value;
					SetImage();
				}
			}
		}

		public void Init(IDefinitionService definitionService, IPlayerService playerService)
		{
			this.definitionService = definitionService;
			LevelBandDef = definitionService.Get<MinionBenefitLevelBandDefintion>(89898);
			SetImage();
			UpdateBenefits(MinionLevelSelectorView.GetLowestLevel(playerService, definitionService), null);
		}

		internal KampaiImage GetLevelBar(int level)
		{
			if (level < 0 || level >= LevelBarImages.Length)
			{
				return null;
			}
			return LevelBarImages[level];
		}

		internal void RefreshAllOfTypeArgsCallback(Type type, GUIArguments args)
		{
			if (type == GetType() && args.Contains<int>() && (!args.Contains<Benefit>() || args.Get<Benefit>() == category))
			{
				UpdateBenefits(args.Get<int>(), args);
			}
		}

		internal void SetLevel(int level)
		{
			currentLevel = level;
			for (int i = 0; i < LevelBarImages.Length; i++)
			{
				if (i < level)
				{
					LevelBarImages[i].maskSprite = UIUtils.LoadSpriteFromPath("img_minion_glow_abilities_mask");
					LevelBarImages[i].color = highlightedBarColor;
				}
				else
				{
					LevelBarImages[i].maskSprite = UIUtils.LoadSpriteFromPath("img_minion_abilities_mask");
					LevelBarImages[i].color = disabledBarColor;
				}
			}
		}

		private void SetValues(int level, GUIArguments args)
		{
			int num = currentLevel;
			if (currentLevel >= level || args == null || !args.Get<bool>() || !args.Contains<Signal<float, Tuple<int, int>>>())
			{
				SetLevel(level);
				return;
			}
			for (int i = num; i < level; i++)
			{
				args.Get<Signal<float, Tuple<int, int>>>().Dispatch(levelBarPulseTime * (float)levelBarAnimInterations, new Tuple<int, int>((int)category, i + 1));
			}
		}

		internal void AnimateLevelBar(int level)
		{
			SetLevel(level);
			KampaiImage levelBar = GetLevelBar(level - 1);
			if (!(levelBar == null))
			{
				RectTransform target = levelBar.transform as RectTransform;
				Vector3 originalScale = Vector3.one;
				triggerAbilityBarAudio.Dispatch();
				TweenUtil.Throb(target, levelBarPulseScaler, levelBarPulseTime, out originalScale, levelBarAnimInterations);
			}
		}

		private void SetImage()
		{
			if (LevelBandDef != null)
			{
				Ability.LocKey = LevelBandDef.benefitDescriptions[(int)category].localizedKey;
				int itemIconId = LevelBandDef.benefitDescriptions[(int)category].itemIconId;
				if (itemIconId == 0 || itemIconId == 1)
				{
					ItemBackgroundPanel.SetActive(false);
					UIUtils.SetItemIcon(AbilityImage, definitionService.Get<DisplayableDefinition>(itemIconId));
					return;
				}
				ItemDefinition itemDefinition = definitionService.Get<ItemDefinition>(itemIconId);
				UIUtils.SetItemIcon(AbilityImage, itemDefinition);
				ItemBackgroundPanel.SetActive(true);
				ItemBlueRingBackground.gameObject.SetActive(!(itemDefinition is DropItemDefinition));
			}
		}

		private void UpdateBenefits(int index, GUIArguments args)
		{
			int level = 0;
			switch (category)
			{
			case Benefit.DOUBLE_DROP:
				level = LevelBandDef.minionBenefitLevelBands[index].doubleDropLevel;
				break;
			case Benefit.PREMIUM:
				level = LevelBandDef.minionBenefitLevelBands[index].premiumDropLevel;
				break;
			case Benefit.RARE_DROP:
				level = LevelBandDef.minionBenefitLevelBands[index].rareDropLevel;
				break;
			}
			SetValues(level, args);
		}
	}
}
