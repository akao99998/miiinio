using System.Collections.Generic;
using Kampai.Game.Transaction;
using Kampai.Util;

namespace Kampai.Game
{
	public static class MasterPlanQuestType
	{
		public sealed class PlanDefinition : QuestLine
		{
			public IList<MasterPlanComponent> components;

			public MasterPlan plan;

			public MasterPlanComponent component;

			public override QuestLineState state
			{
				get
				{
					if (component == null)
					{
						for (int i = 0; i < components.Count; i++)
						{
							MasterPlanComponent masterPlanComponent = components[i];
							if (masterPlanComponent.State != MasterPlanComponentState.Complete)
							{
								return QuestLineState.NotStarted;
							}
						}
						return (!plan.displayCooldownAlert) ? QuestLineState.Started : QuestLineState.Finished;
					}
					if (component.State == MasterPlanComponentState.Complete)
					{
						return QuestLineState.Finished;
					}
					return (component.State >= MasterPlanComponentState.InProgress) ? QuestLineState.Started : QuestLineState.NotStarted;
				}
			}

			public override int QuestLineID
			{
				get
				{
					return (components != null) ? plan.Definition.ID : component.ID;
				}
			}

			public override int GivenByCharacterID
			{
				get
				{
					return plan.Definition.VillainCharacterDefID;
				}
				set
				{
				}
			}
		}

		public sealed class ComponentTaskDefinition : QuestStepDefinition
		{
			public MasterPlanComponentTaskDefinition taskDefinition;
		}

		public sealed class ComponentTask : QuestStep
		{
			public MasterPlanComponentTask task;
		}

		public sealed class Component : Quest
		{
			private IList<QuestStep> questSteps;

			public MasterPlanComponent component;

			public MasterPlan masterPlan;

			public IList<MasterPlanComponent> components;

			public int buildDefId;

			public int index;

			public bool isBuildQuest;

			public bool isBuildingCompete;

			public override IList<QuestStep> Steps
			{
				get
				{
					QuestStepState? questStepState = null;
					if (components != null)
					{
						questStepState = ((!isBuildingCompete) ? QuestStepState.Ready : QuestStepState.Complete);
					}
					if (isBuildQuest)
					{
						questStepState = ((component.State <= MasterPlanComponentState.Scaffolding) ? QuestStepState.Ready : QuestStepState.Complete);
					}
					if (questStepState.HasValue)
					{
						if (questSteps.Count != 1)
						{
							questSteps.Clear();
							questSteps.Add(new ComponentTask());
						}
						questSteps[0].state = questStepState.Value;
						return questSteps;
					}
					int count = component.tasks.Count;
					if (questSteps.Count != count)
					{
						questSteps.Clear();
						for (int i = 0; i < count; i++)
						{
							questSteps.Add(new ComponentTask
							{
								task = component.tasks[i]
							});
						}
					}
					foreach (QuestStep questStep in questSteps)
					{
						MasterPlanComponentTask task = (questStep as ComponentTask).task;
						questStep.state = ((!task.isComplete) ? QuestStepState.Inprogress : QuestStepState.Complete);
						questStep.AmountCompleted = (int)task.earnedQuantity;
						questStep.AmountReady = (int)task.earnedQuantity;
					}
					return questSteps;
				}
				set
				{
					base.Steps = value;
				}
			}

			public override QuestState state
			{
				get
				{
					QuestState result = QuestState.Notstarted;
					if (components != null)
					{
						for (int i = 0; i < components.Count; i++)
						{
							MasterPlanComponent masterPlanComponent = components[i];
							if (masterPlanComponent.State != MasterPlanComponentState.Complete)
							{
								return result;
							}
						}
						return masterPlan.displayCooldownAlert ? QuestState.Complete : ((!isBuildingCompete) ? QuestState.RunningTasks : QuestState.Harvestable);
					}
					if (isBuildQuest)
					{
						if (component.State == MasterPlanComponentState.Built)
						{
							result = QuestState.Harvestable;
						}
						else if (component.State == MasterPlanComponentState.Complete)
						{
							result = QuestState.Complete;
						}
						else if (component.State == MasterPlanComponentState.TasksCollected || component.State == MasterPlanComponentState.Scaffolding)
						{
							result = QuestState.RunningTasks;
						}
						return result;
					}
					if (component.State == MasterPlanComponentState.InProgress)
					{
						result = QuestState.RunningTasks;
					}
					else if (component.State == MasterPlanComponentState.TasksCollected)
					{
						result = QuestState.Complete;
					}
					else if (component.State == MasterPlanComponentState.TasksComplete)
					{
						result = QuestState.Harvestable;
					}
					return result;
				}
				set
				{
					base.state = value;
				}
			}

			public override bool AutoGrantReward
			{
				get
				{
					return state == QuestState.Harvestable;
				}
				set
				{
					base.AutoGrantReward = value;
				}
			}

			public Component(ComponentDefinition def)
				: base(def)
			{
				questSteps = new List<QuestStep>();
			}

			public override string ToString()
			{
				return string.Format("Base: {0}, component: {1}, state: {2}, step count: {3}", base.ToString(), component, state, (Steps != null) ? Steps.Count : 0);
			}
		}

		public sealed class ComponentDefinition : QuestDefinition
		{
			public bool isBuildQuest;

			public MasterPlanComponentRewardDefinition reward;

			public override int RewardDisplayCount
			{
				get
				{
					if (reward == null)
					{
						return base.RewardDisplayCount;
					}
					if (isBuildQuest)
					{
						return 1;
					}
					int num = 0;
					if (reward.grindReward != 0)
					{
						num++;
					}
					if (reward.premiumReward != 0)
					{
						num++;
					}
					return num;
				}
				set
				{
					base.RewardDisplayCount = value;
				}
			}

			public override TransactionDefinition GetReward(IDefinitionService definitionService)
			{
				TransactionDefinition transactionDefinition;
				if (reward == null || (base.RewardTransaction != 0 && definitionService != null))
				{
					transactionDefinition = definitionService.Get<TransactionDefinition>(base.RewardTransaction);
					RewardDisplayCount = transactionDefinition.GetOutputCount();
					return transactionDefinition;
				}
				transactionDefinition = new TransactionDefinition();
				transactionDefinition.Outputs = new List<QuantityItem>();
				if (!isBuildQuest)
				{
					if (reward.grindReward != 0)
					{
						transactionDefinition.Outputs.Add(new QuantityItem(0, reward.grindReward));
					}
					if (reward.premiumReward != 0)
					{
						transactionDefinition.Outputs.Add(new QuantityItem(1, reward.premiumReward));
					}
					RewardDisplayCount = transactionDefinition.GetOutputCount();
					return transactionDefinition;
				}
				transactionDefinition.Outputs.Add(new QuantityItem(reward.rewardItemId, reward.rewardQuantity));
				RewardDisplayCount = transactionDefinition.GetOutputCount();
				return transactionDefinition;
			}
		}
	}
}
