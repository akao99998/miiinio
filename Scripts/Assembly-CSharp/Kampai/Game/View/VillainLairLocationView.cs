using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class VillainLairLocationView : KampaiView
	{
		public List<GameObject> colliders;

		public GameObject masterPlanPlatformCollider;

		internal Dictionary<int, int> colliderInstanceKeysToComponentIDs = new Dictionary<int, int>();

		internal Dictionary<int, GameObject> componentIDKeysToColliders = new Dictionary<int, GameObject>();

		public void SetUpInstanceIDs(MasterPlanDefinition masterPlanDef, IPlayerService playerService, IKampaiLogger logger)
		{
			List<int> componentDefinitionIDs = masterPlanDef.ComponentDefinitionIDs;
			int count = componentDefinitionIDs.Count;
			if (count != colliders.Count)
			{
				logger.Error(string.Format("Count mismatch: we have {0} colliders and {1} components", colliders.Count, count));
			}
			for (int i = 0; i < colliders.Count; i++)
			{
				if (i < count)
				{
					int instanceID = colliders[i].GetInstanceID();
					int num = componentDefinitionIDs[i];
					colliderInstanceKeysToComponentIDs[instanceID] = num;
					componentIDKeysToColliders[num] = colliders[i];
					MasterPlanComponent firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<MasterPlanComponent>(num);
					if (firstInstanceByDefinitionId != null && firstInstanceByDefinitionId.State >= MasterPlanComponentState.Scaffolding)
					{
						EnableCollider(num, false);
					}
				}
			}
			colliderInstanceKeysToComponentIDs[masterPlanPlatformCollider.GetInstanceID()] = masterPlanDef.BuildingDefID;
			componentIDKeysToColliders[masterPlanDef.BuildingDefID] = masterPlanPlatformCollider;
		}

		internal void EnableCollider(int componentID, bool enable)
		{
			if (componentIDKeysToColliders.ContainsKey(componentID))
			{
				componentIDKeysToColliders[componentID].SetActive(enable);
			}
		}

		internal void EnableAllColliders(bool enable)
		{
			foreach (GameObject collider in colliders)
			{
				collider.SetActive(enable);
			}
		}
	}
}
