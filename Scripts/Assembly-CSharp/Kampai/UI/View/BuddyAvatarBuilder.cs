using Kampai.Game;
using Kampai.Util;
using UnityEngine;

namespace Kampai.UI.View
{
	public static class BuddyAvatarBuilder
	{
		public static BuddyAvatarView Build(Prestige prestige, IPrestigeService prestigeService, IKampaiLogger logger)
		{
			if (prestige == null)
			{
				logger.Fatal(FatalCode.PS_NO_SUCH_PRESTIGE);
				return null;
			}
			PrestigeDefinition definition = prestige.Definition;
			if (definition == null)
			{
				return null;
			}
			Sprite characterImage;
			Sprite characterMask;
			prestigeService.GetCharacterImageBasedOnMood(definition, CharacterImageType.SmallAvatarIcon, out characterImage, out characterMask);
			if (characterImage == null)
			{
				return null;
			}
			GameObject original = KampaiResources.Load<GameObject>("cmp_BuddyBarAvatar");
			GameObject gameObject = Object.Instantiate(original);
			BuddyAvatarView component = gameObject.GetComponent<BuddyAvatarView>();
			component.AvatarImage.sprite = characterImage;
			component.AvatarImage.maskSprite = characterMask;
			component.AvatarNameText.LocKey = definition.LocalizedKey;
			component.ProgressBarText.text = string.Format("{0}/{1}", prestige.CurrentPrestigePoints, prestige.NeededPrestigePoints);
			int index = 0;
			if (prestige.CurrentPrestigeLevel >= 0 && prestige.CurrentPrestigeLevel < definition.PrestigeLevelSettings.Count)
			{
				index = prestige.CurrentPrestigeLevel;
			}
			CharacterPrestigeLevelDefinition characterPrestigeLevelDefinition = definition.PrestigeLevelSettings[index];
			float x = (float)prestige.CurrentPrestigePoints / (float)characterPrestigeLevelDefinition.PointsNeeded;
			component.ProgressBarFill.anchorMax = new Vector2(x, 1f);
			return component;
		}
	}
}
