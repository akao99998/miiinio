using Kampai.Util;

namespace Kampai.Game
{
	public interface TaskableMinionPartyBuilding : Building, IMinionPartyBuilding, RepairableBuilding, TaskableBuilding, Instance, Locatable, IFastJSONDeserializable, IFastJSONSerializable, Identifiable
	{
	}
	public abstract class TaskableMinionPartyBuilding<T> : TaskableBuilding<T>, Building, IMinionPartyBuilding, RepairableBuilding, TaskableBuilding, TaskableMinionPartyBuilding, Instance, Locatable, IFastJSONDeserializable, IFastJSONSerializable, Identifiable where T : TaskableMinionPartyBuildingDefinition
	{
		public TaskableMinionPartyBuilding(T definition)
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
