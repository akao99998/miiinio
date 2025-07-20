using System.Collections.Generic;

namespace Kampai.Game
{
	public class UnlockCharacterModel
	{
		public bool stuartFirstTimeHonor { get; set; }

		public int routeIndex { get; set; }

		public IList<Character> minionUnlocks { get; set; }

		public IList<Character> characterUnlocks { get; set; }

		public IList<QuestDialogSetting> dialogQueue { get; set; }

		public UnlockCharacterModel()
		{
			stuartFirstTimeHonor = false;
			routeIndex = -1;
			minionUnlocks = new List<Character>();
			characterUnlocks = new List<Character>();
			dialogQueue = new List<QuestDialogSetting>();
		}
	}
}
