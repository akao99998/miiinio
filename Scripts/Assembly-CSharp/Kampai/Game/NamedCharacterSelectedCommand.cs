using System.Collections.Generic;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class NamedCharacterSelectedCommand : Command
	{
		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public CharacterObject characterObject { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public PopupMessageSignal popupMessageSignal { get; set; }

		public override void Execute()
		{
			Character firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<Character>(characterObject.DefinitionID);
			if (firstInstanceByDefinitionId != null)
			{
				string text = TryGetQuestAspirationMessage(firstInstanceByDefinitionId);
				if (text != null)
				{
					popupMessageSignal.Dispatch(text, PopupMessageType.NORMAL);
				}
			}
		}

		private string TryGetQuestAspirationMessage(Character character)
		{
			string result = null;
			Prestige prestigeFromMinionInstance = prestigeService.GetPrestigeFromMinionInstance(character);
			if (prestigeFromMinionInstance == null || (!(character is SpecialEventCharacter) && (prestigeFromMinionInstance.state == PrestigeState.TaskableWhileQuesting || prestigeFromMinionInstance.state == PrestigeState.Questing)))
			{
				return null;
			}
			PrestigeDefinition definition = prestigeFromMinionInstance.Definition;
			int iD = definition.ID;
			if (IsExpiredEventMinion(iD))
			{
				return null;
			}
			if (questService.HasActiveQuest(iD))
			{
				return null;
			}
			int quantity = (int)playerService.GetQuantity(StaticItem.LEVEL_ID);
			int num = prestigeFromMinionInstance.CurrentPrestigeLevel;
			uint num2 = 0u;
			bool flag = definition.PrestigeLevelSettings != null;
			if (flag)
			{
				bool flag2 = num == definition.PrestigeLevelSettings.Count - 1;
				if (prestigeFromMinionInstance.state == PrestigeState.Taskable && flag2)
				{
					result = localService.GetString("AspirationalMessageCharacterLevelComplete", localService.GetString(prestigeFromMinionInstance.Definition.LocalizedKey));
				}
				else
				{
					if (prestigeFromMinionInstance.state != PrestigeState.Prestige && !flag2)
					{
						num++;
					}
					num2 = definition.PrestigeLevelSettings[num].UnlockLevel;
				}
			}
			else
			{
				num2 = GetNextQuestLevelFromQuestline(iD);
			}
			if (num2 != 0)
			{
				result = ((num2 > quantity) ? localService.GetString("AspirationalMessageCharacterLevelPre", localService.GetString(prestigeFromMinionInstance.Definition.LocalizedKey), num2.ToString()) : ((!flag) ? localService.GetString("AspirationalMessageSpecialEventCharacterLevelAvailable", localService.GetString(prestigeFromMinionInstance.Definition.LocalizedKey)) : localService.GetString("AspirationalMessageCharacterLevelAvailable", localService.GetString(prestigeFromMinionInstance.Definition.LocalizedKey))));
			}
			return result;
		}

		private bool IsExpiredEventMinion(int prestigeDefinitionID)
		{
			List<SpecialEventItem> instancesByType = playerService.GetInstancesByType<SpecialEventItem>();
			foreach (SpecialEventItem item in instancesByType)
			{
				if ((item.Definition as SpecialEventItemDefinition).PrestigeDefinitionID == prestigeDefinitionID && item.HasEnded)
				{
					return true;
				}
			}
			return false;
		}

		public uint GetNextQuestLevelFromQuestline(int prestigeDefinitionID)
		{
			uint result = 0u;
			Dictionary<int, QuestLine> questLines = questService.GetQuestLines();
			foreach (QuestLine value in questLines.Values)
			{
				if (value.Quests.Count == 0 || value.state == QuestLineState.Finished || value.state != 0 || GetSurfaceIDFromQuestLine(value) != prestigeDefinitionID)
				{
					continue;
				}
				foreach (QuestDefinition quest in value.Quests)
				{
					if (quest.ID == value.QuestLineID && quest.QuestVersion != -1)
					{
						result = (uint)quest.UnlockLevel;
						break;
					}
				}
				break;
			}
			return result;
		}

		private int GetSurfaceIDFromQuestLine(QuestLine questLine)
		{
			int result = -1;
			if (questLine != null && questLine.Quests != null)
			{
				for (int i = 0; i < questLine.Quests.Count; i++)
				{
					if (questLine.Quests[i].SurfaceID > 0)
					{
						result = questLine.Quests[i].SurfaceID;
						break;
					}
				}
			}
			return result;
		}
	}
}
