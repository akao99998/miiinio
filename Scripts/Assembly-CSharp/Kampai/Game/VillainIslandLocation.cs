using System.Collections.Generic;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.Game
{
	public class VillainIslandLocation : strange.extensions.mediation.impl.View
	{
		public List<GameObject> colliders;

		public void EnableColliders(bool enable)
		{
			foreach (GameObject collider in colliders)
			{
				collider.GetComponent<Collider>().enabled = enable;
			}
		}
	}
}
