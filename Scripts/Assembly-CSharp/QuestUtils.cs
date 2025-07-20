using System.Collections.Generic;
using Kampai.Game;

public static class QuestUtils
{
	public static List<Quest> ResolveQuests(List<Quest> quests)
	{
		List<Quest> list = new List<Quest>();
		if (quests == null)
		{
			return list;
		}
		quests.Sort();
		int num = -1;
		int num2 = -1;
		foreach (Quest quest in quests)
		{
			if (quest == null)
			{
				continue;
			}
			QuestDefinition activeDefinition = quest.GetActiveDefinition();
			if (activeDefinition != null)
			{
				if (activeDefinition.SurfaceID != num)
				{
					list.Add(quest);
					num = activeDefinition.SurfaceID;
					num2 = activeDefinition.QuestPriority;
				}
				else if (activeDefinition.QuestPriority >= num2)
				{
					list.Add(quest);
					num2 = activeDefinition.QuestPriority;
				}
			}
		}
		return list;
	}
}
