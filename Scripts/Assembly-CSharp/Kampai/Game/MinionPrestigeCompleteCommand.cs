using System.Collections;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class MinionPrestigeCompleteCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("MinionPrestigeCompleteCommand") as IKampaiLogger;

		private Character character;

		private TikiBarBuilding tikiBar;

		private int slotIndex;

		private int prestigeDefId;

		[Inject]
		public Prestige prestige { get; set; }

		[Inject(UIElement.CONTEXT)]
		public ICrossContextCapable uiContext { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IRandomService randomService { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public AddMinionToTikiBarSignal addMinionToTikiBarSignal { get; set; }

		[Inject]
		public UnlockCharacterModel unlockCharacterModel { get; set; }

		[Inject]
		public RemoveWayFinderSignal removeWayFinderSignal { get; set; }

		[Inject]
		public UnlockMinionsSignal unlockMinionSignal { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public DisplayPlayerTrainingSignal displaySignal { get; set; }

		[Inject]
		public PickControllerModel pickControllerModel { get; set; }

		[Inject]
		public CloseAllOtherMenuSignal closeAllMenuSignal { get; set; }

		public override void Execute()
		{
			prestigeDefId = prestige.Definition.ID;
			IList<Instance> instancesByDefinition = playerService.GetInstancesByDefinition<TikiBarBuildingDefinition>();
			if (instancesByDefinition != null && instancesByDefinition.Count != 0)
			{
				tikiBar = instancesByDefinition[0] as TikiBarBuilding;
				int openSlot = tikiBar.GetOpenSlot();
				if (prestigeDefId == 40004)
				{
					if (prestige.CurrentPrestigeLevel <= 0)
					{
						pickControllerModel.SetIgnoreInstance(313, true);
						CreateMinion();
					}
				}
				else if (openSlot != -1)
				{
					slotIndex = openSlot;
					if (prestige.CurrentPrestigeLevel > 0)
					{
						character = playerService.GetByInstanceId<Character>(prestige.trackedInstanceId);
						if (character is Minion || prestigeDefId == 40002 || prestigeDefId == 40003)
						{
							tikiBar.minionQueue[openSlot] = prestigeDefId;
							RemoveWayFinderIfBob(prestigeDefId);
							addMinionToTikiBarSignal.Dispatch(tikiBar, character, prestige, slotIndex);
						}
						else if (character is NamedCharacter)
						{
							prestigeService.ChangeToPrestigeState(prestige, PrestigeState.Questing);
						}
						uiContext.injectionBinder.GetInstance<ShowBuddyWelcomePanelUISignal>().Dispatch(new Boxed<Prestige>(prestige), CharacterWelcomeState.Welcome, prestige.CurrentPrestigeLevel);
					}
					else
					{
						unlockCharacterModel.routeIndex = slotIndex;
						tikiBar.minionQueue[slotIndex] = prestigeDefId;
						CreateMinion();
					}
				}
				else
				{
					HandleNoRoomAtTikiBar();
				}
			}
			telemetryService.Send_TelemetryCharacterPrestiged(prestige);
		}

		private void HandleNoRoomAtTikiBar()
		{
			int count = tikiBar.minionQueue.Count;
			if (this.prestige.CurrentPrestigeLevel > 0)
			{
				character = playerService.GetByInstanceId<Character>(this.prestige.trackedInstanceId);
				if (character is Minion || prestigeDefId == 40002)
				{
					tikiBar.minionQueue.Add(prestigeDefId);
				}
				else if (character is NamedCharacter)
				{
					prestigeService.ChangeToPrestigeState(this.prestige, PrestigeState.Questing);
				}
				return;
			}
			int index = count;
			for (int i = 3; i < count; i++)
			{
				Prestige prestige = prestigeService.GetPrestige(tikiBar.minionQueue[i]);
				if (prestige != null && prestige.CurrentPrestigeLevel > 0)
				{
					index = i;
					break;
				}
			}
			tikiBar.minionQueue.Insert(index, prestigeDefId);
		}

		private void RemoveWayFinderIfBob(int prestigeDefId)
		{
			if (prestigeDefId == 40002)
			{
				removeWayFinderSignal.Dispatch(character.ID);
			}
		}

		private void CreateMinion()
		{
			closeAllMenuSignal.Dispatch(null);
			PrestigeDefinition prestigeDefinition = definitionService.Get<PrestigeDefinition>(prestigeDefId);
			Definition definition = definitionService.Get<Definition>(prestigeDefinition.TrackedDefinitionID);
			NamedCharacterDefinition namedCharacterDefinition = definition as NamedCharacterDefinition;
			if (namedCharacterDefinition != null)
			{
				CreateNamedCharacter(namedCharacterDefinition);
			}
			CostumeItemDefinition costumeItemDefinition = definition as CostumeItemDefinition;
			if (costumeItemDefinition != null)
			{
				CreateCostumedMinion();
			}
			playerService.GetMinionPartyInstance().CharacterUnlocking = true;
		}

		private void CreateCostumedMinion()
		{
			IList<MinionDefinition> all = definitionService.GetAll<MinionDefinition>();
			int count = all.Count;
			int index = randomService.NextInt(count);
			MinionDefinition def = all[index];
			Minion minion = new Minion(def);
			int costumeDefinitionID = prestige.Definition.CostumeDefinitionID;
			playerService.Add(minion);
			prestige.trackedInstanceId = minion.ID;
			minion.PrestigeId = prestige.ID;
			character = minion;
			if (costumeDefinitionID == 99)
			{
				unlockCharacterModel.minionUnlocks.Add(minion);
			}
			else
			{
				unlockCharacterModel.characterUnlocks.Add(minion);
			}
			routineRunner.StartCoroutine(WaitAFrame());
		}

		private void CreateNamedCharacter(NamedCharacterDefinition namedCharacterDefinition)
		{
			NamedCharacter namedCharacter = CreateNamedCharacterBasedOnDefinition(namedCharacterDefinition);
			prestige.trackedInstanceId = namedCharacter.ID;
			character = namedCharacter;
			unlockCharacterModel.characterUnlocks.Add(namedCharacter);
			Signal<bool> signal = new Signal<bool>();
			signal.AddListener(delegate(bool wasShown)
			{
				if (!wasShown)
				{
					unlockMinionSignal.Dispatch();
				}
			});
			int playerTrainingReprestigeDefinitionId = prestige.Definition.PlayerTrainingReprestigeDefinitionId;
			if (prestige.CurrentPrestigeLevel >= 1 && playerTrainingReprestigeDefinitionId > 0)
			{
				displaySignal.Dispatch(prestige.Definition.PlayerTrainingReprestigeDefinitionId, false, signal);
			}
			else
			{
				displaySignal.Dispatch(prestige.Definition.PlayerTrainingPrestigeDefinitionId, false, signal);
			}
			prestigeService.ChangeToPrestigeState(prestige, PrestigeState.Questing, 0, false);
		}

		private NamedCharacter CreateNamedCharacterBasedOnDefinition(NamedCharacterDefinition namedCharacterDefinition)
		{
			NamedCharacter namedCharacter = null;
			PhilCharacterDefinition philCharacterDefinition = namedCharacterDefinition as PhilCharacterDefinition;
			if (philCharacterDefinition != null)
			{
				namedCharacter = new PhilCharacter(philCharacterDefinition);
			}
			BobCharacterDefinition bobCharacterDefinition = namedCharacterDefinition as BobCharacterDefinition;
			if (bobCharacterDefinition != null)
			{
				namedCharacter = new BobCharacter(bobCharacterDefinition);
			}
			KevinCharacterDefinition kevinCharacterDefinition = namedCharacterDefinition as KevinCharacterDefinition;
			if (kevinCharacterDefinition != null)
			{
				namedCharacter = new KevinCharacter(kevinCharacterDefinition);
			}
			StuartCharacterDefinition stuartCharacterDefinition = namedCharacterDefinition as StuartCharacterDefinition;
			if (stuartCharacterDefinition != null)
			{
				namedCharacter = new StuartCharacter(stuartCharacterDefinition);
			}
			List<NamedCharacter> instancesByType = playerService.GetInstancesByType<NamedCharacter>();
			foreach (NamedCharacter item in instancesByType)
			{
				if (item.Definition.ID == namedCharacter.Definition.ID)
				{
					logger.Error("You are trying to create a character that already exists! We are just gonna re-prestige the minion, sorry");
					prestigeService.ChangeToPrestigeState(prestige, PrestigeState.Questing);
					uiContext.injectionBinder.GetInstance<ShowBuddyWelcomePanelUISignal>().Dispatch(new Boxed<Prestige>(prestige), CharacterWelcomeState.Welcome, prestige.CurrentPrestigeLevel);
					return item;
				}
			}
			playerService.Add(namedCharacter);
			return namedCharacter;
		}

		private IEnumerator WaitAFrame()
		{
			yield return new WaitForEndOfFrame();
			prestigeService.ChangeToPrestigeState(prestige, PrestigeState.Questing, 0, false);
			unlockMinionSignal.Dispatch();
		}
	}
}
