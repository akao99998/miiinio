using Kampai.Util;

namespace Kampai.Game
{
	public interface MinionPartyBuilding : Building, IMinionPartyBuilding, RepairableBuilding, Instance, Locatable, IFastJSONDeserializable, IFastJSONSerializable, Identifiable
	{
	}
	public abstract class MinionPartyBuilding<T> : RepairableBuilding<T>, Building, MinionPartyBuilding, IMinionPartyBuilding, RepairableBuilding, Instance, Locatable, IFastJSONDeserializable, IFastJSONSerializable, Identifiable where T : MinionPartyBuildingDefinition
	{
		public MinionPartyBuilding(T definition)
			: base(definition)
		{
		}

		public string GetPartyPrefab(MinionPartyType partyType)
		{
			string result = string.Empty;
			string text = partyType.ToString();
			T definition = base.Definition;
			if (definition.MinionPartyPrefabs != null)
			{
				T definition2 = base.Definition;
				foreach (MinionPartyPrefabDefinition minionPartyPrefab in definition2.MinionPartyPrefabs)
				{
					if (minionPartyPrefab.EventType == text)
					{
						result = minionPartyPrefab.Prefab;
						break;
					}
				}
			}
			return result;
		}
	}
}
