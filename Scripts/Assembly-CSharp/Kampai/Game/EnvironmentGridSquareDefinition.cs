using System;
using UnityEngine;

namespace Kampai.Game
{
	public class EnvironmentGridSquareDefinition
	{
		[Flags]
		private enum ModifierType
		{
			Useable = 1,
			Water = 2,
			Pathable = 4,
			CharacterPathable = 8
		}

		public bool Usable { get; set; }

		public bool Water { get; set; }

		public bool Pathable { get; set; }

		public bool CharacterPathable { get; set; }

		public Vector2 Position { get; set; }

		public void SetModifiers(int input)
		{
			Usable = (1 & input) == 1;
			Water = (2 & input) == 2;
			Pathable = (4 & input) == 4;
			CharacterPathable = (8 & input) == 8;
		}
	}
}
