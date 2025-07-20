using System;

namespace Kampai.Game
{
	public static class AchievementType
	{
		public enum AchievementTypeIdentifier
		{
			UNKNOWN = 0,
			PLAYMIGNETTEXTIMES = 1
		}

		public static AchievementTypeIdentifier ParseIdentifier(string identifier)
		{
			if (!string.IsNullOrEmpty(identifier))
			{
				return (AchievementTypeIdentifier)(int)Enum.Parse(typeof(AchievementTypeIdentifier), identifier);
			}
			return AchievementTypeIdentifier.UNKNOWN;
		}
	}
}
