using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.injector.api;

namespace Kampai.Game
{
	public class GuestOfHonorService : IGuestOfHonorService
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("GuestOfHonorService") as IKampaiLogger;

		private GuestOfHonorDefinition lastGuestOfHonor;

		private GuestOfHonorDefinition currentGuestOfHonor;

		private int currentBuffDuration;

		private bool haveUnlockedAPrestige;

		private bool firstMasterPlanComplete;

		private List<SpecialEventItemDefinition> specialEvents;

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public IPartyService partyService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public IMasterPlanService masterPlanService { get; set; }

		[Inject]
		public EndPartyBuffTimerSignal endPartyBuffTimerSignal { get; set; }

		public GuestOfHonorDefinition CurrentGuestOfHonor
		{
			get
			{
				return currentGuestOfHonor;
			}
		}

		public Dictionary<int, bool> GetAllGOHStates()
		{
			Dictionary<int, bool> dictionary = new Dictionary<int, bool>();
			bool flag = false;
			IList<PrestigeDefinition> all = definitionService.GetAll<PrestigeDefinition>();
			foreach (PrestigeDefinition item in all)
			{
				int iD = item.ID;
				if (item.GuestOfHonorDefinitionID == 0 || item.GuestOfHonorDisplayableType == GOHDisplayableType.Never)
				{
					continue;
				}
				Prestige firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<Prestige>(iD);
				bool flag2 = CharacterShouldBeDisplayed(item, firstInstanceByDefinitionId);
				if (firstInstanceByDefinitionId == null)
				{
					if (flag2)
					{
						dictionary.Add(iD, flag);
					}
				}
				else if (flag2)
				{
					if (firstInstanceByDefinitionId.CurrentPrestigeLevel > 0 || firstInstanceByDefinitionId.state == PrestigeState.Questing || firstInstanceByDefinitionId.state == PrestigeState.Taskable || firstInstanceByDefinitionId.state == PrestigeState.TaskableWhileQuesting)
					{
						dictionary.Add(iD, !flag);
					}
					else
					{
						dictionary.Add(iD, flag);
					}
				}
			}
			return dictionary;
		}

		private bool CharacterShouldBeDisplayed(PrestigeDefinition def, Prestige prestige)
		{
			if (specialEvents == null)
			{
				specialEvents = definitionService.GetAll<SpecialEventItemDefinition>();
			}
			foreach (SpecialEventItemDefinition specialEvent in specialEvents)
			{
				if (specialEvent.PrestigeDefinitionID == def.ID)
				{
					SpecialEventItem firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<SpecialEventItem>(specialEvent.ID);
					return firstInstanceByDefinitionId != null && firstInstanceByDefinitionId.HasEnded;
				}
			}
			if (def.GuestOfHonorDisplayableType != GOHDisplayableType.SpecialConditionOnly)
			{
				return true;
			}
			if (prestige == null)
			{
				return false;
			}
			if (prestige.state == PrestigeState.Locked)
			{
				return false;
			}
			if (firstMasterPlanComplete)
			{
				return true;
			}
			MasterPlanDefinition planDefinition = definitionService.Get<MasterPlanDefinition>(65000);
			if (masterPlanService.HasReceivedInitialRewardFromPlanDefinition(planDefinition))
			{
				firstMasterPlanComplete = true;
				return true;
			}
			return false;
		}

		public float GetCurrentBuffMultiplierForBuffType(BuffType buffType)
		{
			if (currentGuestOfHonor == null || !playerService.GetMinionPartyInstance().IsBuffHappening)
			{
				return 1f;
			}
			if (currentGuestOfHonor.buffDefinitionIDs == null)
			{
				logger.Fatal(FatalCode.BS_BAD_GUEST_OF_HONOR_DEFINITION, currentGuestOfHonor.ID);
				return 1f;
			}
			bool flag = false;
			int index = 0;
			BuffDefinition buffDefinition = null;
			for (int i = 0; i < currentGuestOfHonor.buffDefinitionIDs.Count; i++)
			{
				buffDefinition = definitionService.Get<BuffDefinition>(currentGuestOfHonor.buffDefinitionIDs[i]);
				if (buffDefinition.buffType == buffType)
				{
					index = i;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return 1f;
			}
			int value = currentGuestOfHonor.buffStarValues[index];
			value = Mathf.Clamp(value, 0, buffDefinition.starMultiplierValue.Count);
			return buffDefinition.starMultiplierValue[value];
		}

		public int GetPartyCooldownForPrestige(int prestigeInstanceID)
		{
			Prestige byInstanceId = playerService.GetByInstanceId<Prestige>(prestigeInstanceID);
			if (!byInstanceId.onCooldown)
			{
				return 0;
			}
			PrestigeDefinition prestigeDefinition = definitionService.Get<PrestigeDefinition>(byInstanceId.Definition.ID);
			GuestOfHonorDefinition guestOfHonorDefinition = definitionService.Get<GuestOfHonorDefinition>(prestigeDefinition.GuestOfHonorDefinitionID);
			return guestOfHonorDefinition.cooldown - byInstanceId.numPartiesThrown;
		}

		public int GetRemainingInvitesForPrestige(int prestigeInstanceID)
		{
			Prestige byInstanceId = playerService.GetByInstanceId<Prestige>(prestigeInstanceID);
			if (byInstanceId.onCooldown)
			{
				return 0;
			}
			PrestigeDefinition prestigeDefinition = definitionService.Get<PrestigeDefinition>(byInstanceId.Definition.ID);
			GuestOfHonorDefinition guestOfHonorDefinition = definitionService.Get<GuestOfHonorDefinition>(prestigeDefinition.GuestOfHonorDefinitionID);
			return guestOfHonorDefinition.availableInvites - byInstanceId.numPartiesInvited;
		}

		public int GetCurrentBuffDuration()
		{
			MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
			PartyMeterTierDefinition partyMeterTierDefinition = minionPartyInstance.Definition.partyMeterDefinition.Tiers[minionPartyInstance.PartyStartTier];
			int duration = partyMeterTierDefinition.Duration;
			int num = 0;
			int num2 = 0;
			foreach (int lastGuestsOfHonorPrestigeID in minionPartyInstance.lastGuestsOfHonorPrestigeIDs)
			{
				if (lastGuestsOfHonorPrestigeID != 0)
				{
					PrestigeDefinition prestigeDefinition = definitionService.Get<PrestigeDefinition>(lastGuestsOfHonorPrestigeID);
					GuestOfHonorDefinition guestOfHonorDefinition = definitionService.Get<GuestOfHonorDefinition>(prestigeDefinition.GuestOfHonorDefinitionID);
					num = duration + guestOfHonorDefinition.partyDurationBoost * 60;
					num2 += (int)((float)num * guestOfHonorDefinition.partyDurationMultipler - (float)duration);
				}
			}
			return RoundToMinute(duration + num2);
		}

		private int RoundToMinute(int seconds)
		{
			int num = seconds % 60;
			return (num >= 30) ? (seconds + 60 - num) : (seconds - num);
		}

		public void SelectGuestOfHonor(int prestigeDefinitionID)
		{
			if (prestigeDefinitionID == 0)
			{
				SelectGuestOfHonor(definitionService.Get<GuestOfHonorDefinition>(8200));
				return;
			}
			PrestigeDefinition prestigeDefinition = definitionService.Get<PrestigeDefinition>(prestigeDefinitionID);
			SelectGuestOfHonor(prestigeDefinition);
		}

		public void SelectGuestOfHonor(int guest1PrestigeDefinitionID, int guest2PrestigeDefinitionID)
		{
			PrestigeDefinition guest1PrestigeDefinition = definitionService.Get<PrestigeDefinition>(guest1PrestigeDefinitionID);
			PrestigeDefinition guest2PrestigeDefinition = definitionService.Get<PrestigeDefinition>(guest2PrestigeDefinitionID);
			SelectGuestOfHonor(guest1PrestigeDefinition, guest2PrestigeDefinition);
		}

		public void SelectGuestOfHonor(PrestigeDefinition prestigeDefinition)
		{
			currentGuestOfHonor = definitionService.Get<GuestOfHonorDefinition>(prestigeDefinition.GuestOfHonorDefinitionID);
			MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
			minionPartyInstance.lastGuestsOfHonorPrestigeIDs.Clear();
			minionPartyInstance.lastGuestsOfHonorPrestigeIDs.Add(prestigeDefinition.ID);
		}

		public void SelectGuestOfHonor(PrestigeDefinition guest1PrestigeDefinition, PrestigeDefinition guest2PrestigeDefinition)
		{
			currentGuestOfHonor = definitionService.Get<GuestOfHonorDefinition>(guest1PrestigeDefinition.GuestOfHonorDefinitionID);
			currentGuestOfHonor = definitionService.Get<GuestOfHonorDefinition>(guest2PrestigeDefinition.GuestOfHonorDefinitionID);
			MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
			minionPartyInstance.lastGuestsOfHonorPrestigeIDs.Clear();
			minionPartyInstance.lastGuestsOfHonorPrestigeIDs.Add(guest1PrestigeDefinition.ID);
			minionPartyInstance.lastGuestsOfHonorPrestigeIDs.Add(guest2PrestigeDefinition.ID);
		}

		private void SelectGuestOfHonor(GuestOfHonorDefinition guestDefinition)
		{
			currentGuestOfHonor = guestDefinition;
			MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
			minionPartyInstance.lastGuestsOfHonorPrestigeIDs.Clear();
			minionPartyInstance.lastGuestsOfHonorPrestigeIDs.Add(0);
		}

		public int GetBuffRemainingTime(MinionParty minionParty)
		{
			return minionParty.BuffStartTime + currentBuffDuration - timeService.CurrentTime();
		}

		public void UpdateAndStoreGuestOfHonorCooldowns()
		{
			MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
			foreach (Prestige item in playerService.GetInstancesByType<Prestige>())
			{
				if (minionPartyInstance.lastGuestsOfHonorPrestigeIDs.Contains(item.Definition.ID))
				{
					GuestOfHonorDefinition guestOfHonorDefinition = definitionService.Get<GuestOfHonorDefinition>(item.Definition.GuestOfHonorDefinitionID);
					if (guestOfHonorDefinition.cooldown > 0)
					{
						item.numPartiesInvited++;
						if (item.numPartiesInvited >= guestOfHonorDefinition.availableInvites)
						{
							item.onCooldown = true;
						}
					}
				}
				else if (item.onCooldown)
				{
					item.numPartiesThrown++;
					if (GetPartyCooldownForPrestige(item.ID) <= 0)
					{
						item.onCooldown = false;
						item.numPartiesThrown = 0;
						item.numPartiesInvited = 0;
					}
				}
			}
		}

		public int GetRushCostForPartyCoolDown(int prestigeInstanceID)
		{
			Prestige byInstanceId = playerService.GetByInstanceId<Prestige>(prestigeInstanceID);
			GuestOfHonorDefinition guestOfHonorDefinition = definitionService.Get<GuestOfHonorDefinition>(byInstanceId.Definition.GuestOfHonorDefinitionID);
			return (guestOfHonorDefinition.cooldown - byInstanceId.numPartiesThrown) * guestOfHonorDefinition.rushCostPerParty;
		}

		public void StartBuff(int buffBaseDurationFromMinionParty)
		{
			if (currentGuestOfHonor == null)
			{
				logger.Error("You are trying to start a buff without a guest of honor selected!");
				return;
			}
			int count = currentGuestOfHonor.buffDefinitionIDs.Count;
			int num = buffBaseDurationFromMinionParty + currentGuestOfHonor.partyDurationBoost * 60;
			currentBuffDuration = RoundToMinute((int)((float)num * currentGuestOfHonor.partyDurationMultipler));
			int buffStartTime = playerService.GetMinionPartyInstance().BuffStartTime;
			lastGuestOfHonor = currentGuestOfHonor;
			timeEventService.AddEvent(80000, buffStartTime, currentBuffDuration, endPartyBuffTimerSignal);
			ICrossContextInjectionBinder injectionBinder = gameContext.injectionBinder;
			for (int i = 0; i < count; i++)
			{
				int id = currentGuestOfHonor.buffDefinitionIDs[i];
				BuffDefinition buffDefinition = definitionService.Get<BuffDefinition>(id);
				int index = currentGuestOfHonor.buffStarValues[i];
				float type = buffDefinition.starMultiplierValue[index];
				switch (buffDefinition.buffType)
				{
				case BuffType.CURRENCY:
					injectionBinder.GetInstance<StartCurrencyBuffSignal>().Dispatch();
					break;
				case BuffType.PARTY:
					injectionBinder.GetInstance<StartPartyPointBuffSignal>().Dispatch(type, buffStartTime);
					break;
				case BuffType.PRODUCTION:
					injectionBinder.GetInstance<StartProductionBuffSignal>().Dispatch(type, buffStartTime);
					break;
				}
			}
		}

		public void StopBuff(int timePassedSinceBuffStarts, int lastBuffStartTime)
		{
			if (lastGuestOfHonor == null)
			{
				return;
			}
			int count = lastGuestOfHonor.buffDefinitionIDs.Count;
			for (int i = 0; i < count; i++)
			{
				int id = lastGuestOfHonor.buffDefinitionIDs[i];
				BuffDefinition buffDefinition = definitionService.Get<BuffDefinition>(id);
				int index = currentGuestOfHonor.buffStarValues[i];
				float third = buffDefinition.starMultiplierValue[index];
				ICrossContextInjectionBinder injectionBinder = gameContext.injectionBinder;
				switch (buffDefinition.buffType)
				{
				case BuffType.CURRENCY:
					injectionBinder.GetInstance<StopCurrencyBuffSignal>().Dispatch();
					break;
				case BuffType.PARTY:
					injectionBinder.GetInstance<StopPartyPointBuffSignal>().Dispatch(new Tuple<int, int, float>(lastBuffStartTime, timePassedSinceBuffStarts, third));
					break;
				case BuffType.PRODUCTION:
					injectionBinder.GetInstance<StopProductionBuffSignal>().Dispatch(new Tuple<int, int, float>(lastBuffStartTime, timePassedSinceBuffStarts, third));
					break;
				}
			}
			currentBuffDuration = 0;
			lastGuestOfHonor = null;
		}

		public bool PartyShouldProduceBuff()
		{
			if (haveUnlockedAPrestige)
			{
				return true;
			}
			StuartCharacter firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<StuartCharacter>(70001);
			if (firstInstanceByDefinitionId != null)
			{
				haveUnlockedAPrestige = true;
			}
			return haveUnlockedAPrestige;
		}

		public float GetBuffMultiplierForPrestige(int prestigeDefinitionID)
		{
			PrestigeDefinition prestigeDefinition = definitionService.Get<PrestigeDefinition>(prestigeDefinitionID);
			GuestOfHonorDefinition guestOfHonorDefinition = definitionService.Get<GuestOfHonorDefinition>(prestigeDefinition.GuestOfHonorDefinitionID);
			return GetBuffMultiplerFromGuestOfHonor(guestOfHonorDefinition);
		}

		public int GetBuffDurationForSingleGuestOfHonorOnNextLevel(GuestOfHonorDefinition gohDefinition)
		{
			MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
			int currentPartyIndex = minionPartyInstance.CurrentPartyIndex;
			int quantity = (int)playerService.GetQuantity(StaticItem.LEVEL_ID);
			int currentLevel = ((!partyService.IsInspirationParty(quantity, currentPartyIndex)) ? quantity : (quantity + 1));
			int index = minionPartyInstance.DeterminePartyTier((uint)currentLevel);
			PartyMeterTierDefinition partyMeterTierDefinition = minionPartyInstance.Definition.partyMeterDefinition.Tiers[index];
			int duration = partyMeterTierDefinition.Duration;
			int num = gohDefinition.partyDurationBoost * 60;
			int seconds = (int)((float)duration * gohDefinition.partyDurationMultipler + (float)num);
			return RoundToMinute(seconds);
		}

		public float GetCurrentBuffMultipler()
		{
			return GetBuffMultiplerFromGuestOfHonor(lastGuestOfHonor);
		}

		public BuffDefinition GetRecentBuffDefinition(bool useCurrent = false)
		{
			GuestOfHonorDefinition guestOfHonorDefinition = lastGuestOfHonor;
			if (useCurrent)
			{
				guestOfHonorDefinition = currentGuestOfHonor;
			}
			if (guestOfHonorDefinition == null || guestOfHonorDefinition.buffDefinitionIDs == null || guestOfHonorDefinition.buffDefinitionIDs.Count == 0)
			{
				return null;
			}
			return definitionService.Get<BuffDefinition>(guestOfHonorDefinition.buffDefinitionIDs[0]);
		}

		private float GetBuffMultiplerFromGuestOfHonor(GuestOfHonorDefinition guestOfHonorDefinition)
		{
			if (guestOfHonorDefinition == null || guestOfHonorDefinition.buffDefinitionIDs == null || guestOfHonorDefinition.buffDefinitionIDs.Count == 0)
			{
				return 1f;
			}
			BuffDefinition buffDefinition = definitionService.Get<BuffDefinition>(guestOfHonorDefinition.buffDefinitionIDs[0]);
			int index = guestOfHonorDefinition.buffStarValues[0];
			return buffDefinition.starMultiplierValue[index];
		}

		public void RushPartyCooldownForPrestige(int prestigeInstanceID)
		{
			Prestige byInstanceId = playerService.GetByInstanceId<Prestige>(prestigeInstanceID);
			byInstanceId.onCooldown = false;
			byInstanceId.numPartiesInvited = 0;
			byInstanceId.numPartiesThrown = 0;
		}

		public bool ShouldDisplayUnlockAtLevelText(uint unlockLevel, uint prestigeDefID)
		{
			int quantity = (int)playerService.GetQuantity(StaticItem.LEVEL_ID);
			MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
			int currentPartyIndex = minionPartyInstance.CurrentPartyIndex;
			int num = ((!partyService.IsInspirationParty(quantity, currentPartyIndex)) ? quantity : (quantity + 1));
			if (num < (int)unlockLevel)
			{
				return true;
			}
			if (prestigeDefID == 40004)
			{
				return true;
			}
			return false;
		}
	}
}
