using System.Collections.Generic;

namespace Kampai.Game
{
	public class QuestLine
	{
		public virtual int QuestLineID
		{
			get
			{
				if (Quests.Count > 0)
				{
					return Quests[0].QuestLineID;
				}
				return -1;
			}
		}

		public IList<QuestDefinition> Quests { get; set; }

		public virtual QuestLineState state { get; set; }

		public int unlockByQuestLine { get; set; }

		public int UnlockCharacterPrestigeLevel { get; set; }

		public virtual int GivenByCharacterID { get; set; }

		public int GivenByCharacterPrestigeLevel { get; set; }
	}
}
