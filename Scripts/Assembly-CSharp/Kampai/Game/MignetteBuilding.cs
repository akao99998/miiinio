using System;
using System.Collections.Generic;
using Kampai.Game.View;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class MignetteBuilding : TaskableBuilding<MignetteBuildingDefinition>, Building, IBuildingWithCooldown, Instance, Locatable, IFastJSONDeserializable, IFastJSONSerializable, Identifiable
	{
		public int MignetteData;

		public IList<int> StartedMainCollectionIDs { get; set; }

		public IList<int> StartedRepeatableCollectionIDs { get; set; }

		public int TotalScore { get; set; }

		[JsonIgnore]
		public MignetteBuildingDefinition MignetteBuildingDefinition
		{
			get
			{
				return base.Definition;
			}
		}

		public MignetteBuilding(MignetteBuildingDefinition def)
			: base(def)
		{
			StartedMainCollectionIDs = new List<int>();
			StartedRepeatableCollectionIDs = new List<int>();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "STARTEDMAINCOLLECTIONIDS":
				reader.Read();
				StartedMainCollectionIDs = ReaderUtil.PopulateListInt32(reader, StartedMainCollectionIDs);
				break;
			case "STARTEDREPEATABLECOLLECTIONIDS":
				reader.Read();
				StartedRepeatableCollectionIDs = ReaderUtil.PopulateListInt32(reader, StartedRepeatableCollectionIDs);
				break;
			case "TOTALSCORE":
				reader.Read();
				TotalScore = Convert.ToInt32(reader.Value);
				break;
			case "MIGNETTEDATA":
				reader.Read();
				MignetteData = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
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
			if (StartedMainCollectionIDs != null)
			{
				writer.WritePropertyName("StartedMainCollectionIDs");
				writer.WriteStartArray();
				IEnumerator<int> enumerator = StartedMainCollectionIDs.GetEnumerator();
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
			if (StartedRepeatableCollectionIDs != null)
			{
				writer.WritePropertyName("StartedRepeatableCollectionIDs");
				writer.WriteStartArray();
				IEnumerator<int> enumerator2 = StartedRepeatableCollectionIDs.GetEnumerator();
				try
				{
					while (enumerator2.MoveNext())
					{
						int current2 = enumerator2.Current;
						writer.WriteValue(current2);
					}
				}
				finally
				{
					enumerator2.Dispose();
				}
				writer.WriteEndArray();
			}
			writer.WritePropertyName("TotalScore");
			writer.WriteValue(TotalScore);
			writer.WritePropertyName("MignetteData");
			writer.WriteValue(MignetteData);
		}

		public override BuildingObject AddBuildingObject(GameObject gameObject)
		{
			return gameObject.AddComponent<MignetteBuildingObject>();
		}

		public int GetCooldown()
		{
			return MignetteBuildingDefinition.CooldownInSeconds;
		}

		public string SelectMenuToLoad(bool ownsMinigamePack)
		{
			if (AreAllMinionSlotsFilled() && !ownsMinigamePack)
			{
				return "MignettePlayConfirmMenu";
			}
			if (ownsMinigamePack)
			{
				return "MignetteCallMinionsMenu";
			}
			return "MignetteCallMinionsRequiredMenu";
		}

		public override int GetTransactionID(IDefinitionService definitionService)
		{
			return 5001;
		}
	}
}
