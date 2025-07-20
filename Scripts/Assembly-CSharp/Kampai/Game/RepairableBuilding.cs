using Kampai.Util;

namespace Kampai.Game
{
	public interface RepairableBuilding : Building, Instance, Locatable, IFastJSONDeserializable, IFastJSONSerializable, Identifiable
	{
	}
	public abstract class RepairableBuilding<T> : Building<T>, Building, RepairableBuilding, Instance, Locatable, IFastJSONDeserializable, IFastJSONSerializable, Identifiable where T : RepairableBuildingDefinition
	{
		public RepairableBuilding(T definition)
			: base(definition)
		{
		}

		public override string GetPrefab(int index = 0)
		{
			if (State == BuildingState.Broken || State == BuildingState.Inaccessible)
			{
				T definition = base.Definition;
				if (definition.brokenPrefab != null)
				{
					T definition2 = base.Definition;
					return definition2.brokenPrefab;
				}
			}
			return base.GetPrefab();
		}

		public override bool IsBuildingRepaired()
		{
			if (State == BuildingState.Broken || State == BuildingState.Inaccessible)
			{
				T definition = base.Definition;
				if (definition.brokenPrefab != null)
				{
					return false;
				}
			}
			return true;
		}
	}
}
