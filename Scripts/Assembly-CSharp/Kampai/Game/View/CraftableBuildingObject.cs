using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class CraftableBuildingObject : AnimatingBuildingObject, IRequiresBuildingScaffolding
	{
		private readonly List<Material> craftingMaterials = new List<Material>();

		private readonly List<GoTween> tweenList = new List<GoTween>();

		private bool isHighlighted;

		public float highlightedDuration = 0.25f;

		private void Start()
		{
			for (int i = 0; i < base.objectRenderers.Length; i++)
			{
				Renderer renderer = base.objectRenderers[i];
				for (int j = 0; j < renderer.materials.Length; j++)
				{
					Material material = renderer.materials[j];
					if ((!material.HasProperty(GameConstants.ShaderProperties.Blend.DstBlend) || (int)material.GetFloat(GameConstants.ShaderProperties.Blend.DstBlend) != 1) && !material.name.Contains("Platform") && !material.name.Contains("Shadow") && material.HasProperty(GameConstants.ShaderProperties.Procedural.BlendedColor))
					{
						craftingMaterials.Add(material);
					}
				}
			}
		}

		internal void SetWorking()
		{
			if (!IsInAnimatorState(GetHashAnimationState("Base Layer.Loop")))
			{
				if (IsInAnimatorState(GetHashAnimationState("Base Layer.Wait")))
				{
					EnqueueAction(new TriggerBuildingAnimationAction(this, OnlyStateEnabled("OnStop"), logger));
				}
				EnqueueAction(new TriggerBuildingAnimationAction(this, "OnLoop", logger));
			}
		}

		internal void SetWait()
		{
			if (IsInAnimatorState(GetHashAnimationState("Base Layer.Loop")))
			{
				EnqueueAction(new TriggerBuildingAnimationAction(this, OnlyStateEnabled("OnWait"), logger));
			}
		}

		internal void SetIdle()
		{
			if (IsInAnimatorState(GetHashAnimationState("Base Layer.Loop")) || IsInAnimatorState(GetHashAnimationState("Base Layer.Wait")))
			{
				EnqueueAction(new TriggerBuildingAnimationAction(this, OnlyStateEnabled("OnStop"), logger));
			}
		}

		public void EnableHighLightBuilding(bool canCraftRecipe)
		{
			if (!isHighlighted)
			{
				ClearMaterialTweens();
				for (int i = 0; i < craftingMaterials.Count; i++)
				{
					Material self = craftingMaterials[i];
					tweenList.Add(self.colorTo(highlightedDuration, (!canCraftRecipe) ? GameConstants.Building.INVALID_CRAFTING_RECIPE_DROP : GameConstants.Building.VALID_CRAFTING_RECIPE_DROP, "_BlendedColor"));
				}
				isHighlighted = true;
			}
		}

		public void DisableHighLightBuilding()
		{
			if (isHighlighted)
			{
				ClearMaterialTweens();
				for (int i = 0; i < craftingMaterials.Count; i++)
				{
					Material self = craftingMaterials[i];
					tweenList.Add(self.colorTo(highlightedDuration, Color.clear, "_BlendedColor"));
					isHighlighted = false;
				}
			}
		}

		private void ClearMaterialTweens()
		{
			for (int i = 0; i < tweenList.Count; i++)
			{
				tweenList[i].destroy();
			}
			tweenList.Clear();
		}

		public override void SetState(BuildingState newState)
		{
			base.SetState(newState);
			if (buildingState != newState)
			{
				buildingState = newState;
				switch (newState)
				{
				case BuildingState.Working:
				case BuildingState.HarvestableAndWorking:
					SetWorking();
					break;
				case BuildingState.Inactive:
				case BuildingState.Idle:
					SetIdle();
					break;
				case BuildingState.Harvestable:
					SetWait();
					break;
				}
			}
		}
	}
}
