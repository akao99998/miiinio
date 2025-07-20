using System.Collections.Generic;
using Kampai.Game.View;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class MinionUpgradeBuilding : RepairableBuilding<MinionUpgradeBuildingDefinition>
	{
		public List<int> processedPopulationBenefitDefinitionIDs = new List<int>();

		public MinionUpgradeBuilding(MinionUpgradeBuildingDefinition def)
			: base(def)
		{
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "PROCESSEDPOPULATIONBENEFITDEFINITIONIDS":
				reader.Read();
				processedPopulationBenefitDefinitionIDs = ReaderUtil.PopulateListInt32(reader, processedPopulationBenefitDefinitionIDs);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}

		public override void Serialize(JsonWriter writer)
		{
			writer.WriteStartObject();
			SerializeProperties(writer);
			writer.WriteEndObject();
		}

		protected override void SerializeProperties(JsonWriter writer)
		{
			base.SerializeProperties(writer);
			if (processedPopulationBenefitDefinitionIDs == null)
			{
				return;
			}
			writer.WritePropertyName("processedPopulationBenefitDefinitionIDs");
			writer.WriteStartArray();
			List<int>.Enumerator enumerator = processedPopulationBenefitDefinitionIDs.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					int current = enumerator.Current;
					writer.WriteValue(current);
				}
			}
			finally
			{
				enumerator.Dispose();
			}
			writer.WriteEndArray();
		}

		public override BuildingObject AddBuildingObject(GameObject gameObject)
		{
			return gameObject.AddComponent<MinionUpgradeBuildingObject>();
		}
	}
}
