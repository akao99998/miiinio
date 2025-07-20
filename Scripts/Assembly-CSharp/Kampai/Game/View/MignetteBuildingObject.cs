using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class MignetteBuildingObject : TaskableBuildingObject
	{
		private List<BuildingAnimationDefinition> AnimationDefinitions;

		[Inject]
		public PathFinder pathFinder { get; set; }

		internal override void Init(Building building, IKampaiLogger logger, IDictionary<string, RuntimeAnimatorController> controllers, IDefinitionService definitionService)
		{
			base.Init(building, logger, controllers, definitionService);
			AnimatingBuildingDefinition animatingBuildingDefinition = building.Definition as AnimatingBuildingDefinition;
			if (animatingBuildingDefinition != null && animatingBuildingDefinition.AnimationDefinitions != null)
			{
				AnimationDefinitions = (List<BuildingAnimationDefinition>)animatingBuildingDefinition.AnimationDefinitions;
			}
			else
			{
				logger.Fatal(FatalCode.BV_NO_DEFAULT_ANIMATION_CONTROLLER, animatingBuildingDefinition.ID.ToString());
			}
		}

		public TaskingMinionObject GetChildMinion(int index)
		{
			return childQueue[index];
		}

		public Vector3 GetRouteLocation(int routeIndex)
		{
			if (routeIndex >= 0 && routeIndex < routes.Length)
			{
				return routes[routeIndex].position;
			}
			return Vector3.zero;
		}

		public Vector3 GetRouteForward(int routeIndex)
		{
			if (routeIndex >= 0 && routeIndex < routes.Length)
			{
				return routes[routeIndex].forward;
			}
			return Vector3.forward;
		}

		public int GetMignetteMinionCount()
		{
			return GetActiveMinionCount();
		}

		public void LoadMignetteAnimationControllers(Dictionary<string, RuntimeAnimatorController> animationControllers)
		{
			foreach (BuildingAnimationDefinition animationDefinition in AnimationDefinitions)
			{
				if (!buildingControllers.ContainsKey(animationDefinition.CostumeId) && !string.IsNullOrEmpty(animationDefinition.BuildingController))
				{
					RuntimeAnimatorController runtimeAnimatorController = KampaiResources.Load<RuntimeAnimatorController>(animationDefinition.BuildingController);
					if (runtimeAnimatorController == null)
					{
						logger.Fatal(FatalCode.BV_NO_DEFAULT_ANIMATION_CONTROLLER, animationDefinition.ID.ToString());
						break;
					}
					buildingControllers.Add(animationDefinition.CostumeId, runtimeAnimatorController);
					if (!animationControllers.ContainsKey(animationDefinition.BuildingController))
					{
						animationControllers.Add(animationDefinition.BuildingController, runtimeAnimatorController);
					}
				}
				if (!minionControllers.ContainsKey(animationDefinition.CostumeId) && !string.IsNullOrEmpty(animationDefinition.MinionController))
				{
					RuntimeAnimatorController runtimeAnimatorController2 = KampaiResources.Load<RuntimeAnimatorController>(animationDefinition.MinionController);
					if (runtimeAnimatorController2 == null)
					{
						logger.Fatal(FatalCode.BV_NO_DEFAULT_ANIMATION_CONTROLLER, animationDefinition.ID.ToString());
						break;
					}
					minionControllers.Add(animationDefinition.CostumeId, runtimeAnimatorController2);
					if (!animationControllers.ContainsKey(animationDefinition.MinionController))
					{
						animationControllers.Add(animationDefinition.MinionController, runtimeAnimatorController2);
					}
				}
			}
		}

		public override void StartAnimating()
		{
		}

		protected override void SetupChild(int routingIndex, TaskingMinionObject taskingChild, RuntimeAnimatorController controller = null)
		{
			taskingChild.RoutingIndex = routingIndex;
			MinionObject minion = taskingChild.Minion;
			if (controller != null)
			{
				minion.SetAnimController(controller);
			}
			int numberOfStations = GetNumberOfStations();
			if (routingIndex < numberOfStations)
			{
				minion.ApplyRootMotion(true);
				minion.EnableRenderers(false);
				AddMinionRigidBody(minion);
				MoveToRoutingPosition(minion, routingIndex);
			}
		}

		public override void UntrackChild(int minionId, TaskableBuilding building)
		{
			Point partyPoint = pathFinder.PartyPoint;
			Vector3 position = new Vector3(partyPoint.x, 0f, partyPoint.y);
			PurchasedLandExpansion byInstanceId = base.playerService.GetByInstanceId<PurchasedLandExpansion>(354);
			if (!childAnimators.ContainsKey(minionId))
			{
				return;
			}
			TaskingMinionObject taskingMinionObject = childAnimators[minionId];
			MinionObject minion = taskingMinionObject.Minion;
			minion.SetRenderLayer(10);
			minion.ApplyRootMotion(false);
			minion.UnshelveActionQueue();
			bool flag = true;
			MignetteBuilding mignetteBuilding = building as MignetteBuilding;
			if (mignetteBuilding != null && mignetteBuilding.Definition.LandExpansionID != 0)
			{
				int landExpansionID = mignetteBuilding.Definition.LandExpansionID;
				if (!byInstanceId.HasPurchased(landExpansionID))
				{
					flag = false;
				}
			}
			if (!flag)
			{
				minion.transform.position = position;
				minion.ClearActionQueue();
			}
			else
			{
				minion.transform.position = ((building == null) ? base.gameObject.transform.position : GetRandomExit(building));
			}
			minion.transform.rotation = Quaternion.identity;
			minion.EnableBlobShadow(true);
			minion.SetAnimatorCullingMode(AnimatorCullingMode.CullUpdateTransforms);
			minion.EnqueueAction(new SetLayerAction(minion, 8, logger, 2));
			RemoveMinionRigidBody(minion);
			UnlinkChild(minionId);
		}

		public override bool CanFadeGFX()
		{
			return false;
		}

		public override bool CanFadeSFX()
		{
			return false;
		}

		private void AddMinionRigidBody(MinionObject minion)
		{
			Rigidbody component = minion.gameObject.GetComponent<Rigidbody>();
			if (component == null)
			{
				component = minion.gameObject.AddComponent<Rigidbody>();
				component.useGravity = false;
				component.isKinematic = true;
			}
		}

		private void RemoveMinionRigidBody(MinionObject minion)
		{
			Rigidbody component = minion.gameObject.GetComponent<Rigidbody>();
			if (component != null)
			{
				Object.Destroy(component);
			}
		}
	}
}
