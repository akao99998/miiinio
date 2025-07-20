using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game.View;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game.Trigger
{
	public class CheckTriggersCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("CheckTriggersCommand") as IKampaiLogger;

		private IPlayerService playerService;

		private TSMCharacter tsmCharacter;

		[Inject]
		public int questGiverId { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ITriggerService triggerService { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public CreateTSMQuestTaskSignal createTSMQuestTaskSignal { get; set; }

		[Inject]
		public HideTSMCharacterSignal hideTSMCharacterSignal { get; set; }

		[Inject]
		public TriggerFiredSignal triggerFiredSignal { get; set; }

		[Inject]
		public RemoveQuestWorldIconSignal removeQuestWorldIconSignal { get; set; }

		public override void Execute()
		{
			playerService = gameContext.injectionBinder.GetInstance<IPlayerService>();
			if (playerService == null || definitionService == null || logger == null)
			{
				return;
			}
			tsmCharacter = playerService.GetByInstanceId<TSMCharacter>(questGiverId);
			if (tsmCharacter == null)
			{
				logger.Error("Unable to find TSM character in player inventory!");
			}
			else
			{
				if (playerService.GetQuantity(StaticItem.LEVEL_ID) < 3)
				{
					return;
				}
				triggerService.RemoveOldTriggers();
				TriggerInstance activeTrigger = triggerService.ActiveTrigger;
				if (activeTrigger != null)
				{
					TriggerDefinition definition = activeTrigger.Definition;
					if (definition.ForceOverride)
					{
						logger.Info("Trigger fired! {0}", definition);
						triggerFiredSignal.Dispatch(activeTrigger);
						return;
					}
					if (TryToFireTrigger(true))
					{
						return;
					}
				}
				TryToFireTrigger(false);
			}
		}

		private bool TryToFireTrigger(bool activeTriggerExists)
		{
			bool result = false;
			IList<TriggerDefinition> triggerDefinitions = definitionService.GetTriggerDefinitions();
			TriggerDefinition triggerDefinition = CheckForFiredTrigger(triggerDefinitions);
			if (triggerDefinition == null && !activeTriggerExists)
			{
				logger.Info("No Trigger found, using the TSM Level Bands");
				UseLevelBands();
				return result;
			}
			if (triggerDefinition != null)
			{
				if (activeTriggerExists && triggerDefinition.ForceOverride)
				{
					ClearTriggerData();
				}
				else if (!activeTriggerExists)
				{
					if (tsmCharacter.Created)
					{
						ClearQuestData();
						return true;
					}
					TriggerInstance type = triggerService.AddActiveTrigger(triggerDefinition);
					logger.Info("Trigger fired! {0}", triggerDefinition);
					triggerFiredSignal.Dispatch(type);
				}
				else if (activeTriggerExists && !tsmCharacter.Created)
				{
					triggerFiredSignal.Dispatch(triggerService.ActiveTrigger);
				}
				result = true;
			}
			return result;
		}

		public TriggerDefinition CheckForFiredTrigger(IList<TriggerDefinition> triggerDefs)
		{
			if (triggerDefs == null || triggerDefs.Count == 0)
			{
				return null;
			}
			TriggerDefinition triggerDefinition = null;
			for (int i = 0; i < triggerDefs.Count; i++)
			{
				triggerDefinition = triggerDefs[i];
				if (triggerDefinition == null || triggerDefinition.type != TriggerDefinitionType.Identifier.TSM || !triggerDefinition.IsTriggered(gameContext))
				{
					triggerDefinition = null;
					continue;
				}
				break;
			}
			return triggerDefinition;
		}

		public void UseLevelBands()
		{
			createTSMQuestTaskSignal.Dispatch(questGiverId);
		}

		public void ClearQuestData()
		{
			IQuestController questControllerByDefinitionID = questService.GetQuestControllerByDefinitionID(77777);
			if (questControllerByDefinitionID != null && questControllerByDefinitionID.Quest != null)
			{
				removeQuestWorldIconSignal.Dispatch(questControllerByDefinitionID.Quest);
				questService.RemoveQuest(questControllerByDefinitionID);
			}
			hideTSMCharacterSignal.Dispatch(TSMCharacterHideState.CelebrateAndReturn);
			tsmCharacter.Created = false;
		}

		public void ClearTriggerData()
		{
			triggerService.ResetCurrentTrigger();
			hideTSMCharacterSignal.Dispatch(TSMCharacterHideState.CelebrateAndReturn);
			tsmCharacter.Created = false;
		}
	}
}
