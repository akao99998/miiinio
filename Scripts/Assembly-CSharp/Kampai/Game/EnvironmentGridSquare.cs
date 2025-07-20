using System;
using UnityEngine;

namespace Kampai.Game
{
	public class EnvironmentGridSquare
	{
		[Flags]
		public enum ModifierType
		{
			Unlocked = 1,
			Occupied = 2,
			Walkable = 4,
			CharacterWalkable = 8
		}

		public bool Unlocked
		{
			get
			{
				return (Modifier & 1) != 0;
			}
			set
			{
				UpdateModifier(value, 1);
			}
		}

		public bool Occupied
		{
			get
			{
				return (Modifier & 2) != 0;
			}
			set
			{
				UpdateModifier(value, 2);
			}
		}

		public bool Walkable
		{
			get
			{
				return (Modifier & 4) != 0;
			}
			set
			{
				UpdateModifier(value, 4);
			}
		}

		public bool CharacterWalkable
		{
			get
			{
				return (Modifier & 8) != 0;
			}
			set
			{
				UpdateModifier(value, 8);
			}
		}

		public Vector2 Position { get; set; }

		public Instance Instance { get; set; }

		public int Modifier { get; set; }

		private void UpdateModifier(bool value, int type)
		{
			if (value)
			{
				Modifier |= type;
			}
			else
			{
				Modifier &= ~type;
			}
		}
	}
}
