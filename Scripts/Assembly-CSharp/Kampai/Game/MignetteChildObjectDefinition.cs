using UnityEngine;

namespace Kampai.Game
{
	public class MignetteChildObjectDefinition
	{
		public string Prefab { get; set; }

		public Vector3 Position { get; set; }

		public bool IsLocal { get; set; }

		public float Rotation { get; set; }
	}
}
