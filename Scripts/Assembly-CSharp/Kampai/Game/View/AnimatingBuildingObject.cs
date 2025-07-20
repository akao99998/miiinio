using System;
using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class AnimatingBuildingObject : RoutableBuildingObject
	{
		protected Dictionary<int, RuntimeAnimatorController> buildingControllers;

		private Renderer[] renderers;

		private Dictionary<string, int> hashCache = new Dictionary<string, int>();

		protected BuildingState buildingState;

		internal override void Init(Building building, IKampaiLogger logger, IDictionary<string, RuntimeAnimatorController> controllers, IDefinitionService definitionService)
		{
			base.Init(building, logger, controllers, definitionService);
			AnimatingBuildingDefinition animatingBuildingDefinition = building.Definition as AnimatingBuildingDefinition;
			if (animatingBuildingDefinition == null)
			{
				logger.Fatal(FatalCode.BV_ILLEGAL_ANIMATING_DEFINITION, building.Definition.ID.ToString());
			}
			buildingControllers = new Dictionary<int, RuntimeAnimatorController>();
			if (this is MignetteBuildingObject)
			{
				return;
			}
			foreach (BuildingAnimationDefinition animationDefinition in animatingBuildingDefinition.AnimationDefinitions)
			{
				if (controllers.ContainsKey(animationDefinition.BuildingController))
				{
					buildingControllers.Add(animationDefinition.CostumeId, controllers[animationDefinition.BuildingController]);
				}
			}
			if (!buildingControllers.ContainsKey(-1))
			{
				logger.Fatal(FatalCode.BV_NO_DEFAULT_ANIMATION_CONTROLLER, animatingBuildingDefinition.ID.ToString());
			}
			animators = InitAnimators();
			if (building.IsBuildingRepaired())
			{
				if (!(this is VillainLairResourcePlotObjectView))
				{
					EnqueueAction(new ControllerBuildingAction(this, buildingControllers[-1], logger));
				}
				renderers = base.gameObject.GetComponentsInChildren<Renderer>();
			}
		}

		protected bool HasVisibleRenderers()
		{
			if (renderers == null)
			{
				return false;
			}
			for (int i = 0; i < renderers.Length; i++)
			{
				if (renderers[i].isVisible)
				{
					return true;
				}
			}
			return false;
		}

		protected virtual List<Animator> InitAnimators()
		{
			animators = new List<Animator>();
			for (int i = 0; i < base.transform.childCount; i++)
			{
				Transform child = base.transform.GetChild(i);
				if (child.name.Contains("LOD"))
				{
					Animator component = child.GetComponent<Animator>();
					if (component != null)
					{
						animators.Add(component);
					}
				}
			}
			return animators;
		}

		public virtual void StartAnimating()
		{
			if (IsInAnimatorState(GetHashAnimationState("Base Layer.Wait")))
			{
				EnqueueAction(new TriggerBuildingAnimationAction(this, "OnStop", logger));
			}
			EnqueueAction(new TriggerBuildingAnimationAction(this, "OnLoop", logger));
		}

		protected Func<float> GetCurrentAnimationTimeForState(int stateHash)
		{
			return () => IsInAnimatorState(stateHash) ? GetCurrentAnimationTime() : 0f;
		}

		protected float GetCurrentAnimationTime()
		{
			if (GetAnimatorStateInfo(0).HasValue)
			{
				return GetAnimatorStateInfo(0).Value.normalizedTime;
			}
			return 0f;
		}

		protected int GetHashAnimationState(string stateName, int index = -1)
		{
			if (index > -1)
			{
				stateName += index;
			}
			if (!hashCache.ContainsKey(stateName))
			{
				hashCache.Add(stateName, Animator.StringToHash(stateName));
			}
			return hashCache[stateName];
		}

		internal void EnableAnimators(bool enable)
		{
			foreach (Animator animator in animators)
			{
				animator.enabled = enable;
			}
		}

		protected Dictionary<string, object> OnlyStateEnabled(string enabled)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			foreach (string aLL_TRIGGER in GameConstants.Animation.ALL_TRIGGERS)
			{
				dictionary.Add(aLL_TRIGGER, aLL_TRIGGER.Equals(enabled));
			}
			return dictionary;
		}

		public virtual void SetState(BuildingState newState)
		{
			if (newState == BuildingState.Idle)
			{
				base.StopBuildingAudioInIdleStateSignal.Dispatch();
			}
		}

		protected override void Show()
		{
			base.Show();
			StartAnimating();
		}
	}
}
