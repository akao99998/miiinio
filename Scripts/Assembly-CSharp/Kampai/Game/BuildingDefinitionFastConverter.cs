using System;
using Kampai.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kampai.Game
{
	public class BuildingDefinitionFastConverter : FastJsonCreationConverter<BuildingDefinition>
	{
		private BuildingType.BuildingTypeIdentifier buildingType;

		public override BuildingDefinition ReadJson(JsonReader reader, JsonConverters converters)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			JObject jObject = JObject.Load(reader);
			JProperty jProperty = jObject.Property("type");
			if (jProperty != null)
			{
				string value = jProperty.Value.ToString();
				buildingType = (BuildingType.BuildingTypeIdentifier)(int)Enum.Parse(typeof(BuildingType.BuildingTypeIdentifier), value);
			}
			reader = jObject.CreateReader();
			return base.ReadJson(reader, converters);
		}

		public override BuildingDefinition Create()
		{
			switch (buildingType)
			{
			case BuildingType.BuildingTypeIdentifier.CRAFTING:
				return new CraftingBuildingDefinition();
			case BuildingType.BuildingTypeIdentifier.DECORATION:
				return new DecorationBuildingDefinition();
			case BuildingType.BuildingTypeIdentifier.RESOURCE:
				return new ResourceBuildingDefinition();
			case BuildingType.BuildingTypeIdentifier.LEISURE:
				return new LeisureBuildingDefintiion();
			case BuildingType.BuildingTypeIdentifier.SPECIAL:
				return new SpecialBuildingDefinition();
			case BuildingType.BuildingTypeIdentifier.BLACKMARKETBOARD:
				return new BlackMarketBoardDefinition();
			case BuildingType.BuildingTypeIdentifier.STORAGE:
				return new StorageBuildingDefinition();
			case BuildingType.BuildingTypeIdentifier.LANDEXPANSION:
				return new LandExpansionBuildingDefinition();
			case BuildingType.BuildingTypeIdentifier.DEBRIS:
				return new DebrisBuildingDefinition();
			case BuildingType.BuildingTypeIdentifier.MIGNETTE:
				return new MignetteBuildingDefinition();
			case BuildingType.BuildingTypeIdentifier.TIKIBAR:
				return new TikiBarBuildingDefinition();
			case BuildingType.BuildingTypeIdentifier.CABANA:
				return new CabanaBuildingDefinition();
			case BuildingType.BuildingTypeIdentifier.BRIDGE:
				return new BridgeBuildingDefinition();
			case BuildingType.BuildingTypeIdentifier.COMPOSITE:
				return new CompositeBuildingDefinition();
			case BuildingType.BuildingTypeIdentifier.STAGE:
				return new StageBuildingDefinition();
			case BuildingType.BuildingTypeIdentifier.FOUNTAIN:
				return new FountainBuildingDefinition();
			case BuildingType.BuildingTypeIdentifier.WELCOMEHUT:
				return new WelcomeHutBuildingDefinition();
			case BuildingType.BuildingTypeIdentifier.DCN:
				return new DCNBuildingDefinition();
			case BuildingType.BuildingTypeIdentifier.MESSAGEINABOTTLE:
				return new MIBBuildingDefinition();
			case BuildingType.BuildingTypeIdentifier.LAIR_ENTRANCE:
				return new VillainLairEntranceBuildingDefinition();
			case BuildingType.BuildingTypeIdentifier.LAIR_RESOURCEPLOT:
				return new VillainLairResourcePlotDefinition();
			case BuildingType.BuildingTypeIdentifier.MINION_UPGRADE:
				return new MinionUpgradeBuildingDefinition();
			case BuildingType.BuildingTypeIdentifier.MASTER_COMPONENT:
				return new MasterPlanComponentBuildingDefinition();
			case BuildingType.BuildingTypeIdentifier.MASTER_LEFTOVER:
				return new MasterPlanLeftOverBuildingDefinition();
			case BuildingType.BuildingTypeIdentifier.CONNECTABLE:
				return new ConnectableBuildingDefinition();
			default:
				throw new JsonSerializationException(string.Format("Unexpected BuildingDefinition type: {0}", buildingType));
			}
		}
	}
}
