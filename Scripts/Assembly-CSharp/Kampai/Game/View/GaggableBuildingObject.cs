using System.Collections.Generic;
using UnityEngine;

namespace Kampai.Game.View
{
	public class GaggableBuildingObject : TaskableBuildingObject, IRequiresBuildingScaffolding
	{
		internal virtual bool TriggerGagAnimation()
		{
			if (GetActiveMinionCount() == stations)
			{
				IList<TaskingMinionObject> list = new List<TaskingMinionObject>();
				IList<ActionableObject> list2 = new List<ActionableObject>();
				list2.Add(this);
				for (int i = 0; i < stations; i++)
				{
					TaskingMinionObject byRouteSlot = GetByRouteSlot(i);
					if (byRouteSlot == null)
					{
						return false;
					}
					list.Add(byRouteSlot);
					list2.Add(byRouteSlot.Minion);
				}
				SyncAction action = new SyncAction(list2, logger);
				SyncAction action2 = new SyncAction(list2, logger);
				int hashAnimationState = GetHashAnimationState("Base Layer.Gag");
				for (int j = 0; j < stations; j++)
				{
					TaskingMinionObject taskingMinionObject = list[j];
					MinionObject minion = taskingMinionObject.Minion;
					minion.EnqueueAction(action);
					minion.EnqueueAction(new SetAnimatorAction(minion, null, logger, OnlyStateEnabled("OnGag")));
					string stateName = "Base Layer.Gag_Pos" + (taskingMinionObject.RoutingIndex + 1);
					int hashAnimationState2 = GetHashAnimationState(stateName);
					SkipToTime skipToTime = new SkipToTime(0, hashAnimationState2, GetCurrentAnimationTimeForState(hashAnimationState));
					minion.EnqueueAction(new SkipToTimeAction(minion, skipToTime, logger));
					minion.EnqueueAction(action2);
					minion.EnqueueAction(new SetAnimatorAction(minion, null, logger, OnlyStateEnabled("OnLoop")));
				}
				EnqueueAction(action);
				EnqueueAction(new TriggerBuildingAnimationAction(this, OnlyStateEnabled("OnGag"), logger));
				EnqueueAction(new WaitForMecanimStateAction(this, GetHashAnimationState("Base Layer.Gag"), logger));
				EnqueueAction(action2);
				EnqueueAction(new TriggerBuildingAnimationAction(this, OnlyStateEnabled("OnLoop"), logger, "Base Layer.Loop"));
				return true;
			}
			return false;
		}

		internal bool IsGagAnimationPlaying()
		{
			return IsInAnimatorState(GetHashAnimationState("Base Layer.Gag"));
		}

		internal void StopGagAnimation(bool resetTrigger = false)
		{
			for (int i = 0; i < stations; i++)
			{
				TaskingMinionObject byRouteSlot = GetByRouteSlot(i);
				if (byRouteSlot != null)
				{
					MinionObject minion = byRouteSlot.Minion;
					if (resetTrigger)
					{
						minion.ClearActionQueue();
						minion.ExecuteAction(new SetAnimatorAction(minion, null, logger, OnlyStateEnabled("OnLoop")));
					}
					else
					{
						minion.EnqueueAction(new SetAnimatorAction(minion, null, logger, OnlyStateEnabled("OnStop")), true);
						minion.EnqueueAction(new SetAnimatorAction(minion, null, logger, OnlyStateEnabled("OnLoop")));
					}
				}
			}
			if (resetTrigger)
			{
				ClearActionQueue();
				ExecuteAction(new TriggerBuildingAnimationAction(this, OnlyStateEnabled("OnLoop"), logger, "Base Layer.Loop"));
			}
			else
			{
				EnqueueAction(new TriggerBuildingAnimationAction(this, OnlyStateEnabled("OnStop"), logger), true);
				EnqueueAction(new TriggerBuildingAnimationAction(this, OnlyStateEnabled("OnLoop"), logger, "Base Layer.Loop"));
			}
		}

		public override void UntrackChild(int minionId, TaskableBuilding building)
		{
			base.UntrackChild(minionId, building);
			if (!building.IsEligibleForGag())
			{
				return;
			}
			foreach (Animator animator in animators)
			{
				if (animator.GetBool("OnGag"))
				{
					StopGagAnimation(true);
				}
			}
		}
	}
}
