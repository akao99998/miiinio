using System.Collections.Generic;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class ConstuctionQuestStepController : QuestStepController
	{
		public ConstuctionQuestStepController(Quest quest, int stepIndex, IQuestScriptService questScriptService, IPlayerService playerService, ICrossContextCapable gameContext, IKampaiLogger logger)
			: base(quest, stepIndex, questScriptService, playerService, gameContext, logger)
		{
		}

		public override void UpdateTask(QuestTaskTransition questTaskTransition, Building building, int buildingDefId, int itemDefId)
		{
			int num = 0;
			int num2 = 0;
			bool flag = false;
			ICollection<Building> byDefinitionId = playerService.GetByDefinitionId<Building>(base.questStepDefinition.ItemDefinitionID);
			QuestStep questStep = base.questStep;
			foreach (Building item in byDefinitionId)
			{
				BuildingState state = item.State;
				QuestStepState state2 = questStep.state;
				int iD = item.Definition.ID;
				if (base.questStepDefinition.ItemDefinitionID == iD)
				{
					Building unreadyBuilding = GetUnreadyBuilding(iD);
					questStep.TrackedID = ((unreadyBuilding != null) ? unreadyBuilding.ID : 0);
					if (state2 == QuestStepState.Notstarted)
					{
						GoToNextState();
						flag = true;
					}
					switch (state)
					{
					case BuildingState.Complete:
						num++;
						break;
					case BuildingState.Idle:
					case BuildingState.Working:
					case BuildingState.Harvestable:
					case BuildingState.Cooldown:
					case BuildingState.HarvestableAndWorking:
						num2++;
						break;
					}
				}
			}
			questStep.AmountReady = num;
			questStep.AmountCompleted = num2;
			if (questStep.state != QuestStepState.Ready && questStep.AmountReady + questStep.AmountCompleted >= base.questStepDefinition.ItemAmount)
			{
				GoToTaskState(QuestStepState.Ready);
			}
			if (questStep.AmountCompleted >= base.questStepDefinition.ItemAmount)
			{
				if (flag)
				{
					GoToTaskState(QuestStepState.WaitComplete);
				}
				else
				{
					GoToNextState(true);
				}
			}
		}

		private Building GetUnreadyBuilding(int buildingDefId)
		{
			ICollection<Building> byDefinitionId = playerService.GetByDefinitionId<Building>(buildingDefId);
			foreach (Building item in byDefinitionId)
			{
				BuildingState state = item.State;
				if (state == BuildingState.Inactive || state == BuildingState.Construction || state == BuildingState.Complete)
				{
					return item;
				}
			}
			return null;
		}

		public override void SetupTracking()
		{
			int itemDefinitionID = base.questStepDefinition.ItemDefinitionID;
			Building unreadyBuilding = GetUnreadyBuilding(itemDefinitionID);
			base.questStep.TrackedID = ((unreadyBuilding != null) ? unreadyBuilding.ID : 0);
		}

		public override string GetStepAction(ILocalizationService localService)
		{
			return localService.GetString("constructAction");
		}

		public override string GetStepDescription(ILocalizationService localService, IDefinitionService defService)
		{
			BuildingDefinition buildingDefinition = defService.Get<BuildingDefinition>(base.questStepDefinition.ItemDefinitionID);
			string @string = localService.GetString(buildingDefinition.LocalizedKey + "*", base.questStepDefinition.ItemAmount);
			return localService.GetString("constructTaskDesc", @string);
		}

		public override void GetStepDescIcon(IDefinitionService defService, out Sprite mainSprite, out Sprite maskSprite)
		{
			BuildingDefinition buildingDefinition = defService.Get<BuildingDefinition>(base.questStepDefinition.ItemDefinitionID);
			mainSprite = UIUtils.LoadSpriteFromPath(buildingDefinition.Image);
			maskSprite = UIUtils.LoadSpriteFromPath(buildingDefinition.Mask);
		}
	}
}
