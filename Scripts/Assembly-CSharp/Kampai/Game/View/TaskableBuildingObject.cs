using System;
using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public abstract class TaskableBuildingObject : AnimatingBuildingObject
	{
		protected Dictionary<int, RuntimeAnimatorController> minionControllers;

		private int[] layerIndicies;

		protected Dictionary<int, TaskingMinionObject> childAnimators = new Dictionary<int, TaskingMinionObject>();

		protected List<TaskingMinionObject> childQueue = new List<TaskingMinionObject>();

		internal override void Init(Building building, IKampaiLogger logger, IDictionary<string, RuntimeAnimatorController> controllers, IDefinitionService definitionService)
		{
			base.Init(building, logger, controllers, definitionService);
			TaskableBuildingDefinition taskableBuildingDefinition = building.Definition as TaskableBuildingDefinition;
			if (taskableBuildingDefinition == null)
			{
				logger.Fatal(FatalCode.BV_ILLEGAL_TASKABLE_DEFINITION, building.Definition.ID.ToString());
			}
			minionControllers = new Dictionary<int, RuntimeAnimatorController>();
			if (this is MignetteBuildingObject)
			{
				return;
			}
			foreach (BuildingAnimationDefinition animationDefinition in taskableBuildingDefinition.AnimationDefinitions)
			{
				if (controllers.ContainsKey(animationDefinition.MinionController))
				{
					minionControllers.Add(animationDefinition.CostumeId, controllers[animationDefinition.MinionController]);
				}
			}
			if (!minionControllers.ContainsKey(-1))
			{
				logger.Fatal(FatalCode.BV_NO_DEFAULT_ANIMATION_CONTROLLER, taskableBuildingDefinition.ID.ToString());
			}
		}

		internal void SetupLayers()
		{
			if (animators.Count == 0)
			{
				return;
			}
			int layerCount = animators[0].layerCount;
			layerIndicies = new int[stations];
			for (int i = 0; i < layerIndicies.Length; i++)
			{
				layerIndicies[i] = -1;
			}
			for (int j = 0; j < layerCount; j++)
			{
				string layerName = animators[0].GetLayerName(j);
				if (layerName.StartsWith("Pos"))
				{
					string value = layerName.Substring("Pos".Length);
					int num = Convert.ToInt32(value) - 1;
					if (num < 0 || num >= stations)
					{
						logger.Fatal(FatalCode.BV_NO_SUCH_WEIGHT_FOR_STATION);
					}
					layerIndicies[num] = j;
				}
			}
			if (stations <= 1 || !SupportsMultipleLayers())
			{
				return;
			}
			for (int k = 0; k < layerIndicies.Length; k++)
			{
				if (layerIndicies[k] == -1)
				{
					logger.Fatal(FatalCode.BV_MISSING_LAYER);
				}
			}
		}

		public void SetEnabledAllStations(bool isEnabled)
		{
			for (int i = 0; i < routes.Length; i++)
			{
				SetEnabledStation(i, isEnabled);
			}
		}

		public bool SupportsMultipleLayers()
		{
			return !(this is MignetteBuildingObject);
		}

		protected void SetEnabledStation(int station, bool isEnabled)
		{
			if (animators == null)
			{
				logger.Info("animators is null for building {0} ({1}), {2}s since startup", ID, definitionService.Get(base.DefinitionID).LocalizedKey, Time.realtimeSinceStartup);
			}
			foreach (Animator animator in animators)
			{
				SetStationState(animator, station, isEnabled);
			}
		}

		private void SetStationState(Animator animator, int station, bool isEnabled)
		{
			if (GetNumberOfStations() > 1 && SupportsMultipleLayers() && station < GetNumberOfStations() && animator != null && layerIndicies != null && station >= 0)
			{
				animator.SetLayerWeight(layerIndicies[station], (!isEnabled) ? 0f : 1f);
			}
		}

		protected void SetMinionTriggers(string name)
		{
			foreach (TaskingMinionObject value in childAnimators.Values)
			{
				MinionObject minion = value.Minion;
				minion.EnqueueAction(new SetAnimatorAction(minion, null, name, logger));
			}
		}

		public override void Update()
		{
			base.Update();
			AnimatorCullingMode animatorCullingMode = ((!(this is MignetteBuildingObject) && !HasVisibleRenderers()) ? AnimatorCullingMode.CullUpdateTransforms : AnimatorCullingMode.AlwaysAnimate);
			Dictionary<int, TaskingMinionObject>.Enumerator enumerator = childAnimators.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TaskingMinionObject value = enumerator.Current.Value;
					value.Minion.SetAnimatorCullingMode(animatorCullingMode);
				}
			}
			finally
			{
				enumerator.Dispose();
			}
		}

		internal virtual void RestMinion(int minionId)
		{
			for (int i = 0; i < childQueue.Count; i++)
			{
				TaskingMinionObject taskingMinionObject = childQueue[i];
				MinionObject minion = taskingMinionObject.Minion;
				if (minion.ID == minionId)
				{
					if (taskingMinionObject.RoutingIndex < GetNumberOfStations())
					{
						minion.EnqueueAction(new SetLayerAction(minion, 10, logger), true);
						string paramName = "OnWait";
						minion.EnqueueAction(new SetAnimatorAction(minion, null, paramName, logger));
						minion.EnqueueAction(new SetLayerAction(minion, 8, logger, 2));
						SetEnabledStation(i, false);
					}
					taskingMinionObject.IsResting = true;
					break;
				}
			}
			RestBuildingIfNeeded();
		}

		protected int GetActiveMinionCount()
		{
			int num = 0;
			foreach (TaskingMinionObject item in childQueue)
			{
				if (item.RoutingIndex < GetNumberOfStations() && !item.IsResting)
				{
					num++;
				}
			}
			return num;
		}

		private void RestBuildingIfNeeded()
		{
			if (GetActiveMinionCount() == 0 && !IsInAnimatorState(GetHashAnimationState("Base Layer.Wait")))
			{
				RestBuilding();
			}
		}

		private void RestBuilding()
		{
			EnqueueAction(new TriggerBuildingAnimationAction(this, OnlyStateEnabled("OnWait"), logger), true);
		}

		public void TrackChild(MinionObject child, RuntimeAnimatorController controller, bool alreadyRushed)
		{
			int routeForChild = GetRouteForChild();
			TaskingMinionObject taskingMinionObject = new TaskingMinionObject(child, routeForChild);
			taskingMinionObject.IsResting = alreadyRushed;
			if (!childAnimators.ContainsKey(child.ID))
			{
				childAnimators.Add(child.ID, taskingMinionObject);
			}
			if (!childQueue.Contains(taskingMinionObject))
			{
				childQueue.Add(taskingMinionObject);
			}
			child.ShelveActionQueue();
			child.ExecuteAction(new MuteAction(child, false, logger));
			if (!IsAnimating)
			{
				StartAnimating();
			}
			SetSiblingVFXScript(child.gameObject, vfxScript);
			SetupChild(routeForChild, taskingMinionObject, controller);
		}

		private void SetSiblingVFXScript(GameObject sibling, VFXScript vfxScript)
		{
			AnimEventHandler component = sibling.GetComponent<AnimEventHandler>();
			if (component != null)
			{
				component.SetSiblingVFXScript(vfxScript);
			}
		}

		protected int GetRouteForChild()
		{
			int result = childQueue.Count;
			int[] array = new int[routes.Length];
			foreach (TaskingMinionObject item in childQueue)
			{
				if (item.RoutingIndex < routes.Length)
				{
					array[item.RoutingIndex] = 1;
				}
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == 0)
				{
					result = i;
					break;
				}
			}
			return result;
		}

		public virtual void UntrackChild(int minionId, TaskableBuilding building)
		{
			if (childAnimators.ContainsKey(minionId))
			{
				TaskingMinionObject taskingMinionObject = childAnimators[minionId];
				MinionObject minion = taskingMinionObject.Minion;
				minion.ApplyRootMotion(false);
				minion.UnshelveActionQueue();
				minion.EnableBlobShadow(true);
				minion.SetAnimatorCullingMode(AnimatorCullingMode.CullUpdateTransforms);
				UnlinkChild(minionId);
				FillEmptyStation(taskingMinionObject.RoutingIndex);
				if (childAnimators.Count == 0)
				{
					EnqueueAction(new TriggerBuildingAnimationAction(this, OnlyStateEnabled("OnStop"), logger), true);
				}
				else
				{
					RestBuildingIfNeeded();
				}
				SetSiblingVFXScript(minion.gameObject, null);
				SetEnabledStation(taskingMinionObject.RoutingIndex, false);
			}
		}

		public void ReleaseMinionsForParty(TaskableBuilding building)
		{
			foreach (int minion in building.MinionList)
			{
				UntrackChild(minion, building);
			}
		}

		public virtual void FadeMinions(ToggleMinionRendererSignal toggleMinionSignal, bool fadeIn)
		{
			foreach (int key in childAnimators.Keys)
			{
				toggleMinionSignal.Dispatch(key, fadeIn);
			}
		}

		protected Vector3 GetRandomExit(Building building)
		{
			string buildingFootprint = definitionService.GetBuildingFootprint(building.Definition.FootprintID);
			Point point = new Point(building.Location.x, building.Location.y);
			List<Vector3> list = new List<Vector3>();
			int num = 0;
			int num2 = 0;
			string text = buildingFootprint;
			for (int i = 0; i < text.Length; i++)
			{
				switch (text[i])
				{
				case '.':
				{
					Point point2 = new Point(point.x + num, point.y + num2);
					if (!IsOccupiedByMinion(point2))
					{
						list.Add(new Vector3(point2.x, 0f, point2.y));
					}
					break;
				}
				case '|':
					num = 0;
					num2--;
					break;
				}
				num++;
			}
			if (list.Count == 0)
			{
				return base.gameObject.transform.position;
			}
			int index = UnityEngine.Random.Range(0, list.Count - 1);
			return list[index];
		}

		private bool IsOccupiedByMinion(Point point)
		{
			Collider[] array = Physics.OverlapSphere(new Vector3(point.x, 1f, point.y), 1f);
			Collider[] array2 = array;
			foreach (Collider collider in array2)
			{
				if (collider.gameObject.GetComponent<MinionObject>() != null)
				{
					return true;
				}
			}
			return false;
		}

		private void FillEmptyStation(int vacant)
		{
			int numberOfStations = GetNumberOfStations();
			if (numberOfStations > childQueue.Count)
			{
				return;
			}
			foreach (TaskingMinionObject item in childQueue)
			{
				if (item.RoutingIndex >= numberOfStations)
				{
					SetupChild(vacant, item);
					break;
				}
			}
		}

		protected void UnlinkChild(int minionId)
		{
			int num = -1;
			for (int i = 0; i < childQueue.Count; i++)
			{
				if (childQueue[i].ID == minionId)
				{
					num = i;
					break;
				}
			}
			if (num > -1)
			{
				childQueue.RemoveAt(num);
			}
			else
			{
				logger.Log(KampaiLogLevel.Error, "Not found");
			}
			childAnimators.Remove(minionId);
		}

		protected virtual void SetupChild(int routingIndex, TaskingMinionObject taskingChild, RuntimeAnimatorController controller = null)
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
				minion.ApplyRootMotion(false);
				minion.ExecuteAction(new SetAnimatorAction(minion, null, logger, new Dictionary<string, object> { { "minionPosition", routingIndex } }));
				Dictionary<string, object> dictionary = OnlyStateEnabled("OnLoop");
				int hashAnimationState;
				int hashAnimationState2;
				if (taskingChild.IsResting)
				{
					dictionary["OnWait"] = true;
					hashAnimationState = GetHashAnimationState("Base Layer.Wait_Pos" + (routingIndex + 1));
					hashAnimationState2 = GetHashAnimationState("Base Layer.Wait");
				}
				else
				{
					hashAnimationState = GetHashAnimationState("Base Layer.Loop_Pos" + (routingIndex + 1));
					hashAnimationState2 = GetHashAnimationState("Base Layer.Loop");
				}
				minion.EnqueueAction(new SetAnimatorAction(minion, null, logger, OnlyStateEnabled("OnStop")), true);
				minion.EnqueueAction(new WaitForMecanimStateAction(minion, GetHashAnimationState("Base Layer.Idle"), logger));
				minion.EnqueueAction(new SetAnimatorAction(minion, null, logger, dictionary));
				minion.EnqueueAction(new SkipToTimeAction(minion, new SkipToTime(0, hashAnimationState, GetCurrentAnimationTimeForState(hashAnimationState2)), logger));
				MoveToRoutingPosition(minion, routingIndex);
				SetEnabledStation(routingIndex, !taskingChild.IsResting);
			}
			else
			{
				minion.EnableRenderers(false);
			}
		}

		protected TaskingMinionObject GetByRouteSlot(int routingIndex)
		{
			foreach (TaskingMinionObject item in childQueue)
			{
				if (item.RoutingIndex == routingIndex)
				{
					return item;
				}
			}
			return null;
		}
	}
}
