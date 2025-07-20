using System.Collections.Generic;

namespace Kampai.Game
{
	public interface IGuestOfHonorService
	{
		GuestOfHonorDefinition CurrentGuestOfHonor { get; }

		Dictionary<int, bool> GetAllGOHStates();

		float GetCurrentBuffMultiplierForBuffType(BuffType buffType);

		int GetPartyCooldownForPrestige(int prestigeInstanceID);

		int GetRemainingInvitesForPrestige(int prestigeInstanceID);

		void UpdateAndStoreGuestOfHonorCooldowns();

		int GetRushCostForPartyCoolDown(int prestigeInstanceID);

		void SelectGuestOfHonor(PrestigeDefinition prestigeDefinition);

		void SelectGuestOfHonor(int prestigeDefinitionID);

		void SelectGuestOfHonor(PrestigeDefinition guest1PrestigeDefinition, PrestigeDefinition guest2PrestigeDefinition);

		void SelectGuestOfHonor(int guest1PrestigeDefinitionID, int guest2PrestigeDefinitionID);

		int GetBuffRemainingTime(MinionParty minionParty);

		void StartBuff(int buffBaseDurationFromMinionParty);

		void StopBuff(int timePassedSinceBuffStarts, int lastBuffStartTime);

		int GetCurrentBuffDuration();

		bool PartyShouldProduceBuff();

		float GetBuffMultiplierForPrestige(int prestigeDefinitionID);

		int GetBuffDurationForSingleGuestOfHonorOnNextLevel(GuestOfHonorDefinition gohDefinition);

		float GetCurrentBuffMultipler();

		BuffDefinition GetRecentBuffDefinition(bool useCurrent = false);

		void RushPartyCooldownForPrestige(int prestigeInstanceID);

		bool ShouldDisplayUnlockAtLevelText(uint unlockLevel, uint prestigeDefID);
	}
}
