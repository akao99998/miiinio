using System;
using System.Collections;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game.Mignette.View;
using Kampai.Game.Transaction;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class CreateTSMQuestTaskCommand : Command
	{
		private const int TSM_SORTEDLIST_PICK_LIMIT = 3;

		private const int DEFAULT_MIGNETTE_DELAY = 20;

		public IKampaiLogger logger = LogManager.GetClassLogger("CreateTSMQuestTaskCommand") as IKampaiLogger;

		[Inject]
		public int questGiverId { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IRandomService randomService { get; set; }

		[Inject]
		public UpdateQuestWorldIconsSignal UpdateQuestWorldIconsSignal { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public CreateTSMQuestTaskSignal CreateTSMQuestTaskSignal { get; set; }

		[Inject]
		public ITimeEventService TimeEventService { get; set; }

		[Inject]
		public ITimeService TimeService { get; set; }

		[Inject]
		public CreateNamedCharacterViewSignal createNamedCharacterViewSignal { get; set; }

		public TSMCharacter tsmCharacter { get; set; }

		public override void Execute()
		{
			tsmCharacter = playerService.GetByInstanceId<TSMCharacter>(questGiverId);
			if (tsmCharacter == null)
			{
				logger.Log(KampaiLogLevel.Error, "Unable to find TSM character in player inventory!");
				return;
			}
			if (MignetteManagerView.GetIsPlaying())
			{
				int cooldownMignetteDelayInSeconds = tsmCharacter.Definition.CooldownMignetteDelayInSeconds;
				TimeEventService.AddEvent(tsmCharacter.ID, TimeService.CurrentTime(), (cooldownMignetteDelayInSeconds <= 0) ? 20 : cooldownMignetteDelayInSeconds, CreateTSMQuestTaskSignal);
				logger.Log(KampaiLogLevel.Info, "TSM Creation delayed due to mignette");
				return;
			}
			int quantity = (int)playerService.GetQuantity(StaticItem.LEVEL_ID);
			TaskLevelBandDefinition taskLevelBandForLevel = definitionService.GetTaskLevelBandForLevel(quantity);
			DynamicQuestDefinition dynamicQuestDefinition = GenerateQuest(quantity, taskLevelBandForLevel);
			if (dynamicQuestDefinition != null)
			{
				Quest quest = new Quest(dynamicQuestDefinition);
				quest.Initialize();
				questService.AddQuest(quest);
				quest.Steps[0].TrackedID = quest.GetActiveDefinition().QuestSteps[0].ItemDefinitionID;
				quest.QuestIconTrackedInstanceId = questGiverId;
				logger.Info("New TSM quest id: {0} and trackedId: {1}", quest.ID, quest.Steps[0].TrackedID);
				routineRunner.StartCoroutine(WaitAFrame(quest));
			}
			else
			{
				logger.Log(KampaiLogLevel.Error, "Unable to create new task.");
			}
		}

		private DynamicQuestDefinition GenerateQuest(int level, TaskLevelBandDefinition definition)
		{
			if (tsmCharacter != null)
			{
				DynamicQuestDefinition dynamicQuestDefinition = null;
				int num = 4;
				while (dynamicQuestDefinition == null && num-- > 0)
				{
					QuestStepType type = PickQuestType(definition);
					dynamicQuestDefinition = BuildTask(level, type, definition);
				}
				if (dynamicQuestDefinition != null)
				{
					TransactionDefinition transactionDefinition = new TransactionDefinition();
					transactionDefinition.ID = int.MaxValue;
					int grindReward = GetGrindReward(dynamicQuestDefinition);
					int xpReward = GetXpReward(definition);
					transactionDefinition.Inputs = new List<QuantityItem>();
					transactionDefinition.Outputs = new List<QuantityItem>();
					if (grindReward > 0)
					{
						transactionDefinition.Outputs.Add(new QuantityItem(0, (uint)grindReward));
					}
					if (xpReward > 0)
					{
						transactionDefinition.Outputs.Add(new QuantityItem(2, (uint)xpReward));
					}
					dynamicQuestDefinition.RewardTransactionInstance = transactionDefinition.ToInstance();
					dynamicQuestDefinition.type = QuestType.DynamicQuest;
					dynamicQuestDefinition.DropStep = definition.DropOdds;
					return dynamicQuestDefinition;
				}
				logger.Error("Giving up trying to generate a task.");
			}
			else
			{
				logger.Error("TSM character can not be found");
			}
			return null;
		}

		private DynamicQuestDefinition BuildTask(int level, QuestStepType type, TaskLevelBandDefinition def)
		{
			QuestStepDefinition questStepDefinition = null;
			switch (type)
			{
			case QuestStepType.Delivery:
				questStepDefinition = GenerateGiveTask(def);
				break;
			case QuestStepType.MinionTask:
				questStepDefinition = GenerateGiveTask(def);
				break;
			case QuestStepType.OrderBoard:
				questStepDefinition = GenerateOrderBoardTask(level, def);
				break;
			default:
				logger.Error("Illegal task type {0}", type);
				break;
			}
			if (questStepDefinition != null)
			{
				DynamicQuestDefinition dynamicQuestDefinition = new DynamicQuestDefinition();
				dynamicQuestDefinition.ID = 77777;
				dynamicQuestDefinition.SurfaceType = QuestSurfaceType.ProcedurallyGenerated;
				dynamicQuestDefinition.LocalizedKey = QuestDefinition.GetProceduralQuestDescription(type);
				dynamicQuestDefinition.QuestSteps = new List<QuestStepDefinition> { questStepDefinition };
				dynamicQuestDefinition.QuestBookIcon = QuestDefinition.GetProceduralQuestIcon(type);
				dynamicQuestDefinition.SurfaceID = questStepDefinition.ItemDefinitionID;
				return dynamicQuestDefinition;
			}
			return null;
		}

		private IEnumerator WaitAFrame(Quest quest)
		{
			yield return null;
			if (!tsmCharacter.Created)
			{
				createNamedCharacterViewSignal.Dispatch(tsmCharacter);
				UpdateQuestWorldIconsSignal.Dispatch(quest);
			}
		}

		private QuestStepDefinition GenerateGiveTask(TaskLevelBandDefinition def)
		{
			Item item = PickIngredient();
			if (item != null)
			{
				int quantity = (int)item.Quantity;
				float num = PickBetween(def.GiveTaskMinMultiplier, def.GiveTaskMaxMultiplier);
				int num2 = (int)Math.Ceiling((float)quantity * num);
				if (num2 < def.GiveTaskMinQuantity && num2 > 0)
				{
					num2 = def.GiveTaskMinQuantity;
				}
				QuestStepDefinition questStepDefinition = new QuestStepDefinition();
				questStepDefinition.Type = QuestStepType.Delivery;
				questStepDefinition.ItemAmount = num2;
				questStepDefinition.ItemDefinitionID = item.Definition.ID;
				return questStepDefinition;
			}
			logger.Log(KampaiLogLevel.Warning, "Player has no ingredients to sell.");
			return null;
		}

		private QuestStepDefinition GenerateOrderBoardTask(int level, TaskLevelBandDefinition def)
		{
			int num = (int)Math.Floor((float)level * PickBetween(def.FillOrderTaskMinMultiplier, def.FillOrderTaskMaxMultiplier));
			if (num > 0)
			{
				QuestStepDefinition questStepDefinition = new QuestStepDefinition();
				questStepDefinition.Type = QuestStepType.OrderBoard;
				questStepDefinition.ItemAmount = num;
				return questStepDefinition;
			}
			logger.Error("Generated NO orders for level band {0}", def.MinLevel);
			return null;
		}

		private int GetXpReward(TaskLevelBandDefinition def)
		{
			return (int)Math.Floor(PickBetween(def.MinXpMultiplier, def.MaxXpMultiplier) * (float)def.XpReward);
		}

		private int GetGrindReward(DynamicQuestDefinition task)
		{
			Quest quest = new Quest(task);
			ItemDefinition itemDefinition = definitionService.Get<ItemDefinition>(quest.GetActiveDefinition().QuestSteps[0].ItemDefinitionID);
			int itemAmount = quest.GetActiveDefinition().QuestSteps[0].ItemAmount;
			return (int)((float)itemDefinition.BaseGrindCost * (itemDefinition.TSMRewardMultipler / 100f)) * itemAmount;
		}

		private Item PickIngredient()
		{
			List<Item> list = new List<Item>();
			list.AddRange(playerService.GetItemsByDefinition<IngredientsItemDefinition>());
			List<Item> list2 = new List<Item>();
			foreach (Item item in list)
			{
				if (item.Definition is DynamicIngredientsDefinition)
				{
					list2.Add(item);
				}
			}
			foreach (Item item2 in list2)
			{
				list.Remove(item2);
			}
			IList<Item> list3 = ItemUtil.SortItemsByQuantity(list, false);
			int num = ((list3.Count <= 3) ? list3.Count : 3);
			if (num > 0)
			{
				int index = (int)Math.Floor(randomService.NextFloat() * (float)num);
				return list3[index];
			}
			return null;
		}

		private float PickBetween(float min, float max)
		{
			float num = max - min;
			return min + num * randomService.NextFloat();
		}

		private QuestStepType PickQuestType(TaskLevelBandDefinition def)
		{
			WeightedInstance weightedInstance = playerService.GetWeightedInstance(def.PickWeightsId);
			if (weightedInstance == null)
			{
				logger.Fatal(FatalCode.DS_BAD_TASK_WEIGHT);
				return QuestStepType.Construction;
			}
			QuantityItem quantityItem = weightedInstance.NextPick(randomService);
			return (QuestStepType)quantityItem.ID;
		}
	}
}
