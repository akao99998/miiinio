using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class DebrisBuildingObject : TaskableBuildingObject
	{
		public Renderer[] objRenderers;

		internal override void Init(Building building, IKampaiLogger logger, IDictionary<string, RuntimeAnimatorController> controllers, IDefinitionService definitionService)
		{
			objRenderers = base.gameObject.GetComponentsInChildren<MeshRenderer>();
			base.Init(building, logger, controllers, definitionService);
		}

		public void EnableObjectRenderers(bool enable)
		{
			for (int i = 0; i < objRenderers.Length; i++)
			{
				objRenderers[i].enabled = enable;
			}
		}

		protected override void SetupChild(int routingIndex, TaskingMinionObject taskingChild, RuntimeAnimatorController controller)
		{
			taskingChild.RoutingIndex = routingIndex;
			MinionObject minion = taskingChild.Minion;
			if (controller != null)
			{
				taskingChild.Minion.SetAnimController(controller);
			}
			minion.ApplyRootMotion(true);
			minion.EnableRenderers(true);
			minion.ExecuteAction(new SetAnimatorAction(minion, null, logger, new Dictionary<string, object> { { "minionPosition", routingIndex } }));
			minion.ExecuteAction(new SetAnimatorAction(minion, null, "OnGag", logger));
			minion.EnqueueAction(new WaitForMecanimStateAction(minion, GetHashAnimationState("Base Layer.Gag"), logger));
			minion.EnqueueAction(new WaitForMecanimStateAction(minion, GetHashAnimationState("Base Layer.Exit"), logger));
			MoveToRoutingPosition(minion, routingIndex);
		}

		public void FadeDebris()
		{
			Animator animator = GetComponent<Animator>();
			if (animator == null)
			{
				animator = base.gameObject.AddComponent<Animator>();
			}
			if (buildingControllers.ContainsKey(0))
			{
				animator.runtimeAnimatorController = buildingControllers[0];
			}
			else
			{
				logger.Error("Could not find fade animator for debris object.");
			}
		}

		protected override Vector3 GetZoomCenterPosition()
		{
			Vector3 position = base.transform.position;
			position.y = 0f;
			return position;
		}
	}
}
