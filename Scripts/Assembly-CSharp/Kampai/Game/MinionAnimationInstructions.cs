using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game
{
	public class MinionAnimationInstructions
	{
		public HashSet<int> MinionIds { get; set; }

		public Boxed<Vector3> Center { get; set; }

		public bool Party { get; set; }

		public MinionAnimationInstructions(HashSet<int> MinionIds = null, Boxed<Vector3> Center = null, bool Party = false)
		{
			this.MinionIds = MinionIds;
			this.Center = Center;
			this.Party = Party;
		}
	}
}
