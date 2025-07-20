using System.Collections.Generic;
using UnityEngine;

namespace Kampai.Game
{
	public interface IPrestigeService
	{
		void Initialize();

		void AddPrestige(Prestige prestige);

		Prestige CreatePrestige(int prestigeDefinitionId);

		void RemovePrestige(Prestige prestige);

		int GetIdlePrestigeDuration(int prestigeDefinitionId);

		Prestige GetPrestige(int prestigeDefinitionId, bool logIfNonexistant = true);

		Prestige GetPrestigeFromMinionInstance(Character minionCharacter);

		void ChangeToPrestigeState(Prestige prestige, PrestigeState targetState, int targetPrestigeLevel = 0, bool triggerNewQuest = true);

		void GetCharacterImageBasedOnMood(int prestigeDefinitionId, CharacterImageType type, out Sprite characterImage, out Sprite characterMask);

		void GetCharacterImageBasedOnMood(PrestigeDefinition prestigeDefinition, CharacterImageType type, out Sprite characterImage, out Sprite characterMask);

		void GetCharacterImagePathBasedOnMood(int prestigeDefinitionId, CharacterImageType type, out string characterImagePath, out string characterMaskPath);

		void GetCharacterImagePathBasedOnMood(PrestigeDefinition prestigeDefinition, CharacterImageType type, out string characterImagePath, out string characterMaskPath);

		QuestResourceDefinition DetermineQuestResourceDefinition(int prestigeDefinitionId, CharacterImageType type);

		void PostOrderCompletion(Prestige prestige);

		IList<Prestige> GetBuddyPrestiges();

		Dictionary<int, bool> GetPrestigedCharacterStates(bool includeBob = true);

		int GetPrestigeUnlockedPrestigeLevel(PrestigeDefinition prestigeDefinition);

		void UpdateEligiblePrestigeList();

		Dictionary<int, Prestige> GetAllUnlockedPrestiges();

		void AddMinionToTikiBarSlot(Character targetMinion, int slotIndex, TikiBarBuilding tikiBar, bool enablePathing = false);

		int ResolveTrackedId(int questTrackedInstanceId);

		bool IsTikiBarFull();

		CabanaBuilding GetEmptyCabana();

		void CheckCompletedPrestiges();

		bool IsPrestigeFulfilled(Prestige prestige);
	}
}
