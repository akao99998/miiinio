using System.Collections;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Main;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class SetupNamedCharactersCommand : Command
	{
		private ICollection<NamedCharacter> namedCharacters;

		public IKampaiLogger logger = LogManager.GetClassLogger("SetupNamedCharactersCommand") as IKampaiLogger;

		[Inject]
		public IPlayerService player { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		[Inject]
		public MinionPrestigeCompleteSignal minionPrestigeCompleteSignal { get; set; }

		[Inject]
		public AppStartCompleteSignal appStartCompleteSignal { get; set; }

		[Inject]
		public RandomFlyOverCompleteSignal randomFlyOverCompleteSignal { get; set; }

		[Inject]
		public IMasterPlanService masterPlanService { get; set; }

		[Inject]
		public RestoreNamedCharacterSignal restoreNamedCharacterSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public DisplayMasterPlanCooldownAlertSignal displayMasterPlanCooldownAlertSignal { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public CreateVillainViewSignal createViewSignal { get; set; }

		[Inject]
		public VillainGotoCabanaSignal gotoCabanaSignal { get; set; }

		public override void Execute()
		{
			TimeProfiler.StartSection("named characters");
			namedCharacters = player.GetInstancesByType<NamedCharacter>();
			foreach (NamedCharacter namedCharacter in namedCharacters)
			{
				NamedCharacterDefinition definition = namedCharacter.Definition;
				if (definition.Type == NamedCharacterType.SPECIAL_EVENT || definition.ID == 70004)
				{
					continue;
				}
				Prestige prestige = GetPrestige(namedCharacter);
				if (prestige == null)
				{
					continue;
				}
				if (definition.Type == NamedCharacterType.VILLAIN)
				{
					if (prestige.state == PrestigeState.Questing || prestige.state == PrestigeState.Taskable || prestige.CurrentPrestigeLevel > 0)
					{
						routineRunner.StartCoroutine(MoveVillainAfterFrame(namedCharacter));
					}
				}
				else
				{
					restoreNamedCharacterSignal.Dispatch(namedCharacter, prestige);
				}
			}
			OrderBoard firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<OrderBoard>(3022);
			if (firstInstanceByDefinitionId != null && firstInstanceByDefinitionId.HarvestableCharacterDefinitionId != 0)
			{
				Prestige prestige2 = prestigeService.GetPrestige(firstInstanceByDefinitionId.HarvestableCharacterDefinitionId);
				prestigeService.PostOrderCompletion(prestige2);
			}
			TimeProfiler.EndSection("named characters");
			prestigeService.CheckCompletedPrestiges();
			CreateCharacters();
		}

		private IEnumerator MoveVillainAfterFrame(NamedCharacter villain)
		{
			yield return null;
			int cabanaDefinitionId = -1;
			switch (villain.Definition.ID)
			{
			case 70005:
				cabanaDefinitionId = 3042;
				break;
			case 70006:
				cabanaDefinitionId = 3043;
				break;
			case 70007:
				cabanaDefinitionId = 3044;
				break;
			}
			Building cabana = playerService.GetFirstInstanceByDefinitionId<Building>(cabanaDefinitionId);
			createViewSignal.Dispatch(villain.ID);
			gotoCabanaSignal.Dispatch(villain.ID, cabana.ID);
		}

		private Prestige GetPrestige(NamedCharacter namedCharacter)
		{
			Prestige prestigeFromMinionInstance = prestigeService.GetPrestigeFromMinionInstance(namedCharacter);
			if (prestigeFromMinionInstance == null && !FixPrestige(namedCharacter))
			{
				logger.Fatal(FatalCode.CMD_RESTORE_NAMED_CHARACTER_NO_PRESTIGE);
			}
			return prestigeFromMinionInstance;
		}

		private bool FixPrestige(NamedCharacter affectedCharacter)
		{
			int iD = affectedCharacter.Definition.ID;
			foreach (NamedCharacter namedCharacter in namedCharacters)
			{
				if (namedCharacter == affectedCharacter || namedCharacter.Definition.ID != iD)
				{
					continue;
				}
				player.Remove(affectedCharacter);
				return true;
			}
			return false;
		}

		private void CreateCharacters()
		{
			IList<Instance> instancesByDefinition = playerService.GetInstancesByDefinition<TikiBarBuildingDefinition>();
			if (instancesByDefinition == null || instancesByDefinition.Count == 0)
			{
				return;
			}
			TikiBarBuilding tikiBar = instancesByDefinition[0] as TikiBarBuilding;
			if (tikiBar.minionQueue.Count <= 3 || tikiBar.GetOpenSlot() == -1)
			{
				return;
			}
			Prestige missingCharacter = null;
			int prestigeDefinitionId = -1;
			for (int i = 0; i < tikiBar.minionQueue.Count; i++)
			{
				prestigeDefinitionId = tikiBar.minionQueue[i];
				if (prestigeDefinitionId != -1)
				{
					Prestige prestige = prestigeService.GetPrestige(prestigeDefinitionId);
					if (prestige.trackedInstanceId == 0)
					{
						missingCharacter = prestige;
						break;
					}
				}
			}
			if (missingCharacter != null)
			{
				Signal signal = appStartCompleteSignal;
				if (playerService.GetHighestFtueCompleted() == 999999)
				{
					signal = randomFlyOverCompleteSignal;
				}
				signal.AddOnce(delegate
				{
					tikiBar.minionQueue.Remove(prestigeDefinitionId);
					minionPrestigeCompleteSignal.Dispatch(missingCharacter);
					CheckMasterPlanCooldown();
				});
			}
		}

		private void CheckMasterPlanCooldown()
		{
			MasterPlan currentMasterPlan = masterPlanService.CurrentMasterPlan;
			if (currentMasterPlan != null && currentMasterPlan.displayCooldownAlert)
			{
				displayMasterPlanCooldownAlertSignal.Dispatch(currentMasterPlan);
			}
		}
	}
}
