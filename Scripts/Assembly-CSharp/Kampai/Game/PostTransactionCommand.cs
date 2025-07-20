using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game.Transaction;
using Kampai.Game.View;
using Kampai.UI;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class PostTransactionCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("PostTransactionCommand") as IKampaiLogger;

		[Inject(GameElement.NAMED_CHARACTER_MANAGER)]
		public GameObject namedCharacterManager { get; set; }

		[Inject]
		public TransactionUpdateData update { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public SpawnDooberSignal tweenSignal { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public ITelemetryUtil telemetryUtil { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public IRandomService randomService { get; set; }

		[Inject]
		public UnlockCharacterModel unlockCharacterModel { get; set; }

		[Inject]
		public IBuildMenuService buildMenuService { get; set; }

		[Inject]
		public PlayerTrainingTransactionOutputExaminationSignal playerTrainingSignal { get; set; }

		[Inject]
		public InitializeMarketplaceSlotsSignal initializeSlotsSignal { get; set; }

		[Inject]
		public ProcessSpecialSaleItemSignal processSpecialSaleItemSignal { get; set; }

		[Inject]
		public IMasterPlanService masterPlanService { get; set; }

		public override void Execute()
		{
			if (update.Target != 0 && update.Target != TransactionTarget.REWARD_BUILDING)
			{
				RunScreenTween();
			}
			sendTelemetry();
			questService.UpdateAllQuestsWithQuestStepType(QuestStepType.Delivery);
			masterPlanService.ProcessTransactionData(update);
			playerTrainingSignal.Dispatch(update);
			processSpecialSaleItemSignal.Dispatch(update);
			CreateNewMinions();
			AddBuildMenuBadge();
			UpdateMarketplaceSlots();
		}

		private void RunScreenTween()
		{
			if (update.InstanceId == 0 && update.Outputs == null)
			{
				return;
			}
			Vector3 startLocation = GetStartLocation(update);
			bool type = !update.fromGlass;
			IList<QuantityItem> outputs = update.Outputs;
			foreach (QuantityItem item in outputs)
			{
				if (item.Quantity != 0)
				{
					if (item.ID == 0)
					{
						tweenSignal.Dispatch(startLocation, DestinationType.GRIND, -1, type);
					}
					if (item.ID == 2)
					{
						tweenSignal.Dispatch(startLocation, DestinationType.XP, -1, type);
					}
					if (item.ID == 1)
					{
						tweenSignal.Dispatch(startLocation, DestinationType.PREMIUM, -1, type);
					}
				}
			}
		}

		private Vector3 GetStartLocation(TransactionUpdateData update)
		{
			Vector3 result = Vector3.zero;
			int instanceId = update.InstanceId;
			Building byInstanceId = playerService.GetByInstanceId<Building>(instanceId);
			if (byInstanceId != null)
			{
				Location location = byInstanceId.Location;
				if (location != null)
				{
					result = new Vector3(location.x, 0f, location.y);
				}
			}
			else
			{
				if (instanceId != 301)
				{
					return update.startPosition;
				}
				NamedCharacterManagerView component = namedCharacterManager.GetComponent<NamedCharacterManagerView>();
				CharacterObject characterObject = component.Get(instanceId);
				if (characterObject != null)
				{
					result = new Vector3(characterObject.transform.position.x, 0f, characterObject.transform.position.z);
				}
			}
			return result;
		}

		private void sendTelemetry()
		{
			string sourceName = telemetryUtil.GetSourceName(update);
			if (update.Inputs != null)
			{
				sendSpentTelemetry(sourceName);
			}
			if (update.Outputs != null)
			{
				sendEarnedTelemetry(sourceName);
			}
			SendBuildingAcquiredTelemetry(sourceName);
		}

		private void SendBuildingAcquiredTelemetry(string source)
		{
			if (update == null || update.Outputs == null)
			{
				return;
			}
			int sourceDefId = 0;
			PackDefinition packDefinitionFromTransactionId = definitionService.GetPackDefinitionFromTransactionId(update.TransactionId);
			if (packDefinitionFromTransactionId != null)
			{
				sourceDefId = packDefinitionFromTransactionId.ID;
			}
			for (int i = 0; i < update.Outputs.Count; i++)
			{
				QuantityItem quantityItem = update.Outputs[i];
				BuildingDefinition definition;
				if (definitionService.TryGet<BuildingDefinition>(quantityItem.ID, out definition) && definition != null)
				{
					telemetryService.Send_Telemetry_EVT_USER_ACQUIRES_BUILDING(source, definition, sourceDefId);
				}
			}
		}

		private void sendSpentTelemetry(string sourceName)
		{
			if (update.Inputs.Count == 0)
			{
				return;
			}
			string highLevel = string.Empty;
			string specific = string.Empty;
			string type = string.Empty;
			string other = string.Empty;
			telemetryUtil.DetermineTaxonomy(update, false, out highLevel, out specific, out type, out other);
			foreach (QuantityItem input in update.Inputs)
			{
				if (input.ID == 0)
				{
					uint quantity = input.Quantity;
					telemetryService.Send_Telemetry_EVT_IGE_FREE_CREDITS_PURCHASE_REVENUE((int)quantity, sourceName, PurchaseAwarePlayerService.PurchasedCurrencyUsed(logger, playerService, false, quantity), highLevel, specific, type);
					continue;
				}
				if (input.ID == 1)
				{
					uint quantity2 = input.Quantity;
					telemetryService.Send_Telemetry_EVT_IGE_PAID_CREDITS_PURCHASE_REVENUE((int)quantity2, sourceName, PurchaseAwarePlayerService.PurchasedCurrencyUsed(logger, playerService, true, quantity2), highLevel, specific, type);
					continue;
				}
				ItemDefinition definition = null;
				if (definitionService.TryGet<ItemDefinition>(input.ID, out definition) && !TransactionTarget.BLACKMARKETBOARD.Equals(update.Target))
				{
					SendCraftableEarnedSpentTelemetry(sourceName, (int)input.Quantity, definition, false);
				}
			}
		}

		private void sendEarnedTelemetry(string sourceName)
		{
			foreach (QuantityItem output in update.Outputs)
			{
				if (output.ID == 0)
				{
					string eventName = ((!sourceName.Equals("MasterPlan") && !sourceName.Equals("Quest")) ? questService.GetEventName(sourceName) : sourceName);
					telemetryService.Send_Telemetry_EVT_IGE_FREE_CREDITS_EARNED((int)output.Quantity, eventName, update.IsFromPremiumSource);
					continue;
				}
				if (output.ID == 1)
				{
					telemetryService.Send_Telemetry_EVT_IGE_PAID_CREDITS_EARNED((int)output.Quantity, sourceName, update.IsFromPremiumSource);
					continue;
				}
				if (output.ID == 2)
				{
					if (update.Target == TransactionTarget.REWARD_BUILDING)
					{
						telemetryService.Send_Telemetry_EVT_PARTY_POINTS_EARNED((int)output.Quantity, sourceName);
					}
					continue;
				}
				if (output.ID == 50)
				{
					telemetryService.Send_Telemetry_EVT_IGE_RESOURCE_CRAFTABLE_EARNED((int)output.Quantity, "Minion Level Up Token", "Minion Token", sourceName, string.Empty, string.Empty);
					continue;
				}
				IngredientsItemDefinition definition = null;
				if (definitionService.TryGet<IngredientsItemDefinition>(output.ID, out definition))
				{
					SendCraftableEarnedSpentTelemetry(sourceName, (int)output.Quantity, definition, true);
				}
			}
			if (update.TransactionId != 5037 || update.NewItems == null)
			{
				return;
			}
			foreach (Instance newItem in update.NewItems)
			{
				SendCraftableEarnedSpentTelemetry(sourceName, 1, newItem.Definition, true);
			}
		}

		private void SendCraftableEarnedSpentTelemetry(string sourceName, int quantity, Definition def, bool earned)
		{
			string localizedKey = def.LocalizedKey;
			string highLevel = string.Empty;
			string specific = highLevel;
			string type = highLevel;
			string other = highLevel;
			TaxonomyDefinition taxonomyDefinition = def as TaxonomyDefinition;
			if (earned)
			{
				string text = ((taxonomyDefinition == null) ? string.Empty : SafeString(taxonomyDefinition.TaxonomySpecific));
				telemetryUtil.DetermineTaxonomy(update, false, out highLevel, out specific, out type, out other);
				if (string.IsNullOrEmpty(highLevel) || specific == text)
				{
					highLevel = sourceName;
					specific = (type = (other = string.Empty));
				}
				telemetryService.Send_Telemetry_EVT_IGE_RESOURCE_CRAFTABLE_EARNED(quantity, localizedKey, text, highLevel, specific, type);
			}
			else
			{
				if (taxonomyDefinition != null)
				{
					highLevel = SafeString(taxonomyDefinition.TaxonomyHighLevel);
					specific = SafeString(taxonomyDefinition.TaxonomySpecific);
					type = SafeString(taxonomyDefinition.TaxonomyType);
					other = SafeString(taxonomyDefinition.TaxonomyOther);
				}
				telemetryService.Send_Telemetry_EVT_IGE_RESOURCE_CRAFTABLE_SPENT(quantity, sourceName, localizedKey, highLevel, specific, type);
			}
		}

		private string SafeString(string input)
		{
			return input ?? string.Empty;
		}

		private void CreateNewMinions()
		{
			IList<Instance> newItems = update.NewItems;
			if (newItems == null)
			{
				return;
			}
			Minion minion = null;
			int i = 0;
			for (int count = newItems.Count; i < count; i++)
			{
				QuantityInstance quantityInstance = newItems[i] as QuantityInstance;
				if (quantityInstance != null && quantityInstance.ID == 5)
				{
					WeightedInstance weightedInstance = playerService.GetWeightedInstance(4007);
					for (int j = 0; j < quantityInstance.Quantity; j++)
					{
						QuantityItem quantityItem = weightedInstance.NextPick(randomService);
						MinionDefinition def = definitionService.Get<MinionDefinition>(quantityItem.ID);
						minion = new Minion(def);
						playerService.Add(minion);
						unlockCharacterModel.minionUnlocks.Add(minion);
					}
				}
				else
				{
					minion = newItems[i] as Minion;
					if (minion != null)
					{
						unlockCharacterModel.minionUnlocks.Add(minion);
					}
				}
			}
		}

		private void AddBuildMenuBadge()
		{
			PackDefinition packDefinition = null;
			if (update.Target != TransactionTarget.AUTOMATIC)
			{
				if (definitionService.GetPackTransaction(update.TransactionId) == null)
				{
					return;
				}
				packDefinition = definitionService.GetPackDefinitionFromTransactionId(update.TransactionId);
				if (packDefinition == null)
				{
					return;
				}
			}
			if (update.Outputs == null)
			{
				return;
			}
			for (int i = 0; i < update.Outputs.Count; i++)
			{
				QuantityItem quantityItem = update.Outputs[i];
				BuildingDefinition definition;
				if (definitionService.TryGet<BuildingDefinition>(quantityItem.ID, out definition))
				{
					buildMenuService.CompleteBuildMenuUpdate(definition.Type, definition.ID);
				}
			}
		}

		private void UpdateMarketplaceSlots()
		{
			IList<Instance> newItems = update.NewItems;
			if (newItems == null)
			{
				return;
			}
			foreach (Instance item in newItems)
			{
				if (item.Definition != null && item.Definition.ID == 316)
				{
					initializeSlotsSignal.Dispatch();
					break;
				}
			}
		}
	}
}
