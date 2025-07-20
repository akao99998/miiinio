using System.Collections.Generic;
using Kampai.Game.Transaction;
using Kampai.Util;

namespace Kampai.Game
{
	public class TelemetryUtil : ITelemetryUtil
	{
		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ILandExpansionService landExpansionService { get; set; }

		public void DetermineTaxonomy(TransactionUpdateData update, bool concatenateType, out string highLevel, out string specific, out string type, out string other)
		{
			highLevel = string.Empty;
			specific = string.Empty;
			type = string.Empty;
			other = string.Empty;
			if (update.Outputs != null)
			{
				foreach (QuantityItem output in update.Outputs)
				{
					if (output.ID != 0 && output.ID <= 8)
					{
						continue;
					}
					TaxonomyDefinition definition = null;
					int iD = output.ID;
					if (update.Target == TransactionTarget.LAND_EXPANSION || update.Target == TransactionTarget.REPAIR_BRIDGE)
					{
						LandExpansionConfig definition2 = null;
						if (definitionService.TryGet<LandExpansionConfig>(output.ID, out definition2))
						{
							List<LandExpansionBuilding> list = landExpansionService.GetBuildingsByExpansionID(definition2.expansionId) as List<LandExpansionBuilding>;
							if (list != null && list.Count > 0)
							{
								iD = list[0].Definition.ID;
							}
						}
					}
					if (definitionService.TryGet<TaxonomyDefinition>(iD, out definition))
					{
						DetermineTaxonomy(definition, concatenateType, ref highLevel, ref specific, ref type, ref other);
					}
				}
			}
			else if (update.Source == "MasterPlanRush")
			{
				TaxonomyDefinition definition3 = null;
				MasterPlanDefinition definition4 = null;
				int id = 65000;
				if (definitionService.TryGet<MasterPlanDefinition>(id, out definition4))
				{
					int buildingDefID = definition4.BuildingDefID;
					if (definitionService.TryGet<TaxonomyDefinition>(buildingDefID, out definition3))
					{
						DetermineTaxonomy(definition3, concatenateType, ref highLevel, ref specific, ref type, ref other);
					}
				}
			}
			if (update.InstanceId != 0)
			{
				TaxonomyDefinition definition5 = null;
				Instance byInstanceId = playerService.GetByInstanceId<Instance>(update.InstanceId);
				if (byInstanceId != null)
				{
					definition5 = byInstanceId.Definition as TaxonomyDefinition;
				}
				else
				{
					definitionService.TryGet<TaxonomyDefinition>(update.InstanceId, out definition5);
				}
				if (definition5 != null)
				{
					DetermineTaxonomy(definition5, false, ref highLevel, ref specific, ref type, ref other);
				}
			}
		}

		private void DetermineTaxonomy(TaxonomyDefinition taxonomyDef, bool concatenateType, ref string highLevel, ref string specific, ref string type, ref string other)
		{
			highLevel = SafeString(taxonomyDef.TaxonomyHighLevel);
			specific = SafeString(taxonomyDef.TaxonomySpecific);
			if (!concatenateType || string.IsNullOrEmpty(type))
			{
				type = SafeString(taxonomyDef.TaxonomyType);
			}
			else
			{
				string taxonomyType = taxonomyDef.TaxonomyType;
				if (!string.IsNullOrEmpty(taxonomyType))
				{
					type = type + ", " + taxonomyType;
				}
			}
			other = SafeString(taxonomyDef.TaxonomyOther);
		}

		private string SafeString(string input)
		{
			return input ?? string.Empty;
		}

		public string GetSourceName(TransactionUpdateData update)
		{
			string text = update.Source;
			int instanceId = update.InstanceId;
			int transactionId = update.TransactionId;
			if (instanceId != 0 && string.IsNullOrEmpty(text))
			{
				Instance byInstanceId = playerService.GetByInstanceId<Instance>(instanceId);
				if (byInstanceId != null)
				{
					text = byInstanceId.Definition.LocalizedKey;
				}
			}
			else if (text == null && transactionId != 0)
			{
				TransactionDefinition definition = null;
				if (definitionService.TryGet<TransactionDefinition>(transactionId, out definition) && !string.IsNullOrEmpty(definition.LocalizedKey))
				{
					text = definition.LocalizedKey;
				}
			}
			Minion byInstanceId2 = playerService.GetByInstanceId<Minion>(instanceId);
			if (byInstanceId2 != null)
			{
				text = "Minion Level Up";
			}
			if (string.IsNullOrEmpty(text))
			{
				text = "unknown";
			}
			return text;
		}
	}
}
