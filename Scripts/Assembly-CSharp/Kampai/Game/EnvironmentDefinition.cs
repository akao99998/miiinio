using Kampai.Util;

namespace Kampai.Game
{
	public class EnvironmentDefinition : Definition
	{
		[FastDeserializerIgnore]
		public EnvironmentGridSquareDefinition[,] DefinitionGrid;

		public override int TypeCode
		{
			get
			{
				return 1086;
			}
		}

		public bool IsUsable(int x, int z)
		{
			return DefinitionGrid[x, z].Usable;
		}

		public bool IsUsable(Location location)
		{
			return IsUsable(location.x, location.y);
		}

		public bool IsWater(int x, int z)
		{
			return DefinitionGrid[x, z].Water;
		}

		public bool IsWater(Location location)
		{
			return IsWater(location.x, location.y);
		}
	}
}
