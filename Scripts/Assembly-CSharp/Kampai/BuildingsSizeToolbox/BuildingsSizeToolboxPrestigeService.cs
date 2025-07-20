using System;
using System.Collections.Generic;
using Kampai.Game;
using UnityEngine;

namespace Kampai.BuildingsSizeToolbox
{
	internal sealed class BuildingsSizeToolboxPrestigeService : IPrestigeService
	{
		public void Initialize()
		{
			throw new NotImplementedException();
		}

		public void AddPrestige(Prestige prestige)
		{
			throw new NotImplementedException();
		}

		public Prestige CreatePrestige(int prestigeDefinitionId)
		{
			throw new NotImplementedException();
		}

		public void RemovePrestige(Prestige prestige)
		{
			throw new NotImplementedException();
		}

		public int GetIdlePrestigeDuration(int prestigeDefinitionId)
		{
			throw new NotImplementedException();
		}

		public Prestige GetPrestige(int prestigeDefinitionId, bool logIfNonexistant = true)
		{
			return null;
		}

		public Prestige GetPrestigeFromMinionInstance(Character minionCharacter)
		{
			throw new NotImplementedException();
		}

		public void ChangeToPrestigeState(Prestige prestige, PrestigeState targetState, int targetPrestigeLevel = 0, bool triggerNewQuest = true)
		{
			throw new NotImplementedException();
		}

		public void GetCharacterImageBasedOnMood(int prestigeDefinitionId, CharacterImageType type, out Sprite characterImage, out Sprite characterMask)
		{
			throw new NotImplementedException();
		}

		public void GetCharacterImageBasedOnMood(PrestigeDefinition prestigeDefinition, CharacterImageType type, out Sprite characterImage, out Sprite characterMask)
		{
			throw new NotImplementedException();
		}

		public void GetCharacterImagePathBasedOnMood(int prestigeDefinitionId, CharacterImageType type, out string characterImagePath, out string characterMaskPath)
		{
			throw new NotImplementedException();
		}

		public void GetCharacterImagePathBasedOnMood(PrestigeDefinition prestigeDefinition, CharacterImageType type, out string characterImagePath, out string characterMaskPath)
		{
			throw new NotImplementedException();
		}

		public QuestResourceDefinition DetermineQuestResourceDefinition(int prestigeDefinitionId, CharacterImageType type)
		{
			throw new NotImplementedException();
		}

		public void PostOrderCompletion(Prestige prestige)
		{
			throw new NotImplementedException();
		}

		public IList<Prestige> GetBuddyPrestiges()
		{
			throw new NotImplementedException();
		}

		public Dictionary<int, bool> GetPrestigedCharacterStates(bool includeBob = true)
		{
			throw new NotImplementedException();
		}

		public int GetPrestigeUnlockedPrestigeLevel(PrestigeDefinition prestigeDefinition)
		{
			throw new NotImplementedException();
		}

		public void UpdateEligiblePrestigeList()
		{
			throw new NotImplementedException();
		}

		public Dictionary<int, Prestige> GetAllUnlockedPrestiges()
		{
			throw new NotImplementedException();
		}

		public void AddMinionToTikiBarSlot(Character targetMinion, int slotIndex, TikiBarBuilding tikiBar, bool enablePathing = false)
		{
			throw new NotImplementedException();
		}

		public int ResolveTrackedId(int questTrackedInstanceId)
		{
			throw new NotImplementedException();
		}

		public bool IsTikiBarFull()
		{
			throw new NotImplementedException();
		}

		public CabanaBuilding GetEmptyCabana()
		{
			throw new NotImplementedException();
		}

		public void CheckCompletedPrestiges()
		{
			throw new NotImplementedException();
		}

		public bool IsPrestigeFulfilled(Prestige prestige)
		{
			throw new NotImplementedException();
		}
	}
}
