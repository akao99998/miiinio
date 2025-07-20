using System;

namespace Kampai.Game
{
	[Flags]
	public enum AdjacentDirection
	{
		North = 1,
		East = 2,
		South = 4,
		West = 8
	}
}
