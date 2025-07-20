using System;
using Kampai.Game;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class MinionLevelSelectorView : KampaiView
	{
		public GameObject SelectedImage;

		public Text Level;

		public Text Count;

		public Text SelectedLevel;

		public Text SelectedCount;

		public ScrollableButtonView SelectionButton;

		public float levelupAnimPulseScalar = 1.2f;

		public float levelupAnimPulseTime = 0.5f;

		private IPlayerService playerService;

		private IDefinitionService definitionService;

		public int index { get; set; }

		public void Init(IPlayerService playerService, IDefinitionService definitionService)
		{
			this.playerService = playerService;
			this.definitionService = definitionService;
			SetSelectedColor(GetLowestLevel(playerService, definitionService), false);
			UpdateMinionCountText();
		}

		internal void SetSelectedColor(int i, bool tryAnim)
		{
			bool flag = index == i;
			SelectedImage.SetActive(flag);
			if (index < GetLowestLevel(playerService, definitionService))
			{
				ToggleButtonInteractive(false);
			}
			else if (tryAnim && flag)
			{
				RectTransform target = base.transform as RectTransform;
				Vector3 originalScale = Vector3.one;
				TweenUtil.Throb(target, levelupAnimPulseScalar, levelupAnimPulseTime, out originalScale);
			}
		}

		internal void RefreshAllOfTypeArgsCallback(Type type, int index, GUIArguments args)
		{
			if (type == GetType())
			{
				SetSelectedColor(index, args.Contains<bool>() && args.Get<bool>());
			}
		}

		internal void UpdateMinionCountText()
		{
			int minionCountByLevel = playerService.GetMinionCountByLevel(index);
			Text selectedCount = SelectedCount;
			string text = minionCountByLevel.ToString();
			Count.text = text;
			selectedCount.text = text;
		}

		internal void ToggleButtonInteractive(bool isEnabled)
		{
			Button component = SelectionButton.GetComponent<Button>();
			if (!(component == null))
			{
				component.interactable = isEnabled;
			}
		}

		public static int GetLowestLevel(IPlayerService playerService, IDefinitionService definitionService)
		{
			int result = 0;
			int count = definitionService.Get<MinionBenefitLevelBandDefintion>(89898).minionBenefitLevelBands.Count;
			for (int i = 0; i < count; i++)
			{
				int minionCountByLevel = playerService.GetMinionCountByLevel(i);
				if (minionCountByLevel > 0)
				{
					result = i;
					break;
				}
			}
			return result;
		}

		public void SetLevelText(string levelText)
		{
			Text selectedLevel = SelectedLevel;
			Level.text = levelText;
			selectedLevel.text = levelText;
		}
	}
}
